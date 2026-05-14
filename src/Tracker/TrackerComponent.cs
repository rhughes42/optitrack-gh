using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

using Grasshopper.Kernel;

using OptiTrack.Core;
using OptiTrack.NatNet;
using OptiTrack.Telemetry;

using Rhino.Geometry;


namespace Tracker {

	/// <summary>
	/// The main tracker component.
	/// </summary>
	public class TrackerComponent : GH_Component {

		private static ITelemetryService telemetry       = new NoOpTelemetryService();
		private static IOptiTrackClient  optiTrackClient = CreateClient(telemetry);
		private static OptiTrackFrame    currentFrame;
		private static DateTime          connectionStartedUtc = DateTime.MinValue;
		private static DateTime          lastFrameUtc         = DateTime.MinValue;
		private static bool              handlersAttached;
		private static bool              connectionConfirmed;
		private static bool              telemetryEnabled;
		private static int               counter;

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


		public TrackerComponent() : base(
				"OptiTrack Stream",
				"OptiTrack Stream",
				"Connect to Motive over NatNet and stream OptiTrack data into Grasshopper.",
				"Tracker",
				"OptiTrack") { }


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
			pManager.AddIntegerParameter("Redraw Throttle", "Throttle", "Process every Nth NatNet frame. Use 1 for every frame.", GH_ParamAccess.item, 4);
			pManager.AddBooleanParameter("Debug Logging", "Debug", "Show additional component runtime remarks.", GH_ParamAccess.item, false);

			pManager.AddBooleanParameter(
					"Enable Telemetry",
					"Telemetry",
					"Enable optional sanitized Sentry error reporting when configured.",
					GH_ParamAccess.item,
					false);
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
			int    redrawThrottle     = 4;
			bool   debugLogging       = false;
			bool   enableTelemetry    = false;

			if (!DA.GetData(0, ref connect))
				return;

			if (!DA.GetData(1, ref reset))
				return;

			if (!DA.GetData(2, ref localIP))
				return;

			if (!DA.GetData(3, ref serverIP))
				return;

			DA.GetData(4, ref connectionTypeText);
			DA.GetData(5, ref commandPort);
			DA.GetData(6, ref dataPort);
			DA.GetData(7, ref scaleFactor);
			DA.GetData(8, ref yUpInput);
			DA.GetData(9, ref redrawThrottle);
			DA.GetData(10, ref debugLogging);
			DA.GetData(11, ref enableTelemetry);

			warnings.Clear();
			yUp = yUpInput;
			ConfigureTelemetry(enableTelemetry);

			OptiTrackConnectionType connectionType = ParseConnectionType(connectionTypeText);

			bool valid = ValidateConfiguration(
					localIP,
					serverIP,
					commandPort,
					dataPort,
					scaleFactor,
					redrawThrottle,
					connectionTypeText);

			foreach (string warning in warnings) {
				AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
			}

			if (!valid) {
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Tracker configuration is invalid. Fix warnings before connecting.");

				telemetry.CaptureMessage(
						"invalid_tracker_configuration",
						TelemetrySeverity.Warning,
						new TelemetryContext().SetTag("operation", "component_validation"));

				SetOutputs(DA);

				return;
			}

			if (reset) {
				ClearFrameState();
				DisconnectClient();
				counter = 0;
				AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Tracker client reset.");
			}

			if (connect && counter == 0) {
				ConnectClient(
						localIP,
						serverIP,
						connectionType,
						commandPort,
						dataPort,
						scaleFactor,
						redrawThrottle,
						debugLogging);

				counter++;
			}
			else if (connect && connectionConfirmed) {
				ProcessFrameData(currentFrame, scaleFactor);
				ReportNoFrameWarning();
				counter++;
			}
			else if (!connect) {
				DisconnectClient();
				ClearFrameState();
				Log.Add("Service stopped. Set Connect to true to begin streaming.");
			}

