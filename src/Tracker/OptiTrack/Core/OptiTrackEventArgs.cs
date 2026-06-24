using System;

namespace OptiTrack.Core {

	/// <summary>
	/// Event payload for frame-received notifications.
	/// </summary>
	public sealed class OptiTrackFrameEventArgs : EventArgs {

		/// <summary>
		/// Initializes frame event arguments.
		/// </summary>
		/// <param name="frame">The frame received from the OptiTrack stream.</param>
		public OptiTrackFrameEventArgs( OptiTrackFrame frame ) {
			Frame = frame;
		}

		/// <summary>
		/// Gets the received frame.
		/// </summary>
		public OptiTrackFrame Frame { get; private set; }
	}

	/// <summary>
	/// Event payload for connection state-change notifications.
	/// </summary>
	public sealed class OptiTrackConnectionEventArgs : EventArgs {

		/// <summary>
		/// Initializes connection event arguments.
		/// </summary>
		/// <param name="connectionInfo">The latest connection metadata.</param>
		/// <param name="message">A human-readable status message for the transition.</param>
		public OptiTrackConnectionEventArgs( OptiTrackConnectionInfo connectionInfo, string message ) {
			ConnectionInfo = connectionInfo;
			Message = message;
		}

		/// <summary>
		/// Gets the latest connection metadata.
		/// </summary>
		public OptiTrackConnectionInfo ConnectionInfo { get; private set; }

		/// <summary>
		/// Gets a status message associated with the connection change.
		/// </summary>
		public string Message { get; private set; }
	}
}
