using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Azure.IoT.Studio.Device
{
    internal class DeviceEngine
    {
        private Dictionary<string, DeviceBlueprint> _currentBlueprints = new Dictionary<string, DeviceBlueprint>();
        private Dictionary<string, Device> _devices = new Dictionary<string, Device>();

        private IEnumerable<DeviceBlueprint> Load(string manifest)
        {
            JToken root = JsonConvert.DeserializeObject(manifest) as JToken;

            var globalSettingsToken = root["globalSettings"];
            if (globalSettingsToken != null)
            {
                GlobalSettings.Instance.Load(globalSettingsToken);
            }

            var devices = new List<DeviceBlueprint>();

            var groups = root["deviceGroups"] as JArray;
            foreach (var group in groups)
            {
                var name = group.Value<string>("name");
                var jobs = group["jobs"] as JArray;
                var template = new DeviceTemplate(name, jobs);

                var deviceCredentials = group["devices"] as JArray;
                foreach (var deviceCredential in deviceCredentials)
                {
                    var deviceId = deviceCredential.Value<string>("deviceId");
                    var deviceSecret = deviceCredential.Value<string>("deviceSecret");
                    var device = new DeviceBlueprint(deviceId, deviceSecret, template);

                    devices.Add(device);
                }
            }

            return devices;
        }

        public void Update(string manifest)
        {
            var newBlueprints = Load(manifest);

            IEnumerable<DeviceBlueprint> added, changed;
            IEnumerable<string> removed;

            if (GlobalSettings.Instance.Updated)
            {
                added = newBlueprints;
                removed = _currentBlueprints.Keys;
                changed = new List<DeviceBlueprint>();
            }
            else
            {
                added = newBlueprints.Where(dev => !_currentBlueprints.Keys.Contains(dev.DeviceId));
                removed = _currentBlueprints.Keys.Except(newBlueprints.Select(dev => dev.DeviceId));
                changed = newBlueprints.Where(dev =>
                {
                    DeviceBlueprint old;
                    if (!_currentBlueprints.TryGetValue(dev.DeviceId, out old))
                    {
                        return false;
                    }

                    return !dev.Equals(old);
                });
            }

            var toStop = removed.ToList();
            toStop.AddRange(changed.Select(dev => dev.DeviceId));
            foreach (var deviceId in toStop)
            {
                Device device;
                if (_devices.TryGetValue(deviceId, out device))
                {
                    device.Stop();
                    _devices.Remove(deviceId);
                }
            }

            var toStart = added.ToList();
            toStart.AddRange(changed);
            foreach (var blueprint in toStart)
            {
                var device = new Device();
                device.Start(blueprint);
                _devices.Add(blueprint.DeviceId, device);
            }

            _currentBlueprints = newBlueprints.ToDictionary(d => d.DeviceId);
        }
    }
}
