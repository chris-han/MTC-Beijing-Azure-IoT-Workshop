using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Intermedia
{
    public class UTF8Encoding : IIntermedia
    {
        public void Initialize(JToken parameters, IFilterHost dev)
        {
        }

#pragma warning disable 1998
        public async Task<DataSet> Process(DataSet dataset, CancellationToken ct)
        {
            foreach (var data in dataset)
            {
                var content = data.GetField<string>("stringContent");
                if (content == null)
                {
                    continue;
                }

                data.Add("bytesContent", Encoding.UTF8.GetBytes(content));
            }

            return dataset;
        }
        public async Task<DataSet> Process(DataSet dataset, CancellationToken ct, string DeviceId)
        {
            foreach (var data in dataset)
            {
                data["DeviceId"] = DeviceId;//Chris han add DeviceId to Json
                var content = data.GetField<string>("stringContent");
                if (content == null)
                {
                    continue;
                }

                data.Add("bytesContent", Encoding.UTF8.GetBytes(content));
            }

            return dataset;
        }
#pragma warning restore 1998
    }
}
