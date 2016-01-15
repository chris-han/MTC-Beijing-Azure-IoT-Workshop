using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Source
{
    public class BasicQueue : ISource
    {
        private Queue<Data> _queue;

        public void Initialize(JToken parameters, IFilterHost dev)
        {
            var queueID = parameters.Value<string>("queueID");

            _queue = dev.GetConnector("MemoryQueue", queueID) as Queue<Data>;
        }

#pragma warning disable 1998
        public async Task<DataSet> Read(CancellationToken ct)
        {
            var output = new DataSet();

            lock (_queue)
            {
                output.AddRange(_queue);
                _queue.Clear();
            }

            return output;
        }
#pragma warning restore 1998
    }
}
