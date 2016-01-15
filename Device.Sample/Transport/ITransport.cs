using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device
{
    interface ITransport
    {
        void Initialize(string gateway, int port, string deviceId, string deviceSecret);
        Task<HttpStatusCode> SendMessage(byte[] content, CancellationToken ct);
        Task<HttpStatusCode> ReceiveCommand(Stream content, CancellationToken ct);
    }
}
