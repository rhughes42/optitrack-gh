namespace OptiTrack.Core {

	public sealed class OptiTrackConnectionInfo {

		public OptiTrackConnectionInfo() {
			Status = OptiTrackConnectionStatus.Disconnected;
		}

		public OptiTrackConnectionStatus Status { get; set; }

		public string LocalAddress { get; set; } = string.Empty;

		public string ServerAddress { get; set; } = string.Empty;

		public string HostApplication { get; set; } = string.Empty;

		public string NatNetVersion { get; set; } = string.Empty;

		public bool IsConnected {
			get { return Status == OptiTrackConnectionStatus.Connected; }
		}
	}
}
