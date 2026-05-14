using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

using Grasshopper.Kernel;

using OptiTrack.Core;
using OptiTrack.NatNet;
using OptiTrack.Telemetry;

using Rhino;
using Rhino.Geometry;


namespace Tracker {

	public class TrackerComponent : GH_Component {

		private const int DefaultUpdateIntervalMs = 100;

		private static readonly object            sync = new object();
		private static readonly LatestFrameBuffer frameBuffer = new LatestFrameBuffer();
		private static readonly TrackerLogger     logger      = new TrackerLogger();
		private static          ITelemetryService telemetry   = new NoOpTelemetryService();
		private static          IOptiTrackClient  optiTrackClient = CreateClient(telemetry);
		private static          OptiTrackFrame    currentFrame;
		private static          DateTime          currentFrameReceivedUtc = DateTime.MinValue;
		private static          DateTime          connectionStartedUtc    = DateTime.MinValue;
		private static          bool              handlersAttached;
		private static          bool              connectionConfirmed;
		private static          bool              telemetryEnabled;
		private static          bool              isDeleted;
		private static          bool              redrawEveryFrame;
		private static          int               updateIntervalMs = DefaultUpdateIntervalMs;
		private static          int               reconnectCount;
		private static          long              droppedFrameCount;
		private static          long              skippedFrameCount;
		private static          Timer             updateTimer;
		private static          DateTime          lastSolveUtc = DateTime.MinValue;
		private static          DateTime          lastScheduledSolutionUtc = DateTime.MinValue;
		private static          DateTime          lastFrameUtc = DateTime.MinValue;

		private static bool RigidBody;
		private static bool Skeleton;
		private static bool ForcePlate;
		private static bool yUp;

		public static           List<string>    Log             = new List<string>();
		protected static        List<Point3d>   mPoints         = new List<Point3d>();
		protected static        List<string>    mLabels         = new List<string>();
		private static readonly List<string>    rBodyNames      = new List<string>();
		private static readonly List<Plane>     rBodyPlanes     = new List<Plane>();
		private static readonly List<Transform> rBodyTransforms = new List<Transform>();
		private static readonly List<string>    warnings        = new List<string>();


		public TrackerComponent() : base("OptiTrack Stream", "OptiTrack Stream", "Connect to Motive over NatNet and stream OptiTrack data into Grasshopper.", "Tracker", "OptiTrack") { }


		private static IOptiTrackClient CreateClient(ITelemetryService telemetryService) {
			return new NatNetOptiTrackClient(telemetryService);
		}


