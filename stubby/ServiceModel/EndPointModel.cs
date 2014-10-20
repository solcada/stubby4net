using System.Collections.Generic;

namespace stubby.ServiceModel
{
    public class EndpointModel
    {
        public RequestModel Request { get; set; }

        public List<ResponseModel> Responses { get; set; }
    }
}
