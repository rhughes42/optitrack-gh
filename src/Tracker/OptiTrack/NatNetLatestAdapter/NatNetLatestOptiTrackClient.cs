/*
 * File: NatNetLatestOptiTrackClient.cs
 * Purpose: Wrapper adapter for the newest NatNet SDK artifacts available in-repository/runtime.
 * Scope: NatNet
 * Notes: Preserves core models and callback behavior by delegating to the shared NatNet transport implementation.
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using OptiTrack.Core;
using OptiTrack.NatNet;
using OptiTrack.Telemetry;


namespace OptiTrack.NatNetLatestAdapter {

	/// <summary>
	/// Adapter for the newest NatNet SDK artifacts available in the local repository/runtime.
	/// Uses the same internal NatNet transport implementation and preserves existing models.
	/// </summary>
	public sealed class NatNetLatestOptiTrackClient : IOptiTrackClient {

		public const string AdapterName = "NatNetLatestAdapter";

		public const string AdapterVersion = "1.11.0";

		public const string SupportedSdkVersion = "latest_local_available";

		public const string FrameSchemaVersion = "natnet_frame_v1";

		readonly NatNetOptiTrackClient innerClient;


		public NatNetLatestOptiTrackClient(ITelemetryService telemetryService) {
			innerClient = new NatNetOptiTrackClient(telemetryService);
			innerClient.FrameReceived     += (sender, args) => FrameReceived?.Invoke(sender, args);
			innerClient.ConnectionChanged += (sender, args) => ConnectionChanged?.Invoke(sender, args);
		}


		public bool IsConnected {
			get { return innerClient.IsConnected; }
		}

		public OptiTrackConnectionInfo ConnectionInfo {
			get { return innerClient.ConnectionInfo; }
		}

		public event EventHandler<OptiTrackFrameEventArgs> FrameReceived;

		public event EventHandler<OptiTrackConnectionEventArgs> ConnectionChanged;


		public Task ConnectAsync(OptiTrackConnectionOptions connectionOptions, CancellationToken cancellationToken) => innerClient.ConnectAsync(connectionOptions, cancellationToken);


		public Task DisconnectAsync() => innerClient.DisconnectAsync();


		public static SdkCompatibilityReport BuildCompatibilityReport(OptiTrackConnectionType connectionType) => SdkCompatibilityReport.Collect(
				AdapterName,
				AdapterVersion,
				connectionType.ToString(),
				SupportedSdkVersion,
				FrameSchemaVersion);

	}

}
