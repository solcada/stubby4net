using System.Collections.Generic;

namespace stubby.ServiceModel
{
    public class RequestModel
    {
        public string Url { get; set; }

        /// <summary>
        /// A list of acceptable HTTP verbs such as GET or POST. Defaults to GET.
        /// </summary>
        public IList<string> Method { get; set; }

        /// <summary>
        /// Name/Value headers that incoming requests must at least possess.
        /// </summary>
        public List<KeyValuePair<string,string>> Headers { get; set; }

        /// <summary>
        /// A Name/Value collection of URI Query parameters that must be present.
        /// </summary>
        public List<KeyValuePair<string, string>> Query { get; set; }

        /// <summary>
        /// The post body contents of the incoming request. If File is specified and exists upon request, this value is ignored.
        /// </summary>
        public string Post { get; set; }

        /// <summary>
        /// A filepath whose file contains the incoming request body to match against. If the file cannot be found, Post is used instead.
        /// </summary>
        public string File { get; set; }
    }
}
