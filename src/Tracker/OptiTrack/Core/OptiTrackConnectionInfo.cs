namespace OptiTrack.Core {

	/// <summary>
	/// Stores connection metadata and status details for the current NatNet session.
	/// </summary>
	public sealed class OptiTrackConnectionInfo {

		/// <summary>
		/// Initializes a new connection info instance in the disconnected state.
		/// </summary>
		public OptiTrackConnectionInfo() {
			Status = OptiTrackConnectionStatus.Disconnected;
		}

		/// <summary>
		/// Gets or sets the current connection lifecycle state.
		/// </summary>
		public OptiTrackConnectionStatus Status { get; set; }

		/// <summary>
		/// Gets or sets the local adapter IP address used by the client.
		/// </summary>
		public string LocalAddress { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the remote Motive/NatNet server IP address.
		/// </summary>
		public string ServerAddress { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the NatNet host application name reported by the server.
		/// </summary>
		public string HostApplication { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the NatNet protocol version reported by the server.
		/// </summary>
		public string NatNetVersion { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the server command port.
		/// </summary>
		public int ServerCommandPort { get; set; }

		/// <summary>
		/// Gets or sets the server data port.
		/// </summary>
		public int ServerDataPort { get; set; }

		/// <summary>
		/// Gets a value indicating whether <see cref="Status"/> is <see cref="OptiTrackConnectionStatus.Connected"/>.
		/// </summary>
		public bool IsConnected {
			get { return Status == OptiTrackConnectionStatus.Connected; }
		}
	}
}
