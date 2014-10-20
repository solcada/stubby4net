using System.Collections.Generic;
using System.Collections.Specialized;

namespace stubby.ServiceModel.Mapping
{
    public static class NameValueCollectionMapper
    {
        public static List<KeyValuePair<string, string>> ToList(this NameValueCollection col)
        {
            List<KeyValuePair<string, string>> dict = new List<KeyValuePair<string, string>>();
            foreach (var k in col.AllKeys)
            {
                var keyvaluepair = new KeyValuePair<string,string>(k, col[k]);
                dict.Add(keyvaluepair);
            }
            return dict;
        }
    }
}
