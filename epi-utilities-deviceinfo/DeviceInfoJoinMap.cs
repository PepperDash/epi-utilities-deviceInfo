using PepperDash.Essentials.Core;

namespace PepperDash.Plugins.DeviceInfo
{
	/// <summary>
	/// Plugin device Bridge Join Map
	/// </summary>
	/// <remarks>
	/// Rename the class to match the device plugin being developed.  Reference Essentials JoinMaps, if one exists for the device plugin being developed
	/// </remarks>
	/// <see cref="PepperDash.Essentials.Core.Bridges"/>
	
	public class DeviceInfoJoinMap : JoinMapBaseAdvanced
	{
	    [JoinName("UpdateInfo")] public JoinDataComplete UpdateInfo =
	        new JoinDataComplete(new JoinData {JoinNumber = 1, JoinSpan = 1},
	            new JoinMetadata
	            {
	                Description = "Update device info",
	                JoinCapabilities = eJoinCapabilities.FromSIMPL,
	                JoinType = eJoinType.Digital
	            });

	    [JoinName("UpdateInfoInProgress")] public JoinDataComplete UpdateInfoComplete = new JoinDataComplete(
	        new JoinData{JoinNumber = 2,JoinSpan = 1},
	        new JoinMetadata
	        {
	            Description = "High when update is complete, low when in progress",
	            JoinCapabilities = eJoinCapabilities.ToSIMPL,
	            JoinType = eJoinType.Digital
	        });
		#region Analog

		[JoinName("Device Count")]
		public JoinDataComplete DeviceCount = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Number of devices reporting information",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		#endregion


		#region Serial

        [JoinName("DeviceInfoXSigs")]
		public JoinDataComplete DeviceInfoXSig = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1024
			},
			new JoinMetadata
			{
				Description = "Device Info XSig. One per device",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		#endregion

		/// <summary>
		/// Plugin device BridgeJoinMap constructor
		/// </summary>
		/// <param name="joinStart">This will be the join it starts on the EISC bridge</param>
		public DeviceInfoJoinMap(uint joinStart)
			: base(joinStart, typeof(DeviceInfoJoinMap))
		{
		}
	}
}