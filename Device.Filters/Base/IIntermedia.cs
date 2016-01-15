using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Intermedia
{
    public interface IIntermedia : IFilter
    {
        Task<DataSet> Process(DataSet input, CancellationToken ct);
        Task<DataSet> Process(DataSet dataset, CancellationToken ct, string deviceId);
    }
}
