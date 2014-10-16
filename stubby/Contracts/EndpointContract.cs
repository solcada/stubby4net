using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using stubby.Domain;

namespace stubby.Contracts {

    internal static class EndpointContract {
        public static Endpoint Verify(Endpoint endpoint) {
            endpoint = endpoint ?? new Endpoint();

            return new Endpoint
            {
                Request = VerifyRequest(endpoint.Request),
                Responses = VerifyResponses(endpoint.Responses)
            };
        }

        private static Request VerifyRequest(Request request) {
            request = request ?? new Request();

            return new Request
            {
                Url = VerifyUrl(request.Url),
                Query = request.Query ?? new NameValueCollection(),
                Headers = request.Headers ?? new NameValueCollection(),
                Post = request.Post,
                File = request.File,
                Method = VerifyMethod(request.Method)
            };
        }

        private static string VerifyUrl(string url) {
            if(string.IsNullOrWhiteSpace(url))
                return "/";
            if(!url.StartsWith("/"))
                return "/" + url;
            return url;
        }

        private static IList<string> VerifyMethod(ICollection<string> methods) {
            IList<string> verified = new List<string>();

            if(methods == null || methods.Count.Equals(0)) {
                verified.Add("GET");
                return verified;
            }

            foreach(var method in methods.Where(method => !string.IsNullOrWhiteSpace(method)))
                verified.Add(method.ToUpper());

            return verified;
        }

        private static List<Response> VerifyResponses(IList<Response> responses){
            if(responses == null || responses.Count == 0)
                return new List<Response> { VerifyResponse(null) };
            return (from response in responses select VerifyResponse(response)).ToList();
        }

        private static Response VerifyResponse(Response response) {
            response = response ?? new Response();

            return new Response
            {
                Status = VerifyStatus(response.Status),
                Headers = response.Headers ?? new NameValueCollection(),
                Body = response.Body,
                Latency = response.Latency,
                File = response.File
            };
        }

        private static ushort VerifyStatus(ushort status) {
            if(status < 100 || status >= 600)
                return 200;

            return status;
        }
    }
}