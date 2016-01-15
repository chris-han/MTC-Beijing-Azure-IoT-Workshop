using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device
{
    class IoTHubTransport : ITransport
    {
        private string _deviceId;
        private Devices.Client.DeviceClient _client;

        public void Initialize(string gateway, int port, string deviceId, string deviceSecret)
        {
            try {
                _deviceId = deviceId;
                _client = Devices.Client.DeviceClient.CreateFromConnectionString(deviceSecret, Devices.Client.TransportType.Http1);
            }
            catch(Exception ex)
            {
                var err=ex.Message;
                throw ex;
            }
        }

        public async Task<HttpStatusCode> SendMessage(byte[] content, CancellationToken ct)
        {
            try
            {
                await _client.SendEventAsync(new Devices.Client.Message(content));
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Failed to send message as device {0}, exception: {1}", _deviceId, ex.ToString()));
                return HttpStatusCode.InternalServerError;
            }
        }

        public async Task<HttpStatusCode> ReceiveCommand(Stream content, CancellationToken ct)
        {
            try
            {
                var message = await _client.ReceiveAsync();
                if (message != null)
                {
                    await message.BodyStream.CopyToAsync(content);
                    return HttpStatusCode.OK;
                }
                else
                {
                    return HttpStatusCode.NoContent;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Failed to receive command as device {0}, exception: {1}", _deviceId, ex.ToString()));
                return HttpStatusCode.InternalServerError;
            }
        }
    }
}
