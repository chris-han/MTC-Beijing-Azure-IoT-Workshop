using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Azure.IoT.Studio.Device
{
    internal class DeviceTemplate : IEquatable<DeviceTemplate>
    {
        public string Name { get; private set; }
        public ISet<JobDesc> Jobs { get; private set; }

        public DeviceTemplate(string name, JArray jobs)
        {
            Name = name;

            var jobDescs = new HashSet<JobDesc>();

            foreach (var job in jobs)
            {
                var jobName = job.Value<string>("name");
                var interval = job.Value<int>("interval");

                if (interval <= 0)
                {
                    throw new ApplicationException(string.Format(CultureInfo.InvariantCulture, "{0}.interval = {1}. Please assign it a positive value to avoid tight loop without interval", name, interval));
                }

                var sourceDesc = FilterDesc.Create(job["sourceFilter"]);

                var intermedias = job["intermediaFilters"] as JArray;
                var intermediaDescs = intermedias == null || intermedias.Count == 0 ?
                    new List<FilterDesc>() :
                    intermedias.Select(t => FilterDesc.Create(t));

                var sinkDesc = FilterDesc.Create(job["sinkFilter"]);

                var jobDesc = new JobDesc(jobName, interval, sourceDesc, intermediaDescs, sinkDesc);
                jobDescs.Add(jobDesc);
            }

            Jobs = jobDescs;
        }

        public override string ToString()
        {
            return Name;
        }

        #region Equals implementation
        public override bool Equals(object obj)
        {
            return Equals(obj as DeviceTemplate);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Jobs.Aggregate(0, (sum, j) => sum ^ j.GetHashCode());
        }

        public bool Equals(DeviceTemplate other)
        {
            if (other == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return Name.Equals(other.Name) && Jobs.SetEquals(other.Jobs);
        }
        #endregion
    }
}
