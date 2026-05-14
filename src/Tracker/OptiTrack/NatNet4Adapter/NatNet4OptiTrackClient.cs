using System;
using System.Threading;
using System.Threading.Tasks;

using OptiTrack.Core;
using OptiTrack.NatNet;
using OptiTrack.Telemetry;


namespace OptiTrack.NatNet4Adapter {

	public sealed class NatNet4OptiTrackClient : IOptiTrackClient {

		public const string AdapterName = "NatNet4Adapter";

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
			return SdkCompatibilityReport.Collect(AdapterName, connectionType.ToString());
		}

	}

}