		protected override void RegisterInputParams(GH_InputParamManager pManager) {
			pManager.AddBooleanParameter("Connect", "Connect", "Connect to the Motive NatNet stream.", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Reset", "Reset", "Reset the streaming client and clear cached frame data.", GH_ParamAccess.item, false);
			pManager.AddTextParameter("Local IP", "Local IP", "IP address for the receiver network adapter.", GH_ParamAccess.item, "127.0.0.1");
			pManager.AddTextParameter("Server IP", "Server IP", "IP address for the Motive server.", GH_ParamAccess.item, "127.0.0.1");
			pManager.AddTextParameter("Connection Type", "Type", "NatNet connection type: Multicast or Unicast.", GH_ParamAccess.item, "Multicast");
			pManager.AddIntegerParameter("Command Port", "Cmd Port", "NatNet server command port.", GH_ParamAccess.item, 1510);
			pManager.AddIntegerParameter("Data Port", "Data Port", "NatNet server data port.", GH_ParamAccess.item, 1511);
			pManager.AddNumberParameter("Scale Factor", "Scale", "Additional scale factor applied to rigid body planes.", GH_ParamAccess.item, 1.0);
			pManager.AddBooleanParameter("Y Up", "Y Up", "Apply the existing Y-up coordinate adjustment.", GH_ParamAccess.item, false);
			pManager.AddIntegerParameter("Target Update Interval (ms)", "Interval", "Target component update interval in milliseconds.", GH_ParamAccess.item, DefaultUpdateIntervalMs);
			pManager.AddBooleanParameter("Redraw Every Frame", "EveryFrame", "Advanced mode: schedule component solve on each incoming frame.", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Debug Logging", "Debug", "Show additional component runtime remarks.", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized Sentry error/performance reporting when configured.", GH_ParamAccess.item, false);
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
			pManager.AddTextParameter("Status", "Status", "Status of the streaming service.", GH_ParamAccess.list);
			pManager.AddPointParameter("Markers", "Markers", "Generic, unlabelled marker data.", GH_ParamAccess.list);
			pManager.AddTextParameter("Labels", "Labels", "Marker labels.", GH_ParamAccess.list);
			pManager.AddTextParameter("RB Name", "RB Name", "List of Rigid Body name data.", GH_ParamAccess.list);
			pManager.AddPlaneParameter("BR Plane", "BR Plane", "List of planes assosieated with the Ridgid Bodys", GH_ParamAccess.list);
			pManager.AddTransformParameter("RB Transform", "RB XForm", "Rigid body transforms derived from output planes.", GH_ParamAccess.list);
			pManager.AddIntegerParameter("Frame Number", "Frame", "Latest processed NatNet frame number.", GH_ParamAccess.item);
			pManager.AddNumberParameter("Timestamp", "Time", "Latest processed NatNet software timestamp in seconds.", GH_ParamAccess.item);
			pManager.AddNumberParameter("Latency", "Latency", "Approximate seconds since NatNet host transmit timestamp.", GH_ParamAccess.item);
			pManager.AddTextParameter("Warnings", "Warnings", "Validation and runtime warnings.", GH_ParamAccess.list);
			pManager.AddTextParameter("Diagnostics", "Diag", "Frame and connection diagnostics.", GH_ParamAccess.list);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status: disabled, active, or failed.", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA) {
			bool   connect            = false;
			bool   reset              = false;
			string localIP            = "127.0.0.1";
			string serverIP           = "127.0.0.1";
			string connectionTypeText = "Multicast";
			int    commandPort        = 1510;
			int    dataPort           = 1511;
			double scaleFactor        = 1.0;
			bool   yUpInput           = yUp;
			int    targetIntervalMs   = DefaultUpdateIntervalMs;
			bool   everyFrame         = false;
			bool   debugLogging       = false;
			bool   enableTelemetry    = false;

			if (!DA.GetData(0, ref connect) || !DA.GetData(1, ref reset) || !DA.GetData(2, ref localIP) || !DA.GetData(3, ref serverIP))
				return;

			DA.GetData(4, ref connectionTypeText);
			DA.GetData(5, ref commandPort);
			DA.GetData(6, ref dataPort);
			DA.GetData(7, ref scaleFactor);
			DA.GetData(8, ref yUpInput);
			DA.GetData(9, ref targetIntervalMs);
			DA.GetData(10, ref everyFrame);
			DA.GetData(11, ref debugLogging);
			DA.GetData(12, ref enableTelemetry);

			warnings.Clear();
			yUp               = yUpInput;
			updateIntervalMs  = Math.Max(10, targetIntervalMs);
			redrawEveryFrame  = everyFrame;
			logger.DebugEnabled = debugLogging;
			ConfigureTelemetry(enableTelemetry);

			OptiTrackConnectionType connectionType = ParseConnectionType(connectionTypeText);
			bool valid = ValidateConfiguration(localIP, serverIP, commandPort, dataPort, scaleFactor, targetIntervalMs, connectionTypeText);

			foreach (string warning in warnings) {
				AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
			}

			if (!valid) {
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Tracker configuration is invalid. Fix warnings before connecting.");
				SetOutputs(DA);

				return;
			}

			if (reset) {
				ClearFrameState();
				DisconnectClient();
				AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Tracker client reset.");
			}

			if (connect && !connectionConfirmed) {
				ConnectClient(localIP, serverIP, connectionType, commandPort, dataPort, scaleFactor, debugLogging);
			}
			else if (!connect && connectionConfirmed) {
				DisconnectClient();
				ClearFrameState();
				logger.Info("Service stopped. Set Connect to true to begin streaming.");
			}

			Stopwatch solveWatch = Stopwatch.StartNew();
			using (telemetry.StartSpan("grasshopper.solve", new TelemetryContext().SetMetric("update_interval_ms", updateIntervalMs))) {
				ConsumeLatestFrame(scaleFactor);
			}
			solveWatch.Stop();
			telemetry.CaptureMessage("grasshopper.solve", TelemetrySeverity.Debug, new TelemetryContext().SetMetric("solve_duration_ms", solveWatch.Elapsed.TotalMilliseconds));

			lastSolveUtc = DateTime.UtcNow;
			SetOutputs(DA);
		}


		private void SetOutputs(IGH_DataAccess DA) {
			try {
				DA.SetDataList("Status", logger.Snapshot());
				DA.SetDataList("Markers", mPoints);
				DA.SetDataList("Labels", mLabels);
				DA.SetDataList("RB Name", rBodyNames);
				DA.SetDataList("BR Plane", rBodyPlanes);
				DA.SetDataList("RB Transform", rBodyTransforms);
				DA.SetData("Frame Number", currentFrame == null ? 0 : currentFrame.FrameNumber);
				DA.SetData("Timestamp", currentFrame == null ? 0.0 : currentFrame.TimestampSeconds);
				DA.SetData("Latency", currentFrame == null ? 0.0 : currentFrame.LatencySeconds);
				DA.SetDataList("Warnings", warnings);
				DA.SetDataList("Diagnostics", BuildDiagnostics());
				DA.SetData("Telemetry Status", telemetry.Status);
			}
			catch (Exception exception) {
				telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "grasshopper_set_output"));
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to set Tracker outputs.");
			}
		}


