using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Sink
{
    internal class CloudGateway : ISink
    {
        private ITransport _transport = new IoTHubTransport();

        public void Initialize(JToken parameters, IFilterHost dev)
        {
            var host = parameters.Value<string>("host");
            var port = parameters.Value<int>("port");
            _transport.Initialize(host, port, dev.DeviceId, dev.DeviceSecret);
        }

        public async Task Write(Data input, CancellationToken ct)
        {
            byte[] content = input.GetField<byte[]>("bytesContent");
            if (content == null)
            {
                return;
            }

            await _transport.SendMessage(content, ct);
        }
    }
}
