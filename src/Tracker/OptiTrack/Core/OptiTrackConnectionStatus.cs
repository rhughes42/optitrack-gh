namespace OptiTrack.Core {

	/// <summary>
	/// Represents high-level lifecycle states for a NatNet connection.
	/// </summary>
	public enum OptiTrackConnectionStatus {
		/// <summary>
		/// No active connection exists.
		/// </summary>
		Disconnected,
		/// <summary>
		/// A connection attempt is currently in progress.
		/// </summary>
		Connecting,
		/// <summary>
		/// The client is connected and able to receive frame data.
		/// </summary>
		Connected,
		/// <summary>
		/// A disconnect operation is currently in progress.
		/// </summary>
		Disconnecting,
		/// <summary>
		/// The connection entered an unrecoverable error state.
		/// </summary>
		Faulted
	}
}