			if (debugLogging) {
				AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Telemetry: " + telemetry.Status);
			}

			SetOutputs(DA);
			ExpireSolution(true);
		}


		private void SetOutputs(IGH_DataAccess DA) {
			try {
				DA.SetDataList("Status", Log);
				DA.SetDataList("Markers", mPoints);
				DA.SetDataList("Labels", mLabels);
				DA.SetDataList("RB Name", rBodyNames);
				DA.SetDataList("BR Plane", rBodyPlanes);
				DA.SetDataList("RB Transform", rBodyTransforms);
				DA.SetData("Frame Number", currentFrame == null ? 0 : currentFrame.FrameNumber);
				DA.SetData("Timestamp", currentFrame == null ? 0.0 : currentFrame.TimestampSeconds);
				DA.SetData("Latency", currentFrame == null ? 0.0 : currentFrame.LatencySeconds);
				DA.SetDataList("Warnings", warnings);
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
			IDisposable disposableTelemetry = telemetry as IDisposable;

			if (disposableTelemetry != null) {
				disposableTelemetry.Dispose();
			}

			telemetry = SentryTelemetryService.Create(enableTelemetry);

			if (!connectionConfirmed) {
				ResetClient();
			}
		}


		private static void ResetClient() {
			if (optiTrackClient != null && handlersAttached) {
				optiTrackClient.FrameReceived     -= OnFrameReceived;
				optiTrackClient.ConnectionChanged -= OnConnectionChanged;
			}

			optiTrackClient  = CreateClient(telemetry);
			handlersAttached = false;
		}


		private static void ConnectClient(string                  localIP,
										  string                  serverIP,
										  OptiTrackConnectionType connectionType,
										  int                     commandPort,
										  int                     dataPort,
										  double                  scaleFactor,
										  int                     redrawThrottle,
										  bool                    debugLogging) {
			EnsureClientHandlers();
			Log.Clear();
			Log.Add("NatNet managed client application starting...");
			Log.Add("Local IP set.");
			Log.Add("Server IP set.");
			Log.Add("Attempting connection to Motive NatNet server...");
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
					FrameDivisor       = redrawThrottle,
					ScaleFactor        = scaleFactor,
					YUp                = yUp,
					DebugLogging       = debugLogging
			};

			try {
				optiTrackClient.ConnectAsync(options, CancellationToken.None).GetAwaiter().GetResult();
				connectionConfirmed = optiTrackClient.IsConnected;

				if (connectionConfirmed) {
					Log.Add("Fetching frame data.");
					Log.Add("Success: Data port connected.");
				}
			}
			catch (FileNotFoundException exception) {
				connectionConfirmed = false;
				Log.Add("Error: NatNet dependency is missing. Check NatNetML.dll and NatNetLib.dll next to Tracker.gha.");
				telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "component_connect_missing_dependency"));
			}
			catch (BadImageFormatException exception) {
				connectionConfirmed = false;
				Log.Add("Error: NatNet dependency architecture mismatch. Use x64 NatNet DLLs with Rhino 8.");
				telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "component_connect_bad_image"));
			}
			catch (Exception exception) {
				connectionConfirmed = false;
				Log.Add("Error: Failed to connect. Check Motive broadcast settings, IP addresses, ports, firewall, and connection type.");
				telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "component_connect"));
			}
		}


		private static void DisconnectClient() {
			if (optiTrackClient == null || !optiTrackClient.IsConnected) {
				connectionConfirmed = false;

				return;
			}

			optiTrackClient.DisconnectAsync().GetAwaiter().GetResult();
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
			currentFrame = e.Frame;
			lastFrameUtc = DateTime.UtcNow;
		}


		private static void OnConnectionChanged(object sender, OptiTrackConnectionEventArgs e) {
			if (!string.IsNullOrWhiteSpace(e.Message)) {
				Log.Add(e.Message);
			}
		}


		private static void ClearFrameState() {
			currentFrame = null;
			Log.Clear();
			mPoints.Clear();
			mLabels.Clear();
			rBodyNames.Clear();
			rBodyPlanes.Clear();
			rBodyTransforms.Clear();
			warnings.Clear();
		}


		private void ProcessFrameData(OptiTrackFrame frame, double scaleFactor) {
			if (frame == null) {
				return;
			}

			mPoints.Clear();
			mLabels.Clear();
			rBodyNames.Clear();
			rBodyPlanes.Clear();
			rBodyTransforms.Clear();

			Log.Clear();

			foreach (string message in frame.StatusMessages) {
				Log.Add(message);
			}

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


		private static Plane CreateRigidBodyPlane(OptiTrackRigidBody rigidBody, double scaleFactor) {
			Quaternion rbQuat = new Quaternion(rigidBody.Qw, rigidBody.Qx, rigidBody.Qy, rigidBody.Qz);
			rbQuat.GetRotation(out Plane rbPlane);
			rbPlane.Origin = new Point3d(rigidBody.X, rigidBody.Y, rigidBody.Z);

			var doc = Rhino.RhinoDoc.ActiveDoc;

			if (doc != null && doc.ModelUnitSystem == Rhino.UnitSystem.Millimeters) {
				rbPlane.Transform(Transform.Scale(new Point3d(0, 0, 0), 1000));
			}

			if (Math.Abs(scaleFactor - 1.0) > 0.000001) {
				rbPlane.Transform(Transform.Scale(new Point3d(0, 0, 0), scaleFactor));
			}

			Transform xformYup = new Transform();

			xformYup.M01 = xformYup.M02 = xformYup.M03 = xformYup.M10
					= xformYup.M11 = xformYup.M13 = xformYup.M20 = xformYup.M22 = xformYup.M33 = xformYup.M30 = xformYup.M31 = xformYup.M32 = 0;

			xformYup.M00 = xformYup.M21 = xformYup.M33 = 1;
			xformYup.M12 = -1;

			Transform xRotate = new Transform();

			xRotate.M00 = xRotate.M02 = xRotate.M03
					= xRotate.M11 = xRotate.M12 = xRotate.M13 = xRotate.M20 = xRotate.M21 = xRotate.M33 = xRotate.M30 = xRotate.M31 = xRotate.M32 = 0;

			xRotate.M10 = xRotate.M22 = xRotate.M33 = 1;
			xRotate.M01 = -1;

			if (yUp) {
				rbPlane.Transform(xformYup);
			}

			rbPlane.Transform(xRotate);

			return rbPlane;
		}


		private bool ValidateConfiguration(string localIP,
										   string serverIP,
										   int    commandPort,
										   int    dataPort,
										   double scaleFactor,
										   int    redrawThrottle,
										   string connectionTypeText) {
			bool valid = true;

			if (!IPAddress.TryParse(localIP, out _)) {
				warnings.Add("Local IP address is invalid.");
				valid = false;
			}

			if (!IPAddress.TryParse(serverIP, out _)) {
				warnings.Add("Server IP address is invalid.");
				valid = false;
			}

			if (!IsValidPort(commandPort)) {
				warnings.Add("Command port must be between 1 and 65535.");
				valid = false;
			}

			if (!IsValidPort(dataPort)) {
				warnings.Add("Data port must be between 1 and 65535.");
				valid = false;
			}

			if (commandPort == dataPort) {
				warnings.Add("Command port and data port should be different.");
				valid = false;
			}

			if (scaleFactor <= 0) {
				warnings.Add("Scale factor must be greater than zero.");
				valid = false;
			}

			if (redrawThrottle < 1) {
				warnings.Add("Redraw throttle must be 1 or greater.");
				valid = false;
			}

			if (ParseConnectionType(connectionTypeText) == OptiTrackConnectionType.Multicast && !IsMulticastText(connectionTypeText)) {
				warnings.Add("Connection type was not recognized. Using Multicast.");
			}

			if (!NatNetDependenciesPresent()) {
				warnings.Add("NatNetML.dll or NatNetLib.dll is missing from the plugin output folder.");
				valid = false;
			}

			if (telemetryEnabled && telemetry.Status.StartsWith("disabled", StringComparison.OrdinalIgnoreCase)) {
				warnings.Add("Telemetry was enabled, but Sentry is not configured. Set SENTRY_DSN or tracker.telemetry.local.json to activate it.");
			}

			return valid;
		}


		private void ReportNoFrameWarning() {
			if (currentFrame != null || connectionStartedUtc == DateTime.MinValue) {
				return;
			}

			if (DateTime.UtcNow.Subtract(connectionStartedUtc).TotalSeconds > 5) {
				string warning
						= "Connected, but no NatNet frame has been received. Check Motive broadcasting, firewall, IP addresses, ports, and multicast/unicast settings.";

				if (!warnings.Contains(warning)) {
					warnings.Add(warning);
					AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
				}
			}
		}


		private static bool IsValidPort(int port) {
			return port >= 1 && port <= 65535;
		}


		private static bool NatNetDependenciesPresent() {
			string assemblyLocation = typeof(TrackerComponent).Assembly.Location;

			string baseDirectory = string.IsNullOrWhiteSpace(assemblyLocation)
					? AppDomain.CurrentDomain.BaseDirectory
					: Path.GetDirectoryName(assemblyLocation);

			return File.Exists(Path.Combine(baseDirectory, "NatNetML.dll")) && File.Exists(Path.Combine(baseDirectory, "NatNetLib.dll"));
		}


		private static OptiTrackConnectionType ParseConnectionType(string value) {
			if (string.Equals(value, "Unicast", StringComparison.OrdinalIgnoreCase)) {
				return OptiTrackConnectionType.Unicast;
			}

			return OptiTrackConnectionType.Multicast;
		}


		private static bool IsMulticastText(string value) {
			return string.Equals(value, "Multicast", StringComparison.OrdinalIgnoreCase);
		}


		protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu) {
			ToolStripMenuItem cSystem = Menu_AppendItem(menu, "Y up", Menu_yUp, true, yUp);
			cSystem.ToolTipText = "When checked, component will expect Y up";

			ToolStripMenuItem rBody = Menu_AppendItem(menu, "Rigid Body", Menu_rBodyClick, true, RigidBody);
			rBody.ToolTipText = "When checked, component will stream Rigid Body data.";

			ToolStripMenuItem skeleton = Menu_AppendItem(menu, "Skeleton", Menu_skeletonClick, true, Skeleton);
			skeleton.ToolTipText = "When checked, component will stream Skeleton data.";

			ToolStripMenuItem fPlate = Menu_AppendItem(menu, "Force Plate", Menu_fPlateClick, true, ForcePlate);
			fPlate.ToolTipText = "When checked, component will stream Force Plate data.";
		}


		private void Menu_yUp(object sender, EventArgs e) {
			RecordUndoEvent("Y Up");
			yUp = !yUp;
			ExpireSolution(true);
		}


		private void Menu_rBodyClick(object sender, EventArgs e) {
			RecordUndoEvent("Rigid Body");
			RigidBody = !RigidBody;
			ExpireSolution(true);
		}


		private void Menu_skeletonClick(object sender, EventArgs e) {
			RecordUndoEvent("Skeleton");
			Skeleton = !Skeleton;
			ExpireSolution(true);
		}


		private void Menu_fPlateClick(object sender, EventArgs e) {
			RecordUndoEvent("Force Plate");
			ForcePlate = !ForcePlate;
			ExpireSolution(true);
		}


		protected override System.Drawing.Bitmap Icon {
			get {
				return Properties.Icons.Tracker;
			}
		}

		public override Guid ComponentGuid {
			get { return new Guid("D1C724B2-FEB4-405E-9570-382F8FD553F8"); }
		}

	}

}
