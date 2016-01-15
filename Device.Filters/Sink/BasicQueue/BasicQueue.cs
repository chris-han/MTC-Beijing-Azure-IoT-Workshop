using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Sink
{
    public class BasicQueue : ISink
    {
        private Queue<Data> _queue;

        public void Initialize(JToken parameters, IFilterHost dev)
        {
            var queueID = parameters.Value<string>("queueID");

            _queue = dev.GetConnector("MemoryQueue", queueID) as Queue<Data>;
        }

#pragma warning disable 1998
        public async Task Write(Data input, CancellationToken ct)
        {
            lock (_queue)
            {
                _queue.Enqueue(input);
            }
        }
#pragma warning restore 1998
    }
}
