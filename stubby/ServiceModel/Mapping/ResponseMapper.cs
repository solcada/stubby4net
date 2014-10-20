using System.Collections.Generic;
using System.Linq;
using stubby.Domain;

namespace stubby.ServiceModel.Mapping
{
    public class ResponseMapper
    {
        public static string LocationToDownloadSite { get; set; }

        public static List<ResponseModel> Map(List<Response> responses)
        {
            var responsesModel = new List<ResponseModel>();

            foreach (var response in responses)
            {
                var responseToSerialize = new ResponseModel
                {
                    Body = response.Body,
                    File = response.File,
                    Headers = response.Headers.ToList(),
                    Latency = response.Latency,
                    Status = response.Status
                };

                responsesModel.Add(responseToSerialize);
            }

            return responsesModel;
        }


        public static ResponseModel Map(Response response)
        {
            if (!response.File.Contains(LocationToDownloadSite))
            {
                response.File = LocationToDownloadSite + response.File;
            }

            var responseToSerialize = new ResponseModel
            {
                Body = response.Body,
                File = response.File,
                Headers = response.Headers.ToList(),
                Latency = response.Latency,
                Status = response.Status
            };

            return responseToSerialize;
        }

        public static Response Map(ResponseModel response)
        {
            if (response.File.Contains(LocationToDownloadSite))
            {
                response.File = response.File.Substring(LocationToDownloadSite.Length);
            }

            var responseToSerialize = new Response
            {
                Body = response.Body,
                File = response.File,
                Headers = response.Headers.ToNameValueCollection(),
                Latency = response.Latency,
                Status = response.Status
            };

            return responseToSerialize;
        }

        
    }
}
