using System;
using System.Threading;
using System.Threading.Tasks;

namespace OptiTrack.Core {

	/// <summary>
	/// Defines the OptiTrack client contract used by Grasshopper components and adapters.
	/// </summary>
	public interface IOptiTrackClient {

		/// <summary>
		/// Gets a value indicating whether the client currently has an active NatNet connection.
		/// </summary>
		bool IsConnected { get; }

		/// <summary>
		/// Gets the latest connection metadata snapshot for the current or last session.
		/// </summary>
		OptiTrackConnectionInfo ConnectionInfo { get; }

		/// <summary>
		/// Raised when a new OptiTrack frame is received from the data stream.
		/// </summary>
		event EventHandler<OptiTrackFrameEventArgs> FrameReceived;

		/// <summary>
		/// Raised when the connection state transitions or status metadata changes.
		/// </summary>
		event EventHandler<OptiTrackConnectionEventArgs> ConnectionChanged;

		/// <summary>
		/// Connects to a NatNet server and starts streaming frames.
		/// </summary>
		/// <param name="options">Connection settings including endpoints and stream options.</param>
		/// <param name="cancellationToken">Cancellation token used to abort connection work.</param>
		/// <returns>A task that completes when the connection attempt finishes.</returns>
		Task ConnectAsync( OptiTrackConnectionOptions options, CancellationToken cancellationToken );

		/// <summary>
		/// Stops streaming and disconnects from the current NatNet server session.
		/// </summary>
		/// <returns>A task that completes when disconnect and cleanup are done.</returns>
		Task DisconnectAsync();
	}
}
