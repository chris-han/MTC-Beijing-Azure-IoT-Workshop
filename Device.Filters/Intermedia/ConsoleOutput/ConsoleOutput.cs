using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Intermedia
{
    public class ConsoleOutput : IIntermedia
    {
        string _deviceId;
        string _interestringField;
        string _format;
        ConsoleColor _foregroundColor;

        public void Initialize(JToken parameters, IFilterHost dev)
        {
            _deviceId = dev.DeviceId;
            _interestringField = parameters.Value<string>("interestingField");
            _format = parameters.Value<string>("format");

            var color = parameters.Value<string>("foregroundColor");
            if(!Enum.TryParse<ConsoleColor>(color, out _foregroundColor))
            {
                _foregroundColor = ConsoleColor.Gray;
            }
        }

#pragma warning disable 1998
        public async Task<DataSet> Process(DataSet dataset, CancellationToken ct)
        {
            foreach (var data in dataset)
            {
                object fieldObject;
                if (data.TryGetValue(_interestringField, out fieldObject))
                {
                    string message = string.Format(CultureInfo.InvariantCulture, _format, fieldObject);

                    Console.ForegroundColor = _foregroundColor;
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} Device [{1}] {2}", DateTime.Now, _deviceId, message));
                }
            }

            return dataset;
        }
        public async Task<DataSet> Process(DataSet dataset, CancellationToken ct, string DeviceId)
        {
            foreach (var data in dataset)
            {
                data["DeviceId"] = DeviceId;//Chris han add DeviceId to Json
                object fieldObject;
                if (data.TryGetValue(_interestringField, out fieldObject))
                {
                    string message = string.Format(CultureInfo.InvariantCulture, _format, fieldObject);

                    Console.ForegroundColor = _foregroundColor;
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} Device [{1}] {2}", DateTime.Now, _deviceId, message));
                }
            }

            return dataset;
        }
#pragma warning restore 1998
    }
}
