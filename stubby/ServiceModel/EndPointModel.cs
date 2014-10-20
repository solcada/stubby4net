using System.Collections.Generic;
using stubby.Domain;

namespace stubby.ServiceModel
{
    public class EndPointModel
    {
        public RequestModel Request { get; set; }

        public List<ResponseModel> Responses { get; set; }
    }
}
