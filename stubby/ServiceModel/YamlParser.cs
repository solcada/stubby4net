using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using stubby.Domain;
using stubby.ServiceModel.Mapping;
using YamlDotNet.RepresentationModel;
using YamlDotNet.RepresentationModel.Serialization;

namespace stubby.ServiceModel {

    internal static class YamlParser {
        private const string CurrentDirectory = ".";
        private static string _fileDirectory = CurrentDirectory;

        public static List<Endpoint> FromFile(string filename) {
            if(string.IsNullOrWhiteSpace(filename))
                return new List<Endpoint>();

            _fileDirectory = Path.GetDirectoryName(filename);

            var yaml = new YamlStream();

            using(var streamReader = new StreamReader(filename)) {
                yaml.Load(streamReader);
            }

            var endpointsModel = Parse(yaml);

            var endPoints = EndpointMapper.Map(endpointsModel);

            return endPoints;
        }

        public static void ToFile(string filename, IList<Endpoint> endpoints)
        {
            _fileDirectory = Path.GetDirectoryName(filename);

            List<EndpointModel> endpointsToSerialize = new List<EndpointModel>();
            foreach (var endpoint in endpoints)
            {
                var endpointToSerialize = new EndpointModel
                {
                    Request = RequestMapper.Map(endpoint.Request),
                    Responses = ResponseMapper.Map(endpoint.Responses)
                };

                endpointsToSerialize.Add(endpointToSerialize);
            }

            var serializer = new Serializer(SerializationOptions.JsonCompatible);
            using (TextWriter writer = File.CreateText(filename))
            {
                serializer.Serialize(writer, endpointsToSerialize);
            }
        }

        public static List<Endpoint> FromString(string data) {
            _fileDirectory = CurrentDirectory;

            var yaml = new YamlStream();

            using(var streamReader = new StringReader(data)) {
                yaml.Load(streamReader);
            }

            var endpointsModel = Parse(yaml);
            return EndpointMapper.Map(endpointsModel);
        }

        private static List<EndpointModel> Parse(YamlStream yaml) {
            var yamlEndpoints = (YamlSequenceNode) yaml.Documents[0].RootNode;

            return (from YamlMappingNode yamlEndpoint in yamlEndpoints select ParseEndpoint(yamlEndpoint)).ToList();
        }

        private static EndpointModel ParseEndpoint(YamlMappingNode yamlEndpoint) {
            var endpoint = new EndpointModel();

            foreach (var requestResponse in yamlEndpoint.Children)
            {
                switch (requestResponse.Key.ToString().ToLowerInvariant())
                {
                    case "request":
                    {
                        endpoint.Request = ParseRequest((YamlMappingNode) requestResponse.Value);
                        break;
                    }
                    case "response":
                    {
                        endpoint.Responses = ParseResponses(requestResponse);
                        break;
                    }
                    case "responses":
                    {
                        endpoint.Responses = ParseResponses(requestResponse);
                        break;
                    }
                }
            }

            return endpoint;
        }

        private static RequestModel ParseRequest(YamlMappingNode yamlRequest) {
            var request = new RequestModel();

            foreach(var property in yamlRequest) {
                switch(property.Key.ToString().ToLowerInvariant()) {
                    case "url":
                        {
                            request.Url = ParseString(property);
                            break;
                        }
                    case "method":
                        {
                            request.Method = ParseMethod(property);
                            break;
                        }
                    case "file":
                        {
                            request.File = ParseFile(property);
                            break;
                        }
                    case "post":
                        {
                            request.Post = ParseString(property);
                            break;
                        }
                    //case "query":
                    //    {
                    //        request.Query = ParseCollection(property);
                    //        break;
                    //    }
                    //case "headers":
                    //    {
                    //        request.Headers = ParseCollection(property, false);
                    //        break;
                    //    }
                }
            }
            return request;
        }

        private static string ParseString(KeyValuePair<YamlNode, YamlNode> property) {
            return property.Value.ToString().TrimEnd(new[]
            {
                ' ',
                '\t',
                '\n',
                '\r'
            });
        }

        private static string ParseFile(KeyValuePair<YamlNode, YamlNode> property) {
            return Path.GetFullPath(Path.Combine(_fileDirectory, property.Value.ToString()));
        }

        private static List<string> ParseMethod(KeyValuePair<YamlNode, YamlNode> yamlMethod) {
            var methods = new List<string>();

            if(yamlMethod.Value.GetType() == typeof(YamlScalarNode))
                methods.Add(yamlMethod.Value.ToString().ToUpper());
            else if(yamlMethod.Value.GetType() == typeof(YamlSequenceNode))
                methods.AddRange(from method in (YamlSequenceNode) yamlMethod.Value select method.ToString().ToUpper());

            return methods;
        }

        private static List<ResponseModel> ParseResponses(KeyValuePair<YamlNode, YamlNode> yamlResponse){
            if(yamlResponse.Value.GetType() == typeof(YamlSequenceNode))
                return (from response in (YamlSequenceNode) yamlResponse.Value select ParseResponse((YamlMappingNode) response)).ToList();            else
            return new List<ResponseModel> {ParseResponse((YamlMappingNode) yamlResponse.Value)};
        }

        private static ResponseModel ParseResponse(YamlMappingNode yamlResponse) {
            var response = new ResponseModel();

            foreach(var property in yamlResponse) {
                switch(property.Key.ToString().ToLowerInvariant()) {
                    case "status":
                        {
                            response.Status = ushort.Parse(property.Value.ToString());
                            break;
                        }
                    //case "headers":
                    //    {
                    //        response.Headers = ParseCollection(property, false);
                    //        break;
                    //    }
                    case "latency":
                        {
                            response.Latency = ulong.Parse(property.Value.ToString());
                            break;
                        }
                    case "body":
                        {
                            response.Body = ParseString(property);
                            break;
                        }
                    case "file":
                        {
                            response.File = ParseFile(property);
                            break;
                        }
                }
            }

            return response;
        }

        private static List<KeyValuePair<string,string>> ParseCollection (KeyValuePair<YamlNode, YamlNode> property, bool caseSensitive = true)
        {
            var keyValuePairs = (YamlMappingNode)property.Value;
            var collection = new List<KeyValuePair<string, string>>();

            foreach (var keyValuePair in keyValuePairs)
            {
                var key = keyValuePair.Key.ToString();
                var value = keyValuePair.Value.ToString();

                if (property.Key.ToString().Equals("headers") &&
                    key.Equals("authorization", StringComparison.InvariantCultureIgnoreCase) && value.Contains(":"))
                {
                    value = value.Replace("Basic ", "");
                    value = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
                }

                collection.Add(new KeyValuePair<string, string>(caseSensitive ? key : key.ToLower(), value));
            }

            return collection;
        }

        private static NameValueCollection ParseCollection_(KeyValuePair<YamlNode, YamlNode> property, bool caseSensitive = true) {
            var keyValuePairs = (YamlMappingNode) property.Value;
            var collection = new NameValueCollection();

            foreach(var keyValuePair in keyValuePairs) {
                var key = keyValuePair.Key.ToString();
                var value = keyValuePair.Value.ToString();

                if(property.Key.ToString().Equals("headers") &&
                    key.Equals("authorization", StringComparison.InvariantCultureIgnoreCase) && value.Contains(":")) {
                    value = value.Replace("Basic ", "");
                    value = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
                }

                collection.Add(caseSensitive ? key : key.ToLower(), value);
            }

            return collection;
        }
    }
}