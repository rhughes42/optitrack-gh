using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NatNetML;

using OptiTrack.Core;
using OptiTrack.Telemetry;


namespace OptiTrack.NatNet {

	public sealed class NatNetOptiTrackClient : IOptiTrackClient {

		private readonly object                     syncRoot = new object();
		private readonly ITelemetryService          telemetryService;
		private          NatNetClientML             natNetClient;
		private          OptiTrackConnectionOptions options;
		private          List<DataDescriptor>       dataDescriptors;
		private          List<RigidBody>            rigidBodies;
		private          List<Skeleton>             skeletons;
		private          List<ForcePlate>           forcePlates;
		private          List<string>               statusMessages;
		private          bool                       assetsChanged;


		public NatNetOptiTrackClient(ITelemetryService telemetryService) {
			this.telemetryService = telemetryService ?? new NoOpTelemetryService();
			natNetClient          = new NatNetClientML();
			options               = new OptiTrackConnectionOptions();
			ConnectionInfo        = new OptiTrackConnectionInfo();
			dataDescriptors       = new List<DataDescriptor>();
			rigidBodies           = new List<RigidBody>();
			skeletons             = new List<Skeleton>();
			forcePlates           = new List<ForcePlate>();
			statusMessages        = new List<string>();
		}


		public bool IsConnected {
			get { return ConnectionInfo.IsConnected; }
		}

		public OptiTrackConnectionInfo ConnectionInfo { get; private set; }

		public event EventHandler<OptiTrackFrameEventArgs> FrameReceived;

		public event EventHandler<OptiTrackConnectionEventArgs> ConnectionChanged;


		public Task ConnectAsync(OptiTrackConnectionOptions connectionOptions, CancellationToken cancellationToken) {
			return Task.Run(() => Connect(connectionOptions, cancellationToken), cancellationToken);
		}


		public Task DisconnectAsync() {
			return Task.Run(() => Disconnect());
		}


		private void Connect(OptiTrackConnectionOptions connectionOptions, CancellationToken cancellationToken) {
			OptiTrackConnectionOptions requestedOptions = connectionOptions ?? new OptiTrackConnectionOptions();

			using (telemetryService.StartSpan("natnet.connect", new TelemetryContext().SetTag("connection_type", requestedOptions.ConnectionType.ToString()))) {
				cancellationToken.ThrowIfCancellationRequested();
				options              = requestedOptions;
				options.FrameDivisor = Math.Max(1, options.FrameDivisor);

				SetConnectionStatus(OptiTrackConnectionStatus.Connecting, "Attempting connection to server...");

				try {
					lock (syncRoot) {
						natNetClient = new NatNetClientML();
						statusMessages.Clear();
						statusMessages.Add("NatNet managed client application starting...");
						statusMessages.Add("Local IP set.");
						statusMessages.Add("Server IP set.");

						int[] natNetVersion = natNetClient.NatNetVersion();
						ConnectionInfo.NatNetVersion = string.Format("{0}.{1}.{2}.{3}", natNetVersion[0], natNetVersion[1], natNetVersion[2], natNetVersion[3]);

						NatNetClientML.ConnectParams connectParams = new NatNetClientML.ConnectParams {
								ConnectionType    = ToNatNetConnectionType(options.ConnectionType),
								ServerAddress     = options.ServerAddress,
								LocalAddress      = options.LocalAddress,
								ServerCommandPort = (ushort)options.ServerCommandPort,
								ServerDataPort    = (ushort)options.ServerDataPort
						};

						natNetClient.Connect(connectParams);
						FetchServerDescriptor();
						FetchDataDescriptions();
						natNetClient.OnFrameReady += OnFrameReady;
					}

					SetConnectionStatus(OptiTrackConnectionStatus.Connected, "Success: Data Port Connected.");
				}
				catch (Exception exception) {
					telemetryService.CaptureException(exception, new TelemetryContext().SetTag("operation", "natnet_connect"));
					SetConnectionStatus(OptiTrackConnectionStatus.Faulted, "Error: Failed to connect. Check the connection settings.");

					throw;
				}
			}
		}


		private void Disconnect() {
			using (telemetryService.StartSpan("natnet.disconnect", new TelemetryContext())) {
				SetConnectionStatus(OptiTrackConnectionStatus.Disconnecting, "Disconnecting from server...");

				try {
					lock (syncRoot) {
						natNetClient.OnFrameReady -= OnFrameReady;
						natNetClient.Disconnect();
						dataDescriptors.Clear();
						rigidBodies.Clear();
						skeletons.Clear();
						forcePlates.Clear();
						statusMessages.Clear();
						statusMessages.Add("Service stopped. Activate module to begin streaming.");
					}

					SetConnectionStatus(OptiTrackConnectionStatus.Disconnected, "Service stopped. Activate module to begin streaming.");
				}
				catch (Exception exception) {
					telemetryService.CaptureException(exception, new TelemetryContext().SetTag("operation", "natnet_disconnect"));
					SetConnectionStatus(OptiTrackConnectionStatus.Faulted, "Error: Failed to disconnect cleanly.");
				}
			}
		}


