/*
 * File: OptiTrackConnectionOptions.cs
 * Purpose: Connection and stream-shaping settings for NatNet-backed clients.
 * Scope: Core
 * Notes: Contains user-provided network values; do not forward these directly to telemetry.
 */

namespace OptiTrack.Core {

	/// <summary>
	/// Supported NatNet connection modes.
	/// </summary>
	public enum OptiTrackConnectionType {

		Multicast,
		Unicast

	}


	/// <summary>
	/// Immutable-by-convention options used to configure an <see cref="IOptiTrackClient"/> connection.
	/// </summary>
	/// <remarks>
	/// This object is populated from Grasshopper UI inputs and consumed by SDK adapters.
	/// </remarks>
	public sealed class OptiTrackConnectionOptions {

		/// <summary>
		/// Gets or sets the local receiver adapter address.
		/// </summary>
		public string LocalAddress { get; set; } = "127.0.0.1";

		/// <summary>
		/// Gets or sets the remote Motive/NatNet server address.
		/// </summary>
		public string ServerAddress { get; set; } = "127.0.0.1";

		public OptiTrackConnectionType ConnectionType { get; set; } = OptiTrackConnectionType.Multicast;

		public int ServerCommandPort { get; set; } = 1510;

		public int ServerDataPort { get; set; } = 1511;

		public bool IncludeMarkers { get; set; } = true;

		public bool IncludeRigidBodies { get; set; }

		public bool IncludeSkeletons { get; set; }

		public bool IncludeForcePlates { get; set; }

		public int FrameDivisor { get; set; } = 4;

		public double ScaleFactor { get; set; } = 1.0;

		public bool YUp { get; set; }

		public bool DebugLogging { get; set; }

	}

}
