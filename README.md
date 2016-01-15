# MTC-Beijing-Azure-IoT-Workshop
Using Azure IoT Studio in Azure China

How to setup the device management tool "DMConsole"?
	Modify setting in the configuration file 'Microsoft.Azure.IoT.Studio.Tool.DMConsole.exe.config':
	Path:
	$/
	  configuration/
	    userSettings/
	      Microsoft.Azure.IoT.Studio.Tool.Properties.Settings/
	        setting name="ConnectionString"/
	          value
	Replace the value with IoTEntry connection string (it could be found at IoTStudio portal)

#########################################
How to register new device by DMConsole?
Microsoft.Azure.IoT.Studio.Tool.DMConsole /a:AddDevice /d:<deviceID>
i.e. 
	"C:\IOT Workshop\IoTStudio\Binary\DMConsole\Microsoft.Azure.IoT.Studio.Tool.DMConsole.exe" /a:AddDevice /d:MTC_Simulator002
########################################

How to unregister device by DMConsole?
Microsoft.Azure.IoT.Studio.Tool.DMConsole /a:RemoveDevice /d:<deviceID>

How to retrieve given device by DMConsole?
Microsoft.Azure.IoT.Studio.Tool.DMConsole /a:GetDevice /d:<deviceID>

How to list all registered devices by DMConsole
Microsoft.Azure.IoT.Studio.Tool.DMConsole /a:ListDevices

How to send command to given device by DMConsole?
Microsoft.Azure.IoT.Studio.Tool.DMConsole /a:SendCommand /d:<deviceID> /p:<command>

################################## 
How to launch device simulator?
	Run Microsoft.Azure.IoT.Studio.Device.Sample.exe /m:<manifest file>
	i.e.
	Cd "C:\IOT Workshop\IoTStudio\Binary\DeviceSimulator"
	Microsoft.Azure.IoT.Studio.Device.Sample.exe /m:".\DeviceManifest.json"
################################## 

To setup device to be simulated, please modify $/deviceGroups/devices. Replace "deviceId" with the device ID, and replace "deviceSecret" with the full device connection string.


