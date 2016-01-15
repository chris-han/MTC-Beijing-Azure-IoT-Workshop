using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.IoT.Studio.Device
{
    internal class GlobalSettings
    {
        static private object _lock = new object();
        static private GlobalSettings _instance;
        public bool Updated { get; private set; }
        public IDictionary<string, string> NamespaceAliases { get; private set; }
        public IDictionary<string, JToken> DefaultParameters { get; private set; }
        static public GlobalSettings Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new GlobalSettings();
                    }
                }

                return _instance;
            }
        }

        private GlobalSettings()
        {
            Updated = false;
            NamespaceAliases = new SortedDictionary<string, string>();
            DefaultParameters = new SortedDictionary<string, JToken>();
        }

        public void Load(JToken token)
        {
            var namespaceAliases = new SortedDictionary<string, string>();
            var defaultParameters = new SortedDictionary<string, JToken>();

            var aliases = token["namespaceAliases"] as JArray;
            if (aliases != null)
            {
                foreach (var alias in aliases)
                {
                    var key = alias.Value<string>("alias");
                    var value = alias.Value<string>("namespace");
                    namespaceAliases.Add(key, value);
                }
            }

            var types = token["defaultParameters"] as JArray;
            if (types != null)
            {
                foreach (var type in types)
                {
                    var key = type.Value<string>("type");
                    var value = type["parameters"];
                    defaultParameters.Add(key, value);
                }
            }

            Updated = !NamespaceAliases.SequenceEqual(namespaceAliases) ||
                !DefaultParameters.SequenceEqual(defaultParameters, new ParameterComparer());
            NamespaceAliases = namespaceAliases;
            DefaultParameters = defaultParameters;
        }
    }

    internal class ParameterComparer : IEqualityComparer<KeyValuePair<string, JToken>>
    {
        public bool Equals(KeyValuePair<string, JToken> x, KeyValuePair<string, JToken> y)
        {
            return x.Key == y.Key && JToken.DeepEquals(x.Value, y.Value);
        }

        public int GetHashCode(KeyValuePair<string, JToken> obj)
        {
            return obj.Key.GetHashCode() ^ obj.Value.GetHashCode();
        }
    }
}
