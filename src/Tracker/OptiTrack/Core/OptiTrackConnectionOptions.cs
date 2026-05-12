namespace OptiTrack.Core {

	public enum OptiTrackConnectionType {
		Multicast,
		Unicast
	}

	public sealed class OptiTrackConnectionOptions {

		public string LocalAddress { get; set; } = "127.0.0.1";

		public string ServerAddress { get; set; } = "127.0.0.1";

		public OptiTrackConnectionType ConnectionType { get; set; } = OptiTrackConnectionType.Multicast;

		public bool IncludeMarkers { get; set; } = true;

		public bool IncludeRigidBodies { get; set; }

		public bool IncludeSkeletons { get; set; }

		public bool IncludeForcePlates { get; set; }

		public int FrameDivisor { get; set; } = 4;
	}
}
