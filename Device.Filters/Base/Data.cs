using System.Collections.Generic;

namespace Microsoft.Azure.IoT.Studio.Device
{
    public class Data : Dictionary<string, object>
    {
        public T GetField<T>(string name) where T : class
        {
            object fieldObject;
            if (!TryGetValue(name, out fieldObject))
            {
                return null;
            }

            return fieldObject as T;
        }
    }
}
