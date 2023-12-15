using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace UtilitiesDeviceInfo
{
    /// <summary>
    /// Plugin device factory for devices that use IBasicCommunication
    /// </summary>
    /// <remarks>
    /// Rename the class to match the device plugin being developed
    /// </remarks>
    /// <example>
    /// "DeviceInfoControllerFactory" renamed to "MyDeviceFactory"
    /// </example>
    public class DeviceInfoControllerFactory : EssentialsPluginDeviceFactory<DeviceInfoController>
    {
        /// <summary>
        /// Plugin device factory constructor
        /// </summary>
        /// <remarks>
        /// Update the MinimumEssentialsFrameworkVersion & TypeNames as needed when creating a plugin
        /// </remarks>
        /// <example>
        /// Set the minimum Essentials Framework Version
        /// <code>
        /// MinimumEssentialsFrameworkVersion = "1.6.4;
        /// </code>
        /// In the constructor we initialize the list with the typenames that will build an instance of this device
        /// <code>
        /// TypeNames = new List<string>() { "SamsungMdc", "SamsungMdcDisplay" };
        /// </code>
        /// </example>
        public DeviceInfoControllerFactory()
        {
            // Set the minimum Essentials Framework Version
            MinimumEssentialsFrameworkVersion = "1.6.6";

            // In the constructor we initialize the list with the typenames that will build an instance of this device
            TypeNames = new List<string>() {"deviceInfo"};
        }

        /// <summary>
        /// Builds and returns an instance of EssentialsPluginDeviceTemplate
        /// </summary>
        /// <param name="dc">device configuration</param>
        /// <returns>plugin device or null</returns>
        /// <remarks>		
        /// The example provided below takes the device key, name, properties config and the comms device created.
        /// Modify the EssetnialsPlugingDeviceTemplate constructor as needed to meet the requirements of the plugin device.
        /// </remarks>
        /// <seealso cref="PepperDash.Core.eControlMethod"/>
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "[{0}] Factory Attempting to create new device from type: {1}", dc.Key, dc.Type);

            return new DeviceInfoController(dc.Key, dc.Name);
        }
    }
}

          