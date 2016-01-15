namespace Microsoft.Azure.IoT.Studio.Device.Filter
{
    public interface IFilterHost
    {
        string DeviceId { get; set; }
        string DeviceSecret { get; set; }

        object GetConnector(string type, string id);
    }
}
