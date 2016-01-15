using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Sink
{
    public class Null : ISink
    {
        public void Initialize(JToken parameters, IFilterHost dev)
        {
        }


#pragma warning disable 1998
        public async Task Write(Data input, CancellationToken ct)
        {
        }
#pragma warning restore 1998
    }
}
