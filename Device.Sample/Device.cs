using Microsoft.Azure.IoT.Studio.Device.Filter;
using Microsoft.Azure.IoT.Studio.Device.Filter.Intermedia;
using Microsoft.Azure.IoT.Studio.Device.Filter.Sink;
using Microsoft.Azure.IoT.Studio.Device.Filter.Source;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device
{
    internal class Device : IDisposable, IFilterHost
    {
        public string DeviceId { get; set; }
        public string DeviceSecret { get; set; }

        /// <summary>
        /// Connectors linking Jobs together
        /// </summary>
        private Dictionary<string, object> _connectors = new Dictionary<string, object>();

        private CancellationTokenSource _source = new CancellationTokenSource();
        private Task[] _runningTasks;

        public void Start(DeviceBlueprint blueprint)
        {
            DeviceId = blueprint.DeviceId;
            DeviceSecret = blueprint.DeviceSecret;
            Trace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Device {0} status = Booting", DeviceId));

            var tasks = new List<Task>();
            foreach (var job in blueprint.Template.Jobs)
            {
                try
                {
                    //Instance filters
                    var sourceFilter = ActivatorExternsion.CreateInstance<ISource>(job.Source.Type);
                    sourceFilter.Initialize(job.Source.Parameters, this);

                    //Chris Han - try to inject DeviceId
                    //Newtonsoft.Json.Linq.JObject did = (Newtonsoft.Json.Linq.JObject)job.Source.Parameters;//.Value<Newtonsoft.Json.Linq.JToken>("DeviceId");
                    //var p = did["parameters"]["DeviceId"];
                    //var k = DeviceId;

                    sourceFilter.Initialize(job.Source.Parameters, this);

                    var intermediaFilters = job.Intermedias.Select(f =>
                    {
                        var filter = ActivatorExternsion.CreateInstance<IIntermedia>(f.Type);
                        filter.Initialize(f.Parameters, this);
                        return filter;
                    });

                    var sinkFilter = ActivatorExternsion.CreateInstance<ISink>(job.Sink.Type);
                    sinkFilter.Initialize(job.Sink.Parameters, this);

                    //Kick-off the Job. Record the task object for checking while they were stopped
                    tasks.Add(JobEntry(job.Name, job.Interval, sourceFilter, intermediaFilters, sinkFilter, _source.Token));
                }
                catch (TypeLoadException ex)
                {
                    Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Failed to start device {0} due to exception {1}", DeviceId, ex.ToString()));
                }
            }

            _runningTasks = tasks.ToArray();
            Trace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Device {0} status = Running", DeviceId));
        }

        public void Stop()
        {
            Trace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Device {0} status = Shutting down", DeviceId));
            _source.Cancel();

            Task.WaitAll(_runningTasks);
            Trace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Device {0} status = Down", DeviceId));

            _runningTasks = null;
            _connectors.Clear();
        }

        public async Task JobEntry(string name, int interval, ISource source, IEnumerable<IIntermedia> intermedias, ISink sink, CancellationToken ct)
        {
            try
            {
                while (true)
                {
                    //Read data set from source filter
                    DataSet dataset = await source.Read(ct);
                    if (dataset == null || !dataset.Any())
                    {
                        //Skip current loop if no data read, or filter is in status "stopping"
                        goto loop;
                    }

                    //Process data set by intermedia filters
                    foreach (var intermedia in intermedias)
                    {
                        //dataset = await intermedia.Process(dataset, ct);
                        dataset = await intermedia.Process(dataset, ct, DeviceId);//Chris Han add DeviceId to Json
                        if (dataset == null || !dataset.Any())
                        {
                            //Skip current loop if no data read, or filter is in status "stopping"
                            goto loop;
                        }
                    }

                    //Individually sink each data, to help down-stream Job in aggregating
                    foreach (var data in dataset)
                    {
                        await sink.Write(data, ct);
                    }

                    loop:
                    await Task.Delay(interval, ct);
                }
            }
            catch (OperationCanceledException)
            {
                Trace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Job {0}.{1} finished", DeviceId, name));
            }
            catch (Exception ex)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Job {0}.{1} stopped due to exception {2}", DeviceId, name, ex.ToString()));
            }
        }

        public object GetConnector(string type, string id)
        {
            object connector;
            string key = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", type, id);

            lock (_connectors)
            {
                if (!_connectors.TryGetValue(key, out connector))
                {
                    switch (type)
                    {
                        case "MemoryQueue":
                            connector = new Queue<Data>();
                            break;

                        default:
                            throw new ApplicationException(string.Format(CultureInfo.InvariantCulture, "Unknown connector type {0}", type));
                    }

                    _connectors.Add(key, connector);
                }
            }

            return connector;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _source.Dispose();
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
