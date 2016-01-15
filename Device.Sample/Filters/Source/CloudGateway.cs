using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Source
{
    internal class CloudGateway : ISource
    {
        private ITransport _transport = new IoTHubTransport();

        public void Initialize(JToken parameters, IFilterHost dev)
        {
            var host = parameters.Value<string>("host");
            var port = parameters.Value<int>("port");
            _transport.Initialize(host, port, dev.DeviceId, dev.DeviceSecret);
        }

        public async Task<DataSet> Read(CancellationToken ct)
        {
            var output = new DataSet();

            using (var stream = new MemoryStream())
            {
                while (true)
                {
                    var status = await _transport.ReceiveCommand(stream, ct);
                    if (status != HttpStatusCode.OK)
                    {
                        break;
                    }

                    var data = new Data();
                    data.Add("bytesContent", stream.ToArray());

                    output.Add(data);
                }
            }

            return output;
        }
    }
}
