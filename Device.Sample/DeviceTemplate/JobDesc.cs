using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Azure.IoT.Studio.Device
{
    internal class JobDesc : IEquatable<JobDesc>
    {
        public string Name { get; private set; }
        public int Interval { get; private set; }
        public FilterDesc Source { get; private set; }
        public IEnumerable<FilterDesc> Intermedias { get; private set; }
        public FilterDesc Sink { get; private set; }

        public JobDesc(string name, int interval, FilterDesc source, IEnumerable<FilterDesc> intermedias, FilterDesc sink)
        {
            Name = name;
            Interval = interval;

            if (source == null)
            {
                throw new ApplicationException(string.Format(CultureInfo.InvariantCulture, "Missing source filter in job {0}", Name));
            }
            Source = source;

            Intermedias = new List<FilterDesc>(intermedias);

            if (sink == null)
            {
                throw new ApplicationException(string.Format(CultureInfo.InvariantCulture, "Missing sink filter in job {0}", Name));
            }
            Sink = sink;
        }

        public override string ToString()
        {
            return Name;
        }

        #region Equals implementation
        public override bool Equals(object obj)
        {
            return base.Equals(obj as JobDesc);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^
                Interval ^
                Source.GetHashCode() ^
                Intermedias.Aggregate(0, (sum, f) => sum ^ f.GetHashCode()) ^
                Sink.GetHashCode();
        }

        public bool Equals(JobDesc other)
        {
            if (other == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return Name.Equals(other.Name) &&
                Interval == other.Interval &&
                Source.Equals(other.Source) &&
                Intermedias.SequenceEqual(other.Intermedias) &&
                Sink.Equals(other.Sink);
        }
        #endregion
    }
}
