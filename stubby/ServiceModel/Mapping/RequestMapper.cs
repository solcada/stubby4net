using stubby.Domain;

namespace stubby.ServiceModel.Mapping
{
    public class RequestMapper
    {
        public static RequestModel Map(Request request)
        {
            var requestModel = new RequestModel
            {
                File = request.File,
                Url = request.Url,
                Method = request.Method,
                Query = request.Query.ToList(),
                Headers = request.Headers.ToList(),
                Post = request.Post
            };

            return requestModel;
        }
    }
}
