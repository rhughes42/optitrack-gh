namespace OptiTrack.Core {

	/// <summary>
	/// Describes NatNet transport modes supported by the client.
	/// </summary>
	public enum OptiTrackConnectionType {
		/// <summary>
		/// Receive data through multicast networking.
		/// </summary>
		Multicast,
		/// <summary>
		/// Receive data through direct unicast networking.
		/// </summary>
		Unicast
	}

	/// <summary>
	/// Defines connection and stream-shaping options for an OptiTrack session.
	/// </summary>
	public sealed class OptiTrackConnectionOptions {

		/// <summary>
		/// Gets or sets the local adapter IP address used to receive NatNet packets.
		/// </summary>
		public string LocalAddress { get; set; } = "127.0.0.1";

		/// <summary>
		/// Gets or sets the Motive/NatNet server IP address.
		/// </summary>
		public string ServerAddress { get; set; } = "127.0.0.1";

		/// <summary>
		/// Gets or sets the connection transport mode.
		/// </summary>
		public OptiTrackConnectionType ConnectionType { get; set; } = OptiTrackConnectionType.Multicast;

		/// <summary>
		/// Gets or sets the NatNet command port.
		/// </summary>
		public int ServerCommandPort { get; set; } = 1510;

		/// <summary>
		/// Gets or sets the NatNet data port.
		/// </summary>
		public int ServerDataPort { get; set; } = 1511;

		/// <summary>
		/// Gets or sets a value indicating whether marker data should be included in emitted frames.
		/// </summary>
		public bool IncludeMarkers { get; set; } = true;

		/// <summary>
		/// Gets or sets a value indicating whether rigid-body data should be included in emitted frames.
		/// </summary>
		public bool IncludeRigidBodies { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether skeleton data should be included in emitted frames.
		/// </summary>
		public bool IncludeSkeletons { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether force-plate data should be included in emitted frames.
		/// </summary>
		public bool IncludeForcePlates { get; set; }

		/// <summary>
		/// Gets or sets the frame throttling divisor used by the adapter.
		/// </summary>
		public int FrameDivisor { get; set; } = 4;
	}
}
