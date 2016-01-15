using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Sink
{
    public interface ISink : IFilter
    {
        Task Write(Data input, CancellationToken ct);
    }
}
