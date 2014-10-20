using System.Collections.Generic;
using System.Linq;
using stubby.Domain;

namespace stubby.ServiceModel.Mapping
{
    public class EndpointMapper
    {
        public static List<Endpoint> Map(List<EndpointModel> endpointsModel)
        {
            var endPoints = new List<Endpoint>();

            foreach (var endpointModel in endpointsModel)
            {
                var endPoint = new Endpoint();
                endPoint.Request = RequestMapper.Map(endpointModel.Request);
                endPoint.Responses = endpointModel.Responses.Select(ResponseMapper.Map).ToList();
                endPoints.Add(endPoint);
            }

            return endPoints;
        }
    }
}
