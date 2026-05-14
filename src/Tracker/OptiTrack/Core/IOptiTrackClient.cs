/*
 * File: IOptiTrackClient.cs
 * Purpose: Contract for live/replay OptiTrack frame providers consumed by Grasshopper components.
 * Scope: Core
 * Notes: Implementations may use background threads and external SDKs; consumers should treat events as asynchronous.
 */

using System;
using System.Threading;
using System.Threading.Tasks;


namespace OptiTrack.Core {

	/// <summary>
	/// Defines the minimal contract for an OptiTrack frame source.
	/// </summary>
	/// <remarks>
	/// Implementations may represent live NatNet streams or replay-backed streams.
	/// Event handlers should be thread-safe because callbacks may arrive from non-UI threads.
	/// </remarks>
	public interface IOptiTrackClient {

		/// <summary>
		/// Gets whether the client is currently connected to its underlying source.
		/// </summary>
		bool IsConnected { get; }

		/// <summary>
		/// Gets the latest connection metadata and status.
		/// </summary>
		OptiTrackConnectionInfo ConnectionInfo { get; }

		/// <summary>
		/// Raised when a new <see cref="OptiTrackFrame"/> has been received.
		/// </summary>
		event EventHandler<OptiTrackFrameEventArgs> FrameReceived;

		/// <summary>
		/// Raised when the connection state changes.
		/// </summary>
		event EventHandler<OptiTrackConnectionEventArgs> ConnectionChanged;

		/// <summary>
		/// Connects to the configured frame source.
		/// </summary>
		/// <param name="options">Connection options used by the implementation.</param>
		/// <param name="cancellationToken">Cancellation token for connect attempts.</param>
		/// <returns>A task that completes when connection succeeds or fails.</returns>
		Task ConnectAsync(OptiTrackConnectionOptions options, CancellationToken cancellationToken);

		/// <summary>
		/// Disconnects from the current source and releases transport resources.
		/// </summary>
		/// <returns>A task that completes when disconnect cleanup is finished.</returns>
		Task DisconnectAsync();

	}

}
