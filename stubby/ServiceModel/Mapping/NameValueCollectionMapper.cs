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

        public static NameValueCollection ToNameValueCollection(this List<KeyValuePair<string, string>> col)
        {
            NameValueCollection dict = new NameValueCollection();
            if (col == null)
            {
                return dict;
            }

            foreach (var k in col)
            {
                dict.Add(k.Key , k.Value);
            }

            return dict;
        }
    }
}
