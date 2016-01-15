using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace Microsoft.Azure.IoT.Studio.Device
{
    internal class FilterDesc : IEquatable<FilterDesc>
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public JToken Parameters { get; private set; }

        private FilterDesc(string name, string type, JToken parameters)
        {
            Name = name;
            Type = type;
            Parameters = parameters;
        }

        static public FilterDesc Create(JToken filter)
        {
            if (filter == null)
            {
                return null;
            }
            else
            {
                string name = filter.Value<string>("name");
                if (string.IsNullOrEmpty(name))
                {
                    throw new ApplicationException("Missing filter name");
                }

                string type = filter.Value<string>("type");
                if (string.IsNullOrEmpty(type))
                {
                    throw new ApplicationException("Missing filter type");
                }

                JToken parameters = filter["parameters"];
                if (parameters == null)
                {
                    parameters = new JObject();
                }

                //Apply default parameters
                JToken defaultParameters;
                if (GlobalSettings.Instance.DefaultParameters.TryGetValue(type, out defaultParameters))
                {
                    foreach (JProperty child in defaultParameters.Children())
                    {
                        if (parameters[child.Name] == null)
                        {
                            parameters[child.Name] = child.Value.DeepClone();
                        }
                    }
                }

                return new FilterDesc(name, type, parameters);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", Name, Type);
        }

        #region Equals implementation
        public override bool Equals(object obj)
        {
            return base.Equals(obj as FilterDesc);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^
                Type.GetHashCode() ^
                Parameters.GetHashCode();
        }

        public bool Equals(FilterDesc other)
        {
            if (other == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return Name.Equals(other.Name) &&
                Type.Equals(other.Type) &&
                JToken.DeepEquals(Parameters, other.Parameters);
        }
        #endregion
    }
}
