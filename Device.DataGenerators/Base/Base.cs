using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.DataGenerator
{
    public abstract class Base
    {
        protected Random _rand = RandExtension.CreateByTask();

        public void Reset(JToken parameters)
        {
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if (!DataGeneratorParameterAttribute.IsAttributed(propertyInfo))
                {
                    continue;
                }

                var valueToken = parameters.SelectToken(propertyInfo.Name);
                if (valueToken == null)
                {
                    throw new ApplicationException(string.Format("Unassigned parameter {0}", propertyInfo.Name));
                }

                object val = valueToken.ToObject(propertyInfo.PropertyType);
                propertyInfo.SetMethod.Invoke(this, new object[] { val });
            }

            OnInitialize();
        }

        public void Reset(string parameters)
        {
            Reset(JsonConvert.DeserializeObject(parameters) as JToken);
        }

        abstract public void OnInitialize();

        abstract public Data Read();

        public override string ToString()
        {
            return GetType().Name;
        }

        public Range Range { get; protected set; }
    }

    public class DataGeneratorParameterAttribute : Attribute
    {
        static public bool IsAttributed(PropertyInfo propertyInfo)
        {
            return propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(DataGeneratorParameterAttribute));
        }
    }
}