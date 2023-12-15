using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Core.Intersystem;
using PepperDash.Core.Intersystem.Tokens;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.DeviceInfo;

namespace UtilitiesDeviceInfo
{
    /// <summary>
    /// Plugin device template for logic devices that don't communicate outside the program
    /// </summary>
    public class DeviceInfoController : EssentialsBridgeableDevice
    {
        private const int XSigEncoding = 28591;
        private readonly byte[] _clearBytes = XSigHelpers.ClearOutputs();

        private readonly CCriticalSection _refreshCriticalSection = new CCriticalSection();
        public BoolFeedback UpdateInProgressFeedback;

        private SortedList<string, IDeviceInfoProvider> _deviceInfoProviderList;

        private bool _updateInProgress;

        /// <summary>
        /// Plugin device constructor
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        public DeviceInfoController(string key, string name)
            : base(key, name)
        {
            UpdateInProgressFeedback = new BoolFeedback(() => UpdateInProgress);
        }

        #region Overrides of Device

        public override bool CustomActivate()
        {
            var devs = DeviceManager.GetDevices().OfType<IDeviceInfoProvider>().ToDictionary(d => d.Key, d => d);

            _deviceInfoProviderList = new SortedList<string, IDeviceInfoProvider>(devs);

            return base.CustomActivate();
        }

        #endregion

        #region Overrides of EssentialsBridgeableDevice

        /// <summary>
        /// Links the plugin device to the EISC bridge
        /// </summary>
        /// <param name="trilist"></param>
        /// <param name="joinStart"></param>
        /// <param name="joinMapKey"></param>
        /// <param name="bridge"></param>
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new DeviceInfoJoinMap(joinStart);

            // This adds the join map to the collection on the bridge
            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
                Debug.Console(0, "Linking to Bridge Type {0}", bridge.GetType().Name);
            }

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            Debug.Console(1, "Linking to Trilist '{0}'", trilist.ID.ToString("X"));

            trilist.SetSigFalseAction(joinMap.UpdateInfo.JoinNumber, RefreshDeviceInfo);
            trilist.SetUshort(joinMap.DeviceCount.JoinNumber, (ushort) _deviceInfoProviderList.Count);

            UpdateInProgressFeedback.LinkInputSig(trilist.BooleanInput[joinMap.UpdateInfo.JoinNumber]);
            UpdateInProgressFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.UpdateInfoComplete.JoinNumber]);


            foreach (var dip in _deviceInfoProviderList.Select(deviceInfoProvider => deviceInfoProvider.Value))
            {
                dip.DeviceInfoChanged += (device, args) => UpdateSingleDevice(trilist, device, joinMap);

                Debug.Console(1, this, "Updating Device Info for device with key {0}", dip.Key);

                UpdateSingleDevice(trilist, dip, joinMap);
            }

            trilist.OnlineStatusChange += (device, args) =>
            {
                if (!args.DeviceOnLine)
                {
                    return;
                }

                RefreshDeviceInfo();
            };
        }

        public void RefreshDeviceInfo()
        {
            try
            {
                _refreshCriticalSection.Enter();

                UpdateInProgress = true;

                foreach (var deviceInfoProvider in _deviceInfoProviderList.Select(d => d.Value))
                {
                    var dev = deviceInfoProvider;

                    CrestronInvoke.BeginInvoke((o) => dev.UpdateDeviceInfo());
                }
            }
            finally
            {
                _refreshCriticalSection.Leave();

                UpdateInProgress = false;
            }
        }

        private void UpdateSingleDevice(BasicTriList trilist, IKeyed device, DeviceInfoJoinMap joinMap)
        {
            var currentInfo = UpdateXSigForDevice(device as IDeviceInfoProvider);

            //0-based index in the list for the device. Indices stay the same as the list is sorted alphabetically by key
            var index = (uint) _deviceInfoProviderList.IndexOfValue(device as IDeviceInfoProvider);

            trilist.SetString(joinMap.DeviceInfoXSig.JoinNumber + index, currentInfo);
        }

        private static string UpdateXSigForDevice(IDeviceInfoProvider device)
        {
            var tokenArray = new XSigToken[6];

            tokenArray[0] = new XSigSerialToken(1, device.Key);
            tokenArray[1] = new XSigSerialToken(2, device.DeviceInfo.HostName ?? String.Empty);
            tokenArray[2] = new XSigSerialToken(3, device.DeviceInfo.IpAddress ?? String.Empty);
            tokenArray[3] = new XSigSerialToken(4, device.DeviceInfo.MacAddress ?? String.Empty);
            tokenArray[4] = new XSigSerialToken(5, device.DeviceInfo.SerialNumber ?? String.Empty);
            tokenArray[5] = new XSigSerialToken(6, device.DeviceInfo.FirmwareVersion ?? String.Empty);

            return GetXSigString(tokenArray);
        }

        private static string GetXSigString(XSigToken[] tokenArray)
        {
            string returnString;
            using (var s = new MemoryStream())
            {
                using (var tw = new XSigTokenStreamWriter(s, true))
                {
                    tw.WriteXSigData(tokenArray);
                }

                var xSig = s.ToArray();

                returnString = Encoding.GetEncoding(XSigEncoding).GetString(xSig, 0, xSig.Length);
            }

            return returnString;
        }

        #endregion

        public bool UpdateInProgress
        {
            get { return _updateInProgress; }
            private set
            {
                if (value == _updateInProgress)
                {
                    return;
                }

                _updateInProgress = value;
                UpdateInProgressFeedback.FireUpdate();
            }
        }
    }
}