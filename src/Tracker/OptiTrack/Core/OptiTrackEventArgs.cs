using System;

namespace OptiTrack.Core {

	public sealed class OptiTrackFrameEventArgs : EventArgs {

		public OptiTrackFrameEventArgs( OptiTrackFrame frame ) {
			Frame = frame;
		}

		public OptiTrackFrame Frame { get; private set; }
	}

	public sealed class OptiTrackConnectionEventArgs : EventArgs {

		public OptiTrackConnectionEventArgs( OptiTrackConnectionInfo connectionInfo, string message ) {
			ConnectionInfo = connectionInfo;
			Message = message;
		}

		public OptiTrackConnectionInfo ConnectionInfo { get; private set; }

		public string Message { get; private set; }
	}
}
