using System.Collections.Generic;

namespace stubby.ServiceModel
{
    public class ResponseModel
    {
        /// <summary>
        /// The HTTP/1.1 Status code (100-599).
        /// </summary>
        public ushort Status { get; set; }

        /// <summary>
        /// Name/Value Collection of HTTP Response Headers.
        /// </summary>
         public List<KeyValuePair<string,string>> Headers { get; set; }

        /// <summary>
        /// The time in milliseconds to wait before responding.
        /// </summary>
        public ulong Latency { get; set; }

        /// <summary>
        /// The content body of the response. If File is supplied and the file exists, this property is ignored.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// A filepath whose file contains the content of the response body. If defined, overrides the Body property.
        /// </summary>
        public string File { get; set; }
    }
}
