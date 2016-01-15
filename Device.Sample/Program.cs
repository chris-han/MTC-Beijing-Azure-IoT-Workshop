using CommandLine;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoT.Studio.Device
{
    class CommandArguments
    {
        [Argument(ArgumentType.Required, ShortName = "m", HelpText = "The device manifest file")]
        public string DeviceManifest;
    }

    class Program
    {
        static private DeviceEngine _engine = new DeviceEngine();
        static private string _manifest;
        static private DateTime _lastUpdate;

        static void Main(string[] args)
        {
            CommandArguments parsedArgs = new CommandArguments();
            if (!Parser.ParseArgumentsWithUsage(args, parsedArgs))
            {
                return;
            }

            //Begin watching change of the manifest file
            var watcher = new FileSystemWatcher();
            watcher.Path = Path.GetDirectoryName(parsedArgs.DeviceManifest);
            watcher.Filter = Path.GetFileName(parsedArgs.DeviceManifest);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += OnManifestChanged;
            watcher.EnableRaisingEvents = true;

            _manifest = parsedArgs.DeviceManifest;
            _lastUpdate = DateTime.MinValue;    //The initial update will be triggered immediatly

            Task.Run(UpdateDeviceEngineTask).Wait();
        }

        private static void OnManifestChanged(object sender, FileSystemEventArgs e)
        {
            _lastUpdate = DateTime.Now;
        }

        private static Task UpdateDeviceEngineTask()
        {
            while (true)
            {
                //Check last updating time to aggregate 'updating' events
                if ((DateTime.Now - _lastUpdate).TotalMilliseconds > 5000)
                {
                    _lastUpdate = DateTime.MaxValue;

                    try
                    {
                        Trace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Updating device engine by manifest {0}", _manifest));

                        var json = string.Join("\r\n", File.ReadAllLines(_manifest));
                        _engine.Update(json);

                        Trace.TraceInformation(string.Format(CultureInfo.InvariantCulture, "Finished updating device engine by manifest {0}", _manifest));
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Failed to update device engine due to exception {0}", ex.ToString()));
                    }
                }

                Task.Delay(1000);
            }
        }
    }
}
