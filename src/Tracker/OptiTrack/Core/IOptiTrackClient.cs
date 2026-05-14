using System;
using System.Threading;
using System.Threading.Tasks;


namespace OptiTrack.Core {

	public interface IOptiTrackClient {

		bool IsConnected { get; }

		OptiTrackConnectionInfo ConnectionInfo { get; }

		event EventHandler<OptiTrackFrameEventArgs> FrameReceived;

		event EventHandler<OptiTrackConnectionEventArgs> ConnectionChanged;

		Task ConnectAsync(OptiTrackConnectionOptions options, CancellationToken cancellationToken);

		Task DisconnectAsync();

	}

}