		private void FetchServerDescriptor() {
			ServerDescription serverDescription = new ServerDescription();
			int               errorCode         = natNetClient.GetServerDescription(serverDescription);

			if (errorCode == 0) {
				statusMessages.Add("Success: Connected to the server.");
				statusMessages.Add("Server Info:");
				statusMessages.Add("Host: " + serverDescription.HostComputerName);
				statusMessages.Add("Application Name: " + serverDescription.HostApp);
				ConnectionInfo.HostApplication   = serverDescription.HostApp;
				ConnectionInfo.LocalAddress      = options.LocalAddress;
				ConnectionInfo.ServerAddress     = options.ServerAddress;
				ConnectionInfo.ServerCommandPort = options.ServerCommandPort;
				ConnectionInfo.ServerDataPort    = options.ServerDataPort;
			}
			else {
				statusMessages.Add("Error: Failed to connect. Check the connection settings.");
				statusMessages.Add("Program terminated.");
			}
		}


		private void FetchDataDescriptions() {
			bool result = natNetClient.GetDataDescriptions(out dataDescriptors);

			if (!result) {
				statusMessages.Add("Error: Could not get the Data Descriptions");

				return;
			}

			statusMessages.Add("Success: Data Descriptions obtained from the server.");
			statusMessages.Add("Total " + dataDescriptors.Count + " data sets in the capture:");
			rigidBodies.Clear();
			skeletons.Clear();
			forcePlates.Clear();

			for (int i = 0; i < dataDescriptors.Count; i++) {
				DataDescriptor descriptor = dataDescriptors[i];

				switch (descriptor.type) {
					case (int)DataDescriptorType.eMarkerSetData:

						MarkerSet markerSet = (MarkerSet)descriptor;
						statusMessages.Add("MarkerSet (" + markerSet.Name + ")");

					break;

					case (int)DataDescriptorType.eRigidbodyData:

						RigidBody rigidBody = (RigidBody)descriptor;
						statusMessages.Add("RigidBody (" + rigidBody.Name + ")");
						rigidBodies.Add(rigidBody);

					break;

					case (int)DataDescriptorType.eSkeletonData: skeletons.Add((Skeleton)descriptor); break;

					case (int)DataDescriptorType.eForcePlateData: forcePlates.Add((ForcePlate)descriptor); break;

					default: statusMessages.Add("Error: Invalid Data Set"); break;
				}
			}
		}


		private void OnFrameReady(FrameOfMocapData data, NatNetClientML client) {
			if (data.iFrame % options.FrameDivisor != 0) {
				return;
			}

			try {
				OptiTrackFrame frame;

				using (telemetryService.StartSpan("natnet.frame_received", new TelemetryContext().SetMetric("frame_count", data.iFrame))) {
					lock (syncRoot) {
						if (data.bTrackingModelsChanged == true
							|| data.nRigidBodies != rigidBodies.Count
							|| data.nSkeletons != skeletons.Count
							|| data.nForcePlates != forcePlates.Count
							|| assetsChanged) {
							assetsChanged = false;
							FetchDataDescriptions();
						}

						frame = NatNetFrameConverter.ConvertFrame(data, client, rigidBodies, options, new List<string>(statusMessages));
					}

					EventHandler<OptiTrackFrameEventArgs> handler = FrameReceived;

					if (handler != null) {
						handler(this, new OptiTrackFrameEventArgs(frame));
					}
				}
			}
			catch (Exception exception) {
				telemetryService.CaptureException(exception, new TelemetryContext().SetTag("operation", "natnet_frame_conversion"));
			}
		}


		private void SetConnectionStatus(OptiTrackConnectionStatus status, string message) {
			ConnectionInfo.Status = status;

			EventHandler<OptiTrackConnectionEventArgs> handler = ConnectionChanged;

			if (handler != null) {
				handler(this, new OptiTrackConnectionEventArgs(ConnectionInfo, message));
			}
		}


		private static ConnectionType ToNatNetConnectionType(OptiTrackConnectionType connectionType) {
			return connectionType == OptiTrackConnectionType.Unicast ? ConnectionType.Unicast : ConnectionType.Multicast;
		}

	}

}
