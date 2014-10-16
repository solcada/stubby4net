﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mime;
using stubby.CLI;
using stubby.Domain;
using utils = stubby.Portals.PortalUtils;

namespace stubby.Portals {

    internal class Stubs : IDisposable {
        private const string Name = "stubs";
        private const string UnregisteredEndoint = "is not a registered endpoint";
        private const string UnexpectedError = "unexpectedtly generated a server error";
	    
		private readonly string _siteToCapture;
        private readonly string _locationToDownloadSite;
        private readonly EndpointDb _endpointDb;
        private readonly HttpListener _listener;

        public Stubs(EndpointDb endpointDb, string siteToCapture, string locationToDownloadSite) : this(endpointDb, new HttpListener())
        {
            _siteToCapture = siteToCapture;
            _locationToDownloadSite = locationToDownloadSite;
        }

        public Stubs(EndpointDb endpointDb, HttpListener listener) {
            _endpointDb = endpointDb;
            _listener = listener;
        }

        public void Dispose() {
            _listener.Stop();
        }

        public void Stop() {
            _listener.Stop();
        }

        public void Start(string location, uint port, uint httpsPort) {
            _listener.Prefixes.Add(utils.BuildUri(location, port));
            _listener.Prefixes.Add(utils.BuildUri(location, httpsPort, true));

            _listener.Start();
            _listener.BeginGetContext(AsyncHandler, _listener);

            utils.PrintListening(Name, location, port);
            utils.PrintListening(Name, location, httpsPort);
        }

        private void ResponseHandler(HttpListenerContext context) {
            
			var found = FindEndpoint(context);

			// Download, Return and Amend Schema (if switched on).
            if (found == null && _siteToCapture != null)
			{
				var webClient = new WebClient();

                var pathToSaveResponse = _locationToDownloadSite + context.Request.Url.AbsolutePath;
				
				var hasExtension = Path.HasExtension(context.Request.Url.AbsolutePath);
				if (!hasExtension)
				{
					pathToSaveResponse = pathToSaveResponse.TrimEnd('/');
					pathToSaveResponse = pathToSaveResponse + ".html";					
				}

				var file = new FileInfo(pathToSaveResponse);
				if (!file.Exists)
				{
                    var realPath = _siteToCapture + context.Request.Url.AbsolutePath;
                    file.Directory.Create();
                    webClient.DownloadFile(realPath, pathToSaveResponse);

                    // Add New EndPoint
				    var request = new Request()
				    {
                        Url = context.Request.Url.ToString(),
				    };

				    var response = new Response()
				    {
				        File = pathToSaveResponse,
				    };

				    var responses = new[]
				    {
                        response
				    };

				    var endpoint = new Endpoint()
				    {
                        Request = request,
                        Responses = responses
				    };
				    _endpointDb.Insert(endpoint);

				    // Serialize to a file
                    YamlParser.ToFile(_locationToDownloadSite + @"\site.yaml", _endpointDb.Fetch());

				}

				found = new Response {File = pathToSaveResponse, Status = (int) HttpStatusCode.OK};
			}

            if(found == null) {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                utils.PrintOutgoing(Name, context, UnregisteredEndoint);
                return;
            }

            if(found.Latency > 0)
                System.Threading.Thread.Sleep((int) found.Latency);

            context.Response.StatusCode = found.Status;
            context.Response.Headers.Add(found.Headers);
            WriteResponseBody(context, found);
            utils.PrintOutgoing(Name, context);
        }

        private Response FindEndpoint(HttpListenerContext context) {

            var incoming = new Endpoint
            {
                Request = {
               Url = context.Request.Url.AbsolutePath,
               Method = new List<string> {context.Request.HttpMethod.ToUpper()},
               Headers = CreateNameValueCollection(context.Request.Headers, caseSensitiveKeys: false),
               Query = CreateNameValueCollection(context.Request.QueryString),
               Post = utils.ReadPost(context.Request)
            }
            };

            var found = _endpointDb.Find(incoming);
            return found;
        }

        private static void WriteResponseBody(HttpListenerContext context, Response found)
        {
            string body;

            try
            {
                var extension = Path.GetExtension(found.File).ToLower();
                var imageFormat = GetImageFormat(extension);
                if (imageFormat != null)
                {
                    using (var image = new Bitmap(found.File))
                    {
                        utils.WriteResponse(context, image, imageFormat);
                    }
                }
                else
                {
                    body = File.ReadAllText(found.File).TrimEnd(new[]
                    {
                        ' ',
                        '\n',
                        '\r',
                        '\t'
                    });
                    utils.WriteBody(context, body);
                }
            }
            catch (Exception)
            {
                body = found.Body;
                utils.WriteBody(context, body);
            }

        }

        private static ImageFormat GetImageFormat(string extension)
        {
            switch (extension)
            {
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".jpg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".gif":
                    return ImageFormat.Gif;
                case ".ico":
                    return ImageFormat.Icon;
            }

            return null;
        }


        private static NameValueCollection CreateNameValueCollection(NameValueCollection collection, bool caseSensitiveKeys = true) {
            var newCollection = new NameValueCollection();

            foreach(var key in collection.AllKeys) {
                newCollection.Add(caseSensitiveKeys ? key : key.ToLower(), collection.Get(key));
            }

            return newCollection;
        }

        private void AsyncHandler(IAsyncResult result) {
            HttpListenerContext context;
            try {
                context = _listener.EndGetContext(result);
            } catch(HttpListenerException) {
                return;
            }

            utils.PrintIncoming(Name, context);
            utils.SetServerHeader(context);

            try {
                ResponseHandler(context);
            } catch {
                utils.SetStatus(context, HttpStatusCode.InternalServerError);
                utils.PrintOutgoing(Name, context, UnexpectedError);
            }

            context.Response.Close();
            _listener.BeginGetContext(AsyncHandler, _listener);
        }
    }
}