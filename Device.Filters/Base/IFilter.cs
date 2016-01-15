using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoT.Studio.Device.Filter
{
    public interface IFilter
    {
        /// <summary>
        /// Called by Device once filter was instanced
        /// </summary>
        /// <param name="parameters">Filter parameters in JSON</param>
        /// <param name="dev">The hosting device</param>
        void Initialize(JToken parameters, IFilterHost dev);
    }
}