using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device
{
    internal class HTTPTransport : ITransport, IDisposable
    {
#if DEBUG
        private Random _rand = RandExtension.CreateByTask();
#endif

        private const string _scheme = "http";
        private const string _messagePath = @"devices/{0}/messages";
        private const string _commandPath = @"devices/{0}/commands";
        private const string _apiVersion = @"api-version=2015-09-01-preview";

        private string _deviceId;
        private Uri _postUri, _getUri;
        private HttpClient _httpClient;

        public void Initialize(string gateway, int port, string deviceId, string deviceSecret)
        {
            _deviceId = deviceId;

            var builder = new UriBuilder();
            builder.Scheme = _scheme;
            builder.Host = gateway;
            builder.Port = port;
            builder.Path = string.Format(CultureInfo.InvariantCulture, _messagePath, deviceId);
            builder.Query = _apiVersion;
            _postUri = builder.Uri;

            builder.Path = string.Format(CultureInfo.InvariantCulture, _commandPath, deviceId);
            _getUri = builder.Uri;

            _httpClient = new HttpClient();
        }

        public async Task<HttpStatusCode> SendMessage(byte[] content, CancellationToken ct)
        {
            using (var httpContent = new ByteArrayContent(content))
            {
                //1st chance exception 'ObjectDisposedException' may be thrown inside HTTPClient while it was canceled. It's an known issue of .NET
                var response = await _httpClient.PostAsync(_postUri, httpContent, ct);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Failed to send message as device {0}, status code = {1}, response content: {2}", _deviceId, response.StatusCode, error));
                }

                return response.StatusCode;
            }
        }

        public async Task<HttpStatusCode> ReceiveCommand(Stream content, CancellationToken ct)
        {
            //1st chance exception 'ObjectDisposedException' may be thrown inside HTTPClient while it was canceled. It's an known issue of .NET
            var response = await _httpClient.GetAsync(_getUri, ct);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                await response.Content.CopyToAsync(content);
            }

            return response.StatusCode;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
