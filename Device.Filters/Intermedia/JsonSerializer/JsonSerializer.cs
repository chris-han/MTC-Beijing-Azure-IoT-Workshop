using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device.Filter.Intermedia
{
    public class JsonSerializer : IIntermedia
    {
        public void Initialize(JToken parameters, IFilterHost dev)
        {
        }

        public async Task<DataSet> Process(DataSet dataset, CancellationToken ct)
        {
#if AggravatedSerialization
            //Serialize the whole dataset as single json string
            var json = await Task.Factory.StartNew(
                new Func<object, string>(JsonConvert.SerializeObject),
                dataset,
                ct);

            var data = new Data();
            data.Add("stringContent", json);

            var output = new DataSet();
            output.Add(data);
            return output;

#else
            //Serialize each data as one json string
            foreach (var data in dataset)
            {
                var json = await Task.Factory.StartNew(
                    new Func<object, string>(JsonConvert.SerializeObject),
                    data,
                    ct);

                data.Add("stringContent", json);
            }

            return dataset;
#endif
        }
    }
}
