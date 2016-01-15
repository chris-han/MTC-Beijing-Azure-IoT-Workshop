using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Azure.IoT.Studio.Device
{
    static public class ActivatorExternsion
    {
        static public T CreateInstance<T>(string type) where T : class
        {
            //Extract namespace alias into the full namespace
            var parts = type.Split('.');

            string prefix;
            if (GlobalSettings.Instance.NamespaceAliases.TryGetValue(parts[0], out prefix))
            {
                parts[0] = prefix;
                type = string.Join(".", parts);
            }

            var builtInTypes = Assembly.GetExecutingAssembly().GetTypes().Select(t => t.FullName);
            if (builtInTypes.Contains(type))
            {
                //BuiltIn filter/dataGenerator
                return Activator.CreateInstance(null, type).Unwrap() as T;
            }
            else
            {
                //Custom filter/dataGenerator must be named as the type name
                return Activator.CreateInstance(type, type).Unwrap() as T;
            }
        }
    }
}
