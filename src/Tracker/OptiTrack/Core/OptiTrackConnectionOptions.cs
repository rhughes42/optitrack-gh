namespace OptiTrack.Core {

	public enum OptiTrackConnectionType {

		Multicast,
		Unicast

	}


	public sealed class OptiTrackConnectionOptions {

		public string LocalAddress { get; set; } = "127.0.0.1";

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
