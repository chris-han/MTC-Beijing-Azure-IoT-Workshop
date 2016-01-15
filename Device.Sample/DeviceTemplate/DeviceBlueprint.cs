using System;
using System.Globalization;

namespace Microsoft.Azure.IoT.Studio.Device
{
    internal class DeviceBlueprint : IEquatable<DeviceBlueprint>
    {
        public string DeviceId { get; private set; }
        public string DeviceSecret { get; private set; }
        public DeviceTemplate Template { get; private set; }

        public DeviceBlueprint(string deviceId, string deviceSecret, DeviceTemplate template)
        {
            DeviceId = deviceId;
            DeviceSecret = deviceSecret;
            Template = template;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}: {1}", Template.Name, DeviceId);
        }

        #region Equals implementation
        public override bool Equals(object obj)
        {
            return this.Equals(obj as DeviceBlueprint);
        }

        public override int GetHashCode()
        {
            return DeviceId.GetHashCode() ^ DeviceSecret.GetHashCode() ^ Template.GetHashCode();
        }

        public bool Equals(DeviceBlueprint other)
        {
            if (other == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return DeviceId.Equals(other.DeviceId) &&
                DeviceSecret.Equals(other.DeviceSecret) &&
                Template.Equals(other.Template);
        }
        #endregion
    }
}
