using CommandLine;
using Microsoft.Azure.IoT.Studio.Tool.Properties;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Azure.IoT.Studio.Tool
{
    enum Action
    {
        AddDevice,
        RemoveDevice,
        GetDevice,
        ListDevices,
        SendCommand
    }

    class CommandArguments
    {
        [Argument(ArgumentType.Required, ShortName = "a", HelpText = "The action to invoke")]
        public Action Action;

        [Argument(ArgumentType.AtMostOnce, ShortName = "d", HelpText = "The target device")]
        public string DeviceId;

        [Argument(ArgumentType.AtMostOnce, ShortName = "p", HelpText = "The parameter of action")]
        public string Parameter;
    }

    class Program
    {
        static void Main(string[] args)
        {
            CommandArguments parsedArgs = new CommandArguments();
            if (!Parser.ParseArgumentsWithUsage(args, parsedArgs))
            {
                return;
            }

            try
            {
                var iothubConnectionStringBuilder = Devices.IotHubConnectionStringBuilder.Create(Settings.Default.ConnectionString);

                string ioTHubHostName = Settings.Default.ConnectionString.Split(';').Single(s => s.StartsWith("HostName=")).Substring(9);
                bool uriSchemeSpecified = ioTHubHostName.Contains("://");
                Console.WriteLine("Target IoTHub: {0}", ioTHubHostName);

                string response;

                switch (parsedArgs.Action)
                {
                    case Action.AddDevice:
                        {
                            var manager = Devices.RegistryManager.CreateFromConnectionString(Settings.Default.ConnectionString);
                            var device = manager.AddDeviceAsync(new Devices.Device(parsedArgs.DeviceId)).Result;
                            response = string.Format("Device {0} created, key = {1}\r\nConnection string = {2}",
                                device.Id,
                                device.Authentication.SymmetricKey.PrimaryKey,
                                DeviceConnectionString(ioTHubHostName, device));
                        }
                        break;

                    case Action.RemoveDevice:
                        {
                            var manager = Devices.RegistryManager.CreateFromConnectionString(Settings.Default.ConnectionString);
                            manager.RemoveDeviceAsync(parsedArgs.DeviceId).Wait();
                            response = string.Format("Device {0} removed", parsedArgs.DeviceId);
                        }
                        break;

                    case Action.GetDevice:
                        {
                            var manager = Devices.RegistryManager.CreateFromConnectionString(Settings.Default.ConnectionString);
                            var device = manager.GetDeviceAsync(parsedArgs.DeviceId).Result;

                            if (device == null)
                            {
                                response = string.Format("Not found device {0}", parsedArgs.DeviceId);
                            }
                            else
                            {
                                response = string.Format("Device {0} retrieved, key = {1}\r\nConnection string = {2}",
                                    device.Id,
                                    device.Authentication.SymmetricKey.PrimaryKey,
                                    DeviceConnectionString(ioTHubHostName, device));
                            }
                        }
                        break;

                    case Action.ListDevices:
                        {
                            var manager = Devices.RegistryManager.CreateFromConnectionString(Settings.Default.ConnectionString);
                            var devices = manager.GetDevicesAsync(1000).Result;

                            response = string.Format("{0} devices found", devices.Count());
                            foreach (var device in devices)
                            {
                                response += string.Format("\r\n\r\n  device {0}, key = {1}\r\n  Connection string = {2}",
                                    device.Id,
                                    device.Authentication.SymmetricKey.PrimaryKey,
                                    DeviceConnectionString(ioTHubHostName, device));
                            }
                        }
                        break;

                    case Action.SendCommand:
                        {
                            string requestUri = string.Format("{0}/devices/{1}/messages/devicebound?api-version=2015-08-15-preview", ioTHubHostName, parsedArgs.DeviceId);
                            if (!uriSchemeSpecified)
                            {
                                requestUri = "https://" + requestUri;
                            }

                            using (var httpClient = new HttpClient())
                            {
                                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri))
                                {
                                    request.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(parsedArgs.Parameter));

                                    string signature = IoTEntrySASGenerator.Create(iothubConnectionStringBuilder.HostName, 3600, iothubConnectionStringBuilder.SharedAccessKey, iothubConnectionStringBuilder.SharedAccessKeyName);
                                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", signature);

                                    httpClient.SendAsync(request).Wait();
                                }
                            }

                            response = string.Format("Sent message {0} to device {1}", parsedArgs.Parameter, parsedArgs.DeviceId);
                        }
                        break;

                    default:
                        response = string.Format("Error: unexpected action {0}", parsedArgs.Action);
                        break;
                }

                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Response = {0}", response));
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Exception raised: {0}", ex.ToString()));
            }
        }

        static string DeviceConnectionString(string ioTHubHostName, Devices.Device device)
        {
            string deviceConnectionString = string.Format("HostName={0};CredentialScope=Device;DeviceId={1};SharedAccessKey={2}",
                ioTHubHostName,
                device.Id,
                device.Authentication.SymmetricKey.PrimaryKey);

            return deviceConnectionString;
        }
    }

    static class IoTEntrySASGenerator
    {
        public static string Create(string audience, int TTL, string key, string name)
        {
            var zero = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TTL += (int)(DateTime.UtcNow - zero).TotalSeconds;

            string expiry = TTL.ToString();
            string request = WebUtility.UrlEncode(audience) + "\n" + WebUtility.UrlEncode(expiry);

            string sign;
            using (var hmac = new HMACSHA256(Convert.FromBase64String(key)))
            {
                sign = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(request)));
            }

            string signature = string.Format("sr={0}&sig={1}&se={2}",
                WebUtility.UrlEncode(audience),
                WebUtility.UrlEncode(sign),
                WebUtility.UrlEncode(expiry));

            if (!string.IsNullOrEmpty(name))
            {
                signature += "&skn=" + WebUtility.UrlEncode(name);
            }

            return signature;
        }
    }
}
