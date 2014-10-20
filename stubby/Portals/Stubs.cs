﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using stubby.CLI;
using stubby.Domain;
using stubby.ServiceModel;
using stubby.ServiceModel.Mapping;
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
            ResponseMapper.LocationToDownloadSite = locationToDownloadSite;
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

        private void ResponseHandler(HttpListenerContext context)
        {
            var rar = context.Request.Url;

			Response found = FindEndpoint(context);

			// Download, Return and Amend Schema (if switched on).
            if (found == null && _siteToCapture != null)
			{
				var webClient = new WebClient();

			    var localFileDownloadedName = "";
                var relativeFileName = "";

                var hasExtension = Path.HasExtension(context.Request.Url.AbsolutePath);
			    if (!hasExtension)
			    {
			        var parameterHash = "";
			        if (!string.IsNullOrEmpty(context.Request.Url.Query))
			        {
			            parameterHash = Hash(context.Request.Url.Query);
			        }

			        localFileDownloadedName = _locationToDownloadSite + context.Request.Url.AbsolutePath.TrimStart('/').TrimEnd('/') + parameterHash + ".html";
			        relativeFileName = context.Request.Url.AbsolutePath.TrimStart('/').TrimEnd('/') + ".html";
			    }
			    else
			    {
			        localFileDownloadedName = _locationToDownloadSite + context.Request.Url.AbsolutePath;
			        relativeFileName = _locationToDownloadSite + context.Request.Url.AbsolutePath;
			    }

			    var file = new FileInfo(localFileDownloadedName);
				if (!file.Exists)
				{
                    var realPath = _siteToCapture + context.Request.Url.AbsolutePath;
                    file.Directory.Create();
                    webClient.DownloadFile(realPath, localFileDownloadedName);

                    // Add New EndPoint
				    var request = new Request()
				    {
                        Url = context.Request.Url.PathAndQuery,
				    };

				    var response = new Response()
				    {
                        File = localFileDownloadedName,
				    };

				    var responses = new List<Response>
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
                    YamlParser.ToFile(_locationToDownloadSite + @"\site.json", _endpointDb.Fetch());

				}

                found = new Response { File = localFileDownloadedName, Status = (int)HttpStatusCode.OK };
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

        private void WriteResponseBody(HttpListenerContext context, Response found)
        {
            string body;

            try
            {

                if (!found.File.Contains(_locationToDownloadSite))
                {
                    found.File = _locationToDownloadSite + found.File;
                }

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


        private string Hash(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);

            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}