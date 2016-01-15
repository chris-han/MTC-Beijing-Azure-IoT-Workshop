using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Source
{
    public interface ISource : IFilter
    {
        Task<DataSet> Read(CancellationToken ct);
    }
}
