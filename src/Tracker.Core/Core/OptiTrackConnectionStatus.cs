namespace OptiTrack.Core {

	/// <summary>
	/// Connection lifecycle states for live/replay client transports.
	/// </summary>
	public enum OptiTrackConnectionStatus {

		Disconnected,
		Connecting,
		Connected,
		Disconnecting,
		Faulted

	}

}
