/*
 * File: OptiTrackConnectionInfo.cs
 * Purpose: Snapshot of transport and server connection metadata for diagnostics/UI.
 * Scope: Core
 * Notes: Contains potentially sensitive addresses for local UI diagnostics only.
 */

namespace OptiTrack.Core {

	/// <summary>
	/// Represents current connection state and high-level adapter/server metadata.
	/// </summary>
	public sealed class OptiTrackConnectionInfo {

		/// <summary>
		/// Initializes connection info in disconnected state.
		/// </summary>
		public OptiTrackConnectionInfo() {
			Status = OptiTrackConnectionStatus.Disconnected;
		}


		/// <summary>
		/// Gets or sets current connection lifecycle status.
		/// </summary>
		public OptiTrackConnectionStatus Status { get; set; }

		/// <summary>
		/// Gets or sets local endpoint address for local diagnostics.
		/// </summary>
		public string LocalAddress { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets remote Motive/NatNet endpoint address for local diagnostics.
		/// </summary>
		public string ServerAddress { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets host application string reported by SDK.
		/// </summary>
		public string HostApplication { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets NatNet version string reported by SDK.
		/// </summary>
		public string NatNetVersion { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets command channel port.
		/// </summary>
		public int ServerCommandPort { get; set; }

		/// <summary>
		/// Gets or sets data channel port.
		/// </summary>
		public int ServerDataPort { get; set; }

		/// <summary>
		/// Gets whether <see cref="Status"/> indicates an active connection.
		/// </summary>
		public bool IsConnected {
			get { return Status == OptiTrackConnectionStatus.Connected; }
		}

		/// <summary>
		/// Gets or sets selected adapter name.
		/// </summary>
		public string AdapterName {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets human-readable status message for local UI.
		/// </summary>
		public string Message {
			get;
			set;
		}

	}

}
