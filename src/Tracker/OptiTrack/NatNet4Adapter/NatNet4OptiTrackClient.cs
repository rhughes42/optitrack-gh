/*
 * File: NatNet4OptiTrackClient.cs
 * Purpose: Compatibility wrapper adapter for NatNet 4-era deployment workflows.
 * Scope: NatNet
 * Notes: Delegates to the shared NatNet transport implementation to preserve behavior.
 */

using System;
using System.Threading;
using System.Threading.Tasks;

using OptiTrack.Core;
using OptiTrack.NatNet;
using OptiTrack.Telemetry;


namespace OptiTrack.NatNet4Adapter {

	/// <summary>
	/// NatNet 4 compatibility adapter for <see cref="IOptiTrackClient"/>.
	/// </summary>
	public sealed class NatNet4OptiTrackClient : IOptiTrackClient {

		public const string AdapterName = "NatNet4Adapter";
		public const string AdapterVersion = "1.11.0";
		public const string SupportedSdkVersion = "natnet4_compat_mode";
		public const string FrameSchemaVersion = "natnet_frame_v1";

		private readonly NatNetOptiTrackClient innerClient;


		public NatNet4OptiTrackClient(ITelemetryService telemetryService) {
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


		public Task ConnectAsync(OptiTrackConnectionOptions connectionOptions, CancellationToken cancellationToken) {
			return innerClient.ConnectAsync(connectionOptions, cancellationToken);
		}


		public Task DisconnectAsync() {
			return innerClient.DisconnectAsync();
		}


		public static SdkCompatibilityReport BuildCompatibilityReport(OptiTrackConnectionType connectionType) {
			return SdkCompatibilityReport.Collect(
					AdapterName,
					AdapterVersion,
					connectionType.ToString(),
					SupportedSdkVersion,
					FrameSchemaVersion);
		}

	}

}
