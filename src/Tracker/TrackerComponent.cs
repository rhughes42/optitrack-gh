using System;
using System.Collections.Generic;
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
		private static readonly ITelemetryService Telemetry = new NoOpTelemetryService();
		private static IOptiTrackClient optiTrackClient = CreateClient();
		private static OptiTrackFrame currentFrame;
		private static bool handlersAttached;
		private static bool connectionConfirmed;
		private static int counter;

		private static bool RigidBody;
		private static bool Skeleton;
		private static bool ForcePlate;
		private static bool yUp;

		public static List<string> Log = new List<string>();
		protected static List<Point3d> mPoints = new List<Point3d>();
		protected static List<string> mLabels = new List<string>();
		private static readonly List<string> rBodyNames = new List<string>();
		private static readonly List<Plane> rBodyPlanes = new List<Plane>();

		public TrackerComponent()
			: base( "OptiTrack Stream",
						"OptiTrack Stream",
						"Receive streaming data from a running Motive broadcast.",
						"Tracker",
						"OptiTrack" ) {
		}

		private static IOptiTrackClient CreateClient() {
			return new NatNetOptiTrackClient( Telemetry );
		}

		protected override void RegisterInputParams( GH_InputParamManager pManager ) {
			pManager.AddBooleanParameter( "Activate", "Activate", "Activate the streaming module.", GH_ParamAccess.item, false );
			pManager.AddBooleanParameter( "Reset", "Reset", "Reset the streaming module.", GH_ParamAccess.item, false );
			pManager.AddTextParameter( "Local IP", "Local IP", "IP address for the receiver.", GH_ParamAccess.item, "127.0.0.1" );
			pManager.AddTextParameter( "Server IP", "Server IP", "IP address for the server.", GH_ParamAccess.item, "127.0.0.1" );
		}

		protected override void RegisterOutputParams( GH_OutputParamManager pManager ) {
			pManager.AddTextParameter( "Status", "Status", "Status of the streaming service.", GH_ParamAccess.list );
			pManager.AddPointParameter( "Markers", "Markers", "Generic, unlabelled marker data.", GH_ParamAccess.list );
			pManager.AddTextParameter( "Labels", "Labels", "Marker labels.", GH_ParamAccess.list );
			pManager.AddTextParameter( "RB Name", "RB Name", "List of Rigid Body name data.", GH_ParamAccess.list );
			pManager.AddPlaneParameter( "BR Plane", "BR Plane", "List of planes assosieated with the Ridgid Bodys", GH_ParamAccess.list );
		}

		protected override void SolveInstance( IGH_DataAccess DA ) {
			bool activate = false;
			bool reset = false;
			string localIP = "127.0.0.1";
			string serverIP = "127.0.0.1";

			if ( !DA.GetData( 0, ref activate ) )
				return;
			if ( !DA.GetData( 1, ref reset ) )
				return;
			if ( !DA.GetData( 2, ref localIP ) )
				return;
			if ( !DA.GetData( 3, ref serverIP ) )
				return;

			if ( reset ) {
				ClearFrameState();
				DisconnectClient();
				counter = 0;
			}

			if ( activate && counter == 0 ) {
				ConnectClient( localIP, serverIP );
				counter++;
			} else if ( activate && connectionConfirmed ) {
				ProcessFrameData( currentFrame );
				counter++;
			} else if ( !activate ) {
				DisconnectClient();
				ClearFrameState();
				Log.Add( "Service stopped. Activate module to begin streaming." );
			}

			try {
				DA.SetDataList( "Status", Log );
				DA.SetDataList( "Markers", mPoints );
				DA.SetDataList( "Labels", mLabels );
				DA.SetDataList( "RB Name", rBodyNames );
				DA.SetDataList( "BR Plane", rBodyPlanes );
			} catch ( Exception exception ) {
				Telemetry.CaptureException( exception, new TelemetryContext().SetTag( "operation", "grasshopper_set_output" ) );
			}

			ExpireSolution( true );
		}

		private static void ConnectClient( string localIP, string serverIP ) {
			EnsureClientHandlers();
			Log.Clear();
			Log.Add( "NatNet managed client application starting..." );
			Log.Add( "Local IP set." );
			Log.Add( "Server IP set." );
			Log.Add( "Attempting connection to server..." );

			OptiTrackConnectionOptions options = new OptiTrackConnectionOptions {
				LocalAddress = localIP,
				ServerAddress = serverIP,
				ConnectionType = OptiTrackConnectionType.Multicast,
				IncludeMarkers = true,
				IncludeRigidBodies = RigidBody,
				IncludeSkeletons = Skeleton,
				IncludeForcePlates = ForcePlate,
				FrameDivisor = 4
			};

			try {
				optiTrackClient.ConnectAsync( options, CancellationToken.None ).GetAwaiter().GetResult();
				connectionConfirmed = optiTrackClient.IsConnected;
				if ( connectionConfirmed ) {
					Log.Add( "Fetching the Frame Data." );
					Log.Add( "Success: Data Port Connected." );
				}
			} catch ( Exception exception ) {
				connectionConfirmed = false;
				Log.Add( "Error: Failed to connect. Check the connection settings." );
				Telemetry.CaptureException( exception, new TelemetryContext().SetTag( "operation", "component_connect" ) );
			}
		}

		private static void DisconnectClient() {
			if ( optiTrackClient == null || !optiTrackClient.IsConnected ) {
				connectionConfirmed = false;
				return;
			}

			optiTrackClient.DisconnectAsync().GetAwaiter().GetResult();
			connectionConfirmed = false;
		}

		private static void EnsureClientHandlers() {
			if ( handlersAttached ) {
				return;
			}

			optiTrackClient.FrameReceived += OnFrameReceived;
			optiTrackClient.ConnectionChanged += OnConnectionChanged;
			handlersAttached = true;
		}

		private static void OnFrameReceived( object sender, OptiTrackFrameEventArgs e ) {
			currentFrame = e.Frame;
		}

		private static void OnConnectionChanged( object sender, OptiTrackConnectionEventArgs e ) {
			if ( !string.IsNullOrWhiteSpace( e.Message ) ) {
				Log.Add( e.Message );
			}
		}

		private static void ClearFrameState() {
			currentFrame = null;
			Log.Clear();
			mPoints.Clear();
			mLabels.Clear();
			rBodyNames.Clear();
			rBodyPlanes.Clear();
		}

		private static void ProcessFrameData( OptiTrackFrame frame ) {
			if ( frame == null ) {
				return;
			}

			mPoints.Clear();
			mLabels.Clear();
			rBodyNames.Clear();
			rBodyPlanes.Clear();

			Log.Clear();
			foreach ( string message in frame.StatusMessages ) {
				Log.Add( message );
			}

			foreach ( OptiTrackMarker marker in frame.Markers ) {
				mLabels.Add( marker.Label );
			}

			if ( RigidBody ) {
				foreach ( OptiTrackRigidBody rigidBody in frame.RigidBodies ) {
					if ( !rigidBody.IsTracked ) {
						continue;
					}

					rBodyNames.Add( rigidBody.Name );
					rBodyPlanes.Add( CreateRigidBodyPlane( rigidBody ) );
				}
			}
		}

		private static Plane CreateRigidBodyPlane( OptiTrackRigidBody rigidBody ) {
			Quaternion rbQuat = new Quaternion( rigidBody.Qw, rigidBody.Qx, rigidBody.Qy, rigidBody.Qz );
			rbQuat.GetRotation( out Plane rbPlane );
			rbPlane.Origin = new Point3d( rigidBody.X, rigidBody.Y, rigidBody.Z );

			var doc = Rhino.RhinoDoc.ActiveDoc;
			if ( doc != null && doc.ModelUnitSystem == Rhino.UnitSystem.Millimeters ) {
				rbPlane.Transform( Transform.Scale( new Point3d( 0, 0, 0 ), 1000 ) );
			}

			Transform xformYup = new Transform();
			xformYup.M01 = xformYup.M02 = xformYup.M03 = xformYup.M10 = xformYup.M11 = xformYup.M13 = xformYup.M20 = xformYup.M22 = xformYup.M33 = xformYup.M30 = xformYup.M31 = xformYup.M32 = 0;
			xformYup.M00 = xformYup.M21 = xformYup.M33 = 1;
			xformYup.M12 = -1;

			Transform xRotate = new Transform();
			xRotate.M00 = xRotate.M02 = xRotate.M03 = xRotate.M11 = xRotate.M12 = xRotate.M13 = xRotate.M20 = xRotate.M21 = xRotate.M33 = xRotate.M30 = xRotate.M31 = xRotate.M32 = 0;
			xRotate.M10 = xRotate.M22 = xRotate.M33 = 1;
			xRotate.M01 = -1;

			if ( yUp ) {
				rbPlane.Transform( xformYup );
			}

			rbPlane.Transform( xRotate );
			return rbPlane;
		}

		protected override void AppendAdditionalComponentMenuItems( ToolStripDropDown menu ) {
			ToolStripMenuItem cSystem = Menu_AppendItem( menu, "Y up", Menu_yUp, true, yUp );
			cSystem.ToolTipText = "When checked, component will expect Y up";

			ToolStripMenuItem rBody = Menu_AppendItem( menu, "Rigid Body", Menu_rBodyClick, true, RigidBody );
			rBody.ToolTipText = "When checked, component will stream Rigid Body data.";

			ToolStripMenuItem skeleton = Menu_AppendItem( menu, "Skeleton", Menu_skeletonClick, true, Skeleton );
			skeleton.ToolTipText = "When checked, component will stream Skeleton data.";

			ToolStripMenuItem fPlate = Menu_AppendItem( menu, "Force Plate", Menu_fPlateClick, true, ForcePlate );
			fPlate.ToolTipText = "When checked, component will stream Force Plate data.";
		}

		private void Menu_yUp( object sender, EventArgs e ) {
			RecordUndoEvent( "Y Up" );
			yUp = !yUp;
			ExpireSolution( true );
		}

		private void Menu_rBodyClick( object sender, EventArgs e ) {
			RecordUndoEvent( "Rigid Body" );
			RigidBody = !RigidBody;
			ExpireSolution( true );
		}

		private void Menu_skeletonClick( object sender, EventArgs e ) {
			RecordUndoEvent( "Skeleton" );
			Skeleton = !Skeleton;
			ExpireSolution( true );
		}

		private void Menu_fPlateClick( object sender, EventArgs e ) {
			RecordUndoEvent( "Force Plate" );
			ForcePlate = !ForcePlate;
			ExpireSolution( true );
		}

		protected override System.Drawing.Bitmap Icon {
			get {
				return Properties.Icons.Tracker;
			}
		}

		public override Guid ComponentGuid {
			get { return new Guid( "D1C724B2-FEB4-405E-9570-382F8FD553F8" ); }
		}
	}
}
