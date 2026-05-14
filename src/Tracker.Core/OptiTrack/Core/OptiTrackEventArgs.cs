/*
 * File: OptiTrackEventArgs.cs
 * Purpose: Event payload types for frame and connection notifications.
 * Scope: Core
 * Notes: Frame events may originate from non-UI threads depending on adapter implementation.
 */

using System;


namespace OptiTrack.Core {

	/// <summary>
	/// Event payload for a received <see cref="OptiTrackFrame"/>.
	/// </summary>
	public sealed class OptiTrackFrameEventArgs : EventArgs {

		public OptiTrackFrameEventArgs(OptiTrackFrame frame) {
			Frame = frame;
		}


		public OptiTrackFrame Frame { get; private set; }

	}


	/// <summary>
	/// Event payload for connection status changes.
	/// </summary>
	public sealed class OptiTrackConnectionEventArgs : EventArgs {

		public OptiTrackConnectionEventArgs(OptiTrackConnectionInfo connectionInfo, string message) {
			ConnectionInfo = connectionInfo;
			Message        = message;
		}


		public OptiTrackConnectionInfo ConnectionInfo { get; private set; }

		public string Message { get; private set; }

	}

}
