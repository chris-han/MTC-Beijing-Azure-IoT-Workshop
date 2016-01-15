using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Source
{
    internal class DataGeneratorHost : ISource
    {
        private DataGenerator.Base _generator;

        public void Initialize(JToken parameters, IFilterHost dev)
        {
            var generatorType = parameters.Value<string>("dataGenerator");
            var generatorParameters = parameters["parameters"];

            try
            {
                _generator = null;
                _generator = ActivatorExternsion.CreateInstance<DataGenerator.Base>(generatorType);
                _generator.Reset(generatorParameters);
            }
            catch (TypeLoadException ex)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Failed to create data generator {0} for device {1} due to exception {2}", generatorType, dev.DeviceId, ex.ToString()));
            }
        }

#pragma warning disable 1998
        public async Task<DataSet> Read(CancellationToken ct)
        {
            if (_generator == null)
            {
                return null;
            }
            else
            {
                var data = _generator.Read();
                data.Add("utcTimestamp", DateTime.UtcNow);

                var output = new DataSet();
                output.Add(data);
                return output;
            }
        }
#pragma warning restore 1998
    }
}