		private static void ConfigureTelemetry(bool enableTelemetry) {
			if (telemetryEnabled == enableTelemetry) {
				return;
			}

			telemetryEnabled = enableTelemetry;
			(telemetry as IDisposable)?.Dispose();
			telemetry = SentryTelemetryService.Create(enableTelemetry);
			ResetClient();
		}


		private static void ResetClient() {
			if (optiTrackClient != null && handlersAttached) {
				optiTrackClient.FrameReceived     -= OnFrameReceived;
				optiTrackClient.ConnectionChanged -= OnConnectionChanged;
			}

			optiTrackClient  = CreateClient(telemetry);
			handlersAttached = false;
		}


		private static void ConnectClient(string localIP, string serverIP, OptiTrackConnectionType connectionType, int commandPort, int dataPort, double scaleFactor, bool debugLogging) {
			EnsureClientHandlers();
			logger.Clear();
			logger.DebugEnabled = debugLogging;
			logger.Info("Attempting connection to Motive NatNet server...");
			connectionStartedUtc = DateTime.UtcNow;
			lastFrameUtc         = DateTime.MinValue;

			OptiTrackConnectionOptions options = new OptiTrackConnectionOptions {
					LocalAddress       = localIP,
					ServerAddress      = serverIP,
					ConnectionType     = connectionType,
					ServerCommandPort  = commandPort,
					ServerDataPort     = dataPort,
					IncludeMarkers     = true,
					IncludeRigidBodies = RigidBody,
					IncludeSkeletons   = Skeleton,
					IncludeForcePlates = ForcePlate,
					FrameDivisor       = 1,
					ScaleFactor        = scaleFactor,
					YUp                = yUp,
					DebugLogging       = debugLogging
			};

			try {
				optiTrackClient.ConnectAsync(options, CancellationToken.None).GetAwaiter().GetResult();
				connectionConfirmed = optiTrackClient.IsConnected;
				if (connectionConfirmed) {
					StartUpdateTimer();
					logger.Info("Success: Data port connected.");
					AddRuntimeMessageToActiveInstances("Tracker connected.", GH_RuntimeMessageLevel.Remark);
				}
			}
			catch (Exception exception) {
				connectionConfirmed = false;
				logger.Warn("Error: Failed to connect. Check Motive broadcast settings, IP addresses, ports, firewall, and connection type.");
				telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "component_connect"));
			}
		}


		private static void DisconnectClient() {
			StopUpdateTimer();
			if (optiTrackClient == null || !optiTrackClient.IsConnected) {
				connectionConfirmed = false;

				return;
			}

			using (telemetry.StartSpan("natnet.disconnect", new TelemetryContext().SetMetric("reconnect_count", reconnectCount))) {
				optiTrackClient.DisconnectAsync().GetAwaiter().GetResult();
			}

			connectionConfirmed = false;
		}


		private static void EnsureClientHandlers() {
			if (handlersAttached) {
				return;
			}

			optiTrackClient.FrameReceived     += OnFrameReceived;
			optiTrackClient.ConnectionChanged += OnConnectionChanged;
			handlersAttached                  =  true;
		}


		private static void OnFrameReceived(object sender, OptiTrackFrameEventArgs e) {
			if (e == null || e.Frame == null || isDeleted) {
				return;
			}

			DateTime now = DateTime.UtcNow;
			frameBuffer.Write(e.Frame, now);
			lastFrameUtc = now;

			if (redrawEveryFrame) {
				ScheduleSolutionSoon();
			}

			telemetry.CaptureMessage("natnet.frame_received", TelemetrySeverity.Debug, new TelemetryContext()
					.SetMetric("frame_count", frameBuffer.TotalReceived)
					.SetMetric("rigid_body_count", e.Frame.RigidBodies == null ? 0 : e.Frame.RigidBodies.Count)
					.SetMetric("marker_count", e.Frame.Markers == null ? 0 : e.Frame.Markers.Count));
		}


		private static void OnConnectionChanged(object sender, OptiTrackConnectionEventArgs e) {
			if (!string.IsNullOrWhiteSpace(e.Message)) {
				logger.Info(e.Message);
			}
		}


		private static void ClearFrameState() {
			lock (sync) {
				currentFrame = null;
				currentFrameReceivedUtc = DateTime.MinValue;
				mPoints.Clear();
				mLabels.Clear();
				rBodyNames.Clear();
				rBodyPlanes.Clear();
				rBodyTransforms.Clear();
				warnings.Clear();
			}
		}


		private static void ConsumeLatestFrame(double scaleFactor) {
			using (telemetry.StartSpan("frame_buffer.consume", new TelemetryContext().SetMetric("frame_count", frameBuffer.TotalReceived))) {
				if (!frameBuffer.TryConsumeLatest(out OptiTrackFrame frame, out DateTime receivedUtc, out bool skippedIntermediateFrames)) {
					return;
				}

				if (skippedIntermediateFrames) {
					Interlocked.Increment(ref skippedFrameCount);
				}

				if (frame.FrameNumber == (currentFrame == null ? -1 : currentFrame.FrameNumber)) {
					Interlocked.Increment(ref droppedFrameCount);
				}

				currentFrame            = frame;
				currentFrameReceivedUtc = receivedUtc;
				ProcessFrameData(frame, scaleFactor);
			}
		}


		private static void ProcessFrameData(OptiTrackFrame frame, double scaleFactor) {
			if (frame == null) {
				return;
			}

			Stopwatch conversionWatch = Stopwatch.StartNew();
			using (telemetry.StartSpan("grasshopper.geometry_conversion", new TelemetryContext())) {
				mPoints.Clear();
				mLabels.Clear();
				rBodyNames.Clear();
				rBodyPlanes.Clear();
				rBodyTransforms.Clear();

				foreach (OptiTrackMarker marker in frame.Markers) {
					mLabels.Add(marker.Label);
					mPoints.Add(new Point3d(marker.X * scaleFactor, marker.Y * scaleFactor, marker.Z * scaleFactor));
				}

				if (RigidBody) {
					foreach (OptiTrackRigidBody rigidBody in frame.RigidBodies) {
						if (!rigidBody.IsTracked) {
							continue;
						}

						rBodyNames.Add(rigidBody.Name);
						Plane plane = CreateRigidBodyPlane(rigidBody, scaleFactor);
						rBodyPlanes.Add(plane);
						rBodyTransforms.Add(Transform.PlaneToPlane(Plane.WorldXY, plane));
					}
				}
			}
			conversionWatch.Stop();
			telemetry.CaptureMessage("grasshopper.geometry_conversion", TelemetrySeverity.Debug, new TelemetryContext().SetMetric("conversion_duration_ms", conversionWatch.Elapsed.TotalMilliseconds));
		}


		private static void StartUpdateTimer() {
			StopUpdateTimer();
			updateTimer = new Timer(_ => ScheduleSolutionSoon(), null, updateIntervalMs, updateIntervalMs);
		}

		private static void StopUpdateTimer() {
			updateTimer?.Dispose();
			updateTimer = null;
		}


		private static void ScheduleSolutionSoon() {
			if (isDeleted) {
				return;
			}

			DateTime now = DateTime.UtcNow;
			if (!redrawEveryFrame && now.Subtract(lastScheduledSolutionUtc).TotalMilliseconds < updateIntervalMs * 0.8) {
				return;
			}

			lastScheduledSolutionUtc = now;

			var doc = Instances.ActiveCanvas?.Document;
			doc?.ScheduleSolution(1, _ => { Instances.ActiveCanvas?.Document?.ExpireSolution(); });
		}


		private static List<string> BuildDiagnostics() {
			List<string> lines = new List<string>();
			TimeSpan uptime = connectionStartedUtc == DateTime.MinValue ? TimeSpan.Zero : DateTime.UtcNow.Subtract(connectionStartedUtc);
			double ageMs = currentFrameReceivedUtc == DateTime.MinValue ? 0.0 : DateTime.UtcNow.Subtract(currentFrameReceivedUtc).TotalMilliseconds;
			lines.Add("status=" + (connectionConfirmed ? "connected" : "disconnected"));
			lines.Add("telemetry_status=" + telemetry.Status);
			lines.Add("frames_received=" + frameBuffer.TotalReceived);
			lines.Add("frames_consumed=" + frameBuffer.TotalConsumed);
			lines.Add("skipped_frame_count=" + Interlocked.Read(ref skippedFrameCount));
			lines.Add("dropped_frame_count=" + Interlocked.Read(ref droppedFrameCount));
			lines.Add("last_frame_timestamp_utc=" + (lastFrameUtc == DateTime.MinValue ? "n/a" : lastFrameUtc.ToString("O")));
			lines.Add("buffer_age_ms=" + ageMs.ToString("F2"));
			lines.Add("uptime_seconds=" + uptime.TotalSeconds.ToString("F1"));
			lines.Add("reconnect_count=" + reconnectCount);
			lines.Add("last_solve_timestamp_utc=" + (lastSolveUtc == DateTime.MinValue ? "n/a" : lastSolveUtc.ToString("O")));

			telemetry.CaptureMessage("diagnostics", TelemetrySeverity.Debug, new TelemetryContext()
					.SetMetric("frame_count", frameBuffer.TotalReceived)
					.SetMetric("skipped_frame_count", Interlocked.Read(ref skippedFrameCount))
					.SetMetric("dropped_frame_count", Interlocked.Read(ref droppedFrameCount))
					.SetMetric("buffer_age_ms", ageMs)
					.SetMetric("reconnect_count", reconnectCount));

			return lines;
		}


		private static void AddRuntimeMessageToActiveInstances(string message, GH_RuntimeMessageLevel level) {
			IList<IGH_DocumentObject> objects = Instances.ActiveCanvas?.Document?.Objects;
			if (objects == null) {
				return;
			}

			foreach (IGH_DocumentObject obj in objects) {
				TrackerComponent component = obj as TrackerComponent;
				component?.AddRuntimeMessage(level, message);
			}
		}


		private static Plane CreateRigidBodyPlane(OptiTrackRigidBody rigidBody, double scaleFactor) {
			Quaternion rbQuat = new Quaternion(rigidBody.Qw, rigidBody.Qx, rigidBody.Qy, rigidBody.Qz);
			rbQuat.GetRotation(out Plane rbPlane);
			rbPlane.Origin = new Point3d(rigidBody.X, rigidBody.Y, rigidBody.Z);

			var doc = RhinoDoc.ActiveDoc;
			if (doc != null && doc.ModelUnitSystem == UnitSystem.Millimeters) {
				rbPlane.Transform(Transform.Scale(new Point3d(0, 0, 0), 1000));
			}

			if (Math.Abs(scaleFactor - 1.0) > 0.000001) {
				rbPlane.Transform(Transform.Scale(new Point3d(0, 0, 0), scaleFactor));
			}

			if (yUp) {
				Transform xformYup = Transform.Identity;
				xformYup.M00 = xformYup.M21 = xformYup.M33 = 1;
				xformYup.M12 = -1;
				rbPlane.Transform(xformYup);
			}

			Transform xRotate = Transform.Identity;
			xRotate.M10 = xRotate.M22 = xRotate.M33 = 1;
			xRotate.M01 = -1;
			rbPlane.Transform(xRotate);

			return rbPlane;
		}


		private bool ValidateConfiguration(string localIP, string serverIP, int commandPort, int dataPort, double scaleFactor, int targetInterval, string connectionTypeText) {
			bool valid = true;
			if (!IPAddress.TryParse(localIP, out _)) { warnings.Add("Local IP address is invalid."); valid = false; }
			if (!IPAddress.TryParse(serverIP, out _)) { warnings.Add("Server IP address is invalid."); valid = false; }
			if (!IsValidPort(commandPort)) { warnings.Add("Command port must be between 1 and 65535."); valid = false; }
			if (!IsValidPort(dataPort)) { warnings.Add("Data port must be between 1 and 65535."); valid = false; }
			if (commandPort == dataPort) { warnings.Add("Command port and data port should be different."); valid = false; }
			if (scaleFactor <= 0) { warnings.Add("Scale factor must be greater than zero."); valid = false; }
			if (targetInterval < 1) { warnings.Add("Target update interval must be 1 ms or greater."); valid = false; }
			if (ParseConnectionType(connectionTypeText) == OptiTrackConnectionType.Multicast && !IsMulticastText(connectionTypeText)) {
				warnings.Add("Connection type was not recognized. Using Multicast.");
			}
			if (!NatNetDependenciesPresent()) { warnings.Add("NatNetML.dll or NatNetLib.dll is missing from the plugin output folder."); valid = false; }
			if (telemetryEnabled && telemetry.Status.StartsWith("disabled", StringComparison.OrdinalIgnoreCase)) {
				warnings.Add("Telemetry was enabled, but Sentry is not configured. Set SENTRY_DSN or tracker.telemetry.local.json to activate it.");
			}

			return valid;
		}

		private static bool IsValidPort(int port) { return port >= 1 && port <= 65535; }

		private static bool NatNetDependenciesPresent() {
			string assemblyLocation = typeof(TrackerComponent).Assembly.Location;
			string baseDirectory = string.IsNullOrWhiteSpace(assemblyLocation) ? AppDomain.CurrentDomain.BaseDirectory : Path.GetDirectoryName(assemblyLocation);
			return File.Exists(Path.Combine(baseDirectory, "NatNetML.dll")) && File.Exists(Path.Combine(baseDirectory, "NatNetLib.dll"));
		}

		private static OptiTrackConnectionType ParseConnectionType(string value) { return string.Equals(value, "Unicast", StringComparison.OrdinalIgnoreCase) ? OptiTrackConnectionType.Unicast : OptiTrackConnectionType.Multicast; }

		private static bool IsMulticastText(string value) { return string.Equals(value, "Multicast", StringComparison.OrdinalIgnoreCase); }


		protected override void RemovedFromDocument(GH_Document document) {
			isDeleted = true;
			DisconnectClient();
			base.RemovedFromDocument(document);
		}


		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
			Menu_AppendItem(menu, "Y up", Menu_yUp, true, yUp).ToolTipText = "When checked, component will expect Y up";
			Menu_AppendItem(menu, "Rigid Body", Menu_rBodyClick, true, RigidBody).ToolTipText = "When checked, component will stream Rigid Body data.";
			Menu_AppendItem(menu, "Skeleton", Menu_skeletonClick, true, Skeleton).ToolTipText = "When checked, component will stream Skeleton data.";
			Menu_AppendItem(menu, "Force Plate", Menu_fPlateClick, true, ForcePlate).ToolTipText = "When checked, component will stream Force Plate data.";
		}

		private void Menu_yUp(object sender, EventArgs e) { RecordUndoEvent("Y Up"); yUp = !yUp; ExpireSolution(true); }
		private void Menu_rBodyClick(object sender, EventArgs e) { RecordUndoEvent("Rigid Body"); RigidBody = !RigidBody; ExpireSolution(true); }
		private void Menu_skeletonClick(object sender, EventArgs e) { RecordUndoEvent("Skeleton"); Skeleton = !Skeleton; ExpireSolution(true); }
		private void Menu_fPlateClick(object sender, EventArgs e) { RecordUndoEvent("Force Plate"); ForcePlate = !ForcePlate; ExpireSolution(true); }

		protected override System.Drawing.Bitmap Icon { get { return Properties.Icons.Tracker; } }
		public override Guid ComponentGuid { get { return new Guid("D1C724B2-FEB4-405E-9570-382F8FD553F8"); } }
	}

}
