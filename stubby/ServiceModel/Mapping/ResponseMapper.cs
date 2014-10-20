using System.Collections.Generic;
using System.Linq;
using stubby.Domain;

namespace stubby.ServiceModel.Mapping
{
    class ResponseMapper
    {
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
    }
}
