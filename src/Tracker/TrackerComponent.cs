using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

using Grasshopper.Kernel;

using NatNetML;

using Rhino.Geometry;

namespace Tracker {

	/// <summary>
	/// The main tracker component.
	/// </summary>
	public class TrackerComponent : GH_Component {

		public TrackerComponent()
			: base( "OptiTrack Stream",
						"OptiTrack Stream",
						"Receive streaming data from a running Motive broadcast.",
						"Tracker",
						"OptiTrack" ) {
		}

		public static List<string> Log = new List<string>();

		// [NatNet] Network connection configuration
		private static NatNetClientML mNatNet = new NatNetClientML();

		private static string mStrLocalIP = "127.0.0.1";   // Local IP address (string)
		private static string mStrServerIP = "127.0.0.1";  // Server IP address (string)
		private static readonly ConnectionType mConnectionType = ConnectionType.Multicast; // Multicast or Unicast mode. Multicast recommended.

		// List for saving each of the data descriptors
		private static List<DataDescriptor> mDataDescriptor = new List<DataDescriptor>();

		// Lists and Hashtables for saving data descriptions
		public static Hashtable mHtSkelRBs = new Hashtable();

		public static List<MarkerSet> mMarkerSet = new List<MarkerSet>();
		public static List<RigidBody> mRigidBodies = new List<RigidBody>();
		public static List<Skeleton> mSkeletons = new List<Skeleton>();
		public static List<ForcePlate> mForcePlates = new List<ForcePlate>();

		// Boolean value for detecting change in assset
		private static bool mAssetChanged = false;

		private static bool connectionConfirmed = false;
		private static int counter;

		// Flags are appended to a list for checking at SolveInstance.
		private static bool RigidBody = false;

		private static bool Skeleton = false;
		private static bool ForcePlate = false;

		// Marker points
		protected static List<Point3d> mPoints = new List<Point3d>();

		protected static List<string> mLabels = new List<string>();

		// Rigid body data.
		private static readonly List<string> rBodyNames = new List<string>();

		private static readonly List<double> rBodyPos = new List<double>();
		private static readonly List<double> rBodyQuats = new List<double>();
		private static readonly List<Plane> rBodyPlanes = new List<Plane>();

		//Setup Configurations
		private static bool yUp = false;

		protected override void RegisterInputParams( GH_InputParamManager pManager ) {
			pManager.AddBooleanParameter( "Activate", "Activate", "Activate the streaming module.", GH_ParamAccess.item, false );
			pManager.AddBooleanParameter( "Reset", "Reset", "Reset the streaming module.", GH_ParamAccess.item, false );
			pManager.AddTextParameter( "Local IP", "Local IP", "IP address for the receiver.", GH_ParamAccess.item, "127.0.0.1" );
			pManager.AddTextParameter( "Server IP", "Server IP", "IP address for the server.", GH_ParamAccess.item, "127.0.0.1" );
			// TODO Frame rate variable input either here or in the menu.
		}

		protected override void RegisterOutputParams( GH_OutputParamManager pManager ) {
			pManager.AddTextParameter( "Status", "Status", "Status of the streaming service.", GH_ParamAccess.list );
			pManager.AddPointParameter( "Markers", "Markers", "Generic, unlabelled marker data.", GH_ParamAccess.list );
			pManager.AddTextParameter( "Labels", "Labels", "Marker labels.", GH_ParamAccess.list );
			pManager.AddTextParameter( "RB Name", "RB Name", "List of Rigid Body name data.", GH_ParamAccess.list );
			pManager.AddPlaneParameter( "BR Plane", "BR Plane", "List of planes assosieated with the Ridgid Bodys", GH_ParamAccess.list );
			// TODO pManager.AddTextParameter("RB Position", "RB Pos", "List of Rigid Body
			// position data.", GH_ParamAccess.list); TODO pManager.AddTextParameter("RB
			// Quaternion", "RB Quat", "List of Rigid Body quaternion data.",
			// GH_ParamAccess.list);
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

			// Reset the service if the flag is active.
			if ( reset ) {
				mDataDescriptor.Clear();
				mHtSkelRBs.Clear();
				mRigidBodies.Clear();
				mSkeletons.Clear();
				mForcePlates.Clear();
				Log.Clear();

				counter = 0;
			}

			// Activate the streaming module and attempt to find a Motive broadcast.
			if ( activate && counter == 0 ) {
				Log.Add( "NatNet managed client application starting..." );

				// If not default, set the IP address
				mStrLocalIP = localIP;
				Log.Add( "Local IP set." );

				mStrServerIP = serverIP;
				Log.Add( "Server IP set." );

				// Attempt connection to the server.
				Log.Add( "Attempting connection to server..." );
				ConnectToServer();
				Log.Add( "Fetching the Server Descriptor." );
				connectionConfirmed = FetchServerDescriptor(); //Fetch and parse data descriptor

				Log.Add( "Fetching the Frame Data." );
				/*  [NatNet] Assigning a event handler function for fetching frame data each time a frame is received   */
				mNatNet.OnFrameReady += new FrameReadyEventHandler( FetchFrameData );
				Log.Add( "Success: Data Port Connected." );

				counter++;
			} else if ( activate && connectionConfirmed ) // If the service is activated and we have a confirmed connection to the server.
				{
				/*  [NatNet] Assigning a event handler function for fetching frame data each time a frame is received   */
				mNatNet.OnFrameReady += new FrameReadyEventHandler( FetchFrameData );

				// Exception handler for updated assets list.
				if ( mAssetChanged == true ) {
					Log.Add( "Change in the list of the assets. Refetching the descriptions" );

					/*  Clear out existing lists */
					mDataDescriptor.Clear();
					mHtSkelRBs.Clear();
					mRigidBodies.Clear();
					mSkeletons.Clear();
					mForcePlates.Clear();

					/* [NatNet] Re-fetch the updated list of descriptors  */
					FetchDataDescriptor();
					mAssetChanged = false;
				}

				/*  [NatNet] Disabling data handling function   */
				mNatNet.OnFrameReady -= FetchFrameData;

				counter++;
			} else if ( !activate ) {
				/*  Clearing Saved Descriptions */
				mRigidBodies.Clear();
				mSkeletons.Clear();
				mHtSkelRBs.Clear();
				mForcePlates.Clear();

				if ( connectionConfirmed ) {
					mNatNet.Disconnect();
				}
				Log.Clear();
				Log.Add( "Service stopped. Activate module to begin streaming." );
			}

			try {
				DA.SetDataList( "Status", Log );
				DA.SetDataList( "Markers", mPoints );
				DA.SetDataList( "Labels", mLabels );
				DA.SetDataList( "RB Name", rBodyNames );
				//DA.SetDataList("RB Position", rigidBodyPos);
				//DA.SetDataList("RB Quaternion", rigidBodyQuat);
				DA.SetDataList( "BR Plane", rBodyPlanes );
			} catch ( Exception ) {
				//throw;
			}

			ExpireSolution( true );
		}

		/// <summary>
		/// [NatNet] parseFrameData will be called when a frame of Mocap data has is received
		/// from the server application.
		///
		/// Note: This callback is on the network service thread, so it is important to return
		/// from this function quickly as possible to prevent incoming frames of data from
		/// buffering up on the network socket.
		///
		/// Note: "data" is a reference structure to the current frame of data. NatNet re-uses
		/// this same instance for each incoming frame, so it should not be kept (the values
		/// contained in "data" will become replaced after this callback function has exited).
		/// </summary>
		/// <param name="data">   The actual frame of mocap data </param>
		/// <param name="client"> The NatNet client instance </param>
		private static void FetchFrameData( FrameOfMocapData data, NatNetClientML client ) {
			/*  Exception handler for cases where assets are added or removed.
          Data description is re-obtained in the main function so that contents
          in the frame handler is kept minimal. */
			if ( data.bTrackingModelsChanged == true || data.nRigidBodies != mRigidBodies.Count || data.nSkeletons != mSkeletons.Count || data.nForcePlates != mForcePlates.Count ) {
				mAssetChanged = true;
			}

			/*  Processing and ouputting frame data every 200th frame.
          This conditional statement is included in order to simplify the program output */
			if ( data.iFrame % 4 == 0 ) // 120 FPS Flex 13 cameras (every 4th frame will give a solid 30 FPS in GH)
			{
				if ( data.bRecording == false ) {
					//Log.Add("Frame # " + data.iFrame.ToString() + " Received:");
				} else if ( data.bRecording == true ) {
					//Log.Add("[Recording] Frame #" + data.iFrame.ToString() + " Received:");
				}

				ProcessFrameData( data );
			}
		}

		/// <summary>
		/// [NatNet] Process mocap frame data.
		/// </summary>
		/// <param name="data"> </param>
		/// <exception cref="Exception"> </exception>
		private static void ProcessFrameData( FrameOfMocapData data ) {
			mPoints.Clear();

			Point3d markerPoint;
			bool createPoints = false; // Set tp true to create points for each marker.
			bool allPoints = false; // Set to true to output all marker points.

			// [NatNet] Fetch Labeled Markers (Point Cloud)
			for ( int i = 0; i < data.nOtherMarkers; i++ ) {
				if ( createPoints ) {
					// Recreate point data.
					markerPoint = new Point3d(
						Math.Round( data.LabeledMarkers[ i ].x, 4 ),
						Math.Round( data.LabeledMarkers[ i ].y, 4 ),
						Math.Round( data.LabeledMarkers[ i ].z, 4 ) );

					mPoints.Add( markerPoint );

					if ( allPoints ) {
						for ( int j = 0; j < data.MarkerSets[ i ].nMarkers; j++ ) {
							// Recreate point data.
							markerPoint = new Point3d(
								Math.Round( data.LabeledMarkers[ i ].x, 4 ),
								Math.Round( data.LabeledMarkers[ i ].y, 4 ),
								Math.Round( data.LabeledMarkers[ i ].z, 4 ) );

							mPoints.Add( markerPoint );
						}
					}
				}

				// Create marker labels.
				string markerLabel = data.OtherMarkers[ i ].ID.ToString();
				mLabels.Add( markerLabel );
			}

			if ( RigidBody ) {
				rBodyNames.Clear();
				rBodyPos.Clear();
				rBodyQuats.Clear();
				rBodyPlanes.Clear();

				// [NatNet] Fetch Rigid Body Frame Data
				for ( int i = 0; i < mRigidBodies.Count; i++ ) {
					int rbID = mRigidBodies[ i ].ID; // Fetch the rigid body IDs.

					// For each rigid body saved in the description list, fetch the actual object.
					for ( int j = 0; j < data.nRigidBodies; j++ ) {
						RigidBody rb = mRigidBodies[ i ];                // Saved rigid body descriptions
						RigidBodyData rbData = data.RigidBodies[ j ];    // Received rigid body descriptions

						if ( rbData.Tracked == true ) {
							// Rigid Body ID
							rBodyNames.Add( rb.Name );

							// Rigid Body Position
							rBodyPos.Add( Math.Round( rbData.x, 4 ) );
							rBodyPos.Add( Math.Round( rbData.y, 4 ) );
							rBodyPos.Add( Math.Round( rbData.z, 4 ) );

							// Rigid Body Quaternion
							float[] quat = new float[ 4 ] { rbData.qx, rbData.qy, rbData.qz, rbData.qw };

							// Attempt to normalise the rotation notation to fit with robot programming
							// (WXYZ).
							rBodyQuats.Add( Math.Round( rbData.qw, 6 ) );
							rBodyQuats.Add( Math.Round( rbData.qx, 6 ) );
							rBodyQuats.Add( Math.Round( rbData.qy, 6 ) );
							rBodyQuats.Add( Math.Round( rbData.qz, 6 ) );

							// Attempt to convert Quaternion to Euler
							try {
								float[] eulers = new float[ 3 ];

								//Converting quat orientation into XYZ Euler representation.
								eulers = NatNetClientML.QuatToEuler( quat, NATEulerOrder.NAT_XYZr );
								double xrot = Utlities.RadiansToDegrees( eulers[ 0 ] );
								double yrot = Utlities.RadiansToDegrees( eulers[ 1 ] );
								double zrot = Utlities.RadiansToDegrees( eulers[ 2 ] );

								Console.WriteLine( "\t\tori ({0:N3}, {1:N3}, {2:N3})", xrot, yrot, zrot );
							} catch ( Exception ) {
								var msg = "Failed to convert quaternion to Euler.";
								Console.WriteLine( msg );
								throw new Exception( msg );
							}

							// Create Plane
							Quaternion rbQuat = new Quaternion( Math.Round( rbData.qw, 6 ), Math.Round( rbData.qx, 6 ), Math.Round( rbData.qy, 6 ), Math.Round( rbData.qz, 6 ) );
							rbQuat.GetRotation( out Plane rbPlane );
							rbPlane.Origin = new Point3d( Math.Round( rbData.x, 4 ), Math.Round( rbData.y, 4 ), Math.Round( rbData.z, 4 ) );

							// If the document is in mm, scale the plane to mm.
							var doc = Rhino.RhinoDoc.ActiveDoc;
							if ( doc != null && doc.ModelUnitSystem == Rhino.UnitSystem.Millimeters ) {
								rbPlane.Transform( Transform.Scale( new Point3d( 0, 0, 0 ), 1000 ) );
							}

							// Y Rotation Matrix (Up)
							Transform xformYup = new Transform();
							xformYup.M01 = xformYup.M02 = xformYup.M03 = xformYup.M10 = xformYup.M11 = xformYup.M13 = xformYup.M20 = xformYup.M22 = xformYup.M33 = xformYup.M30 = xformYup.M31 = xformYup.M32 = 0;
							xformYup.M00 = xformYup.M21 = xformYup.M33 = 1;
							xformYup.M12 = -1;

							// X Rotation Matrix
							Transform xRotate = new Transform();
							xRotate.M00 = xRotate.M02 = xRotate.M03 = xRotate.M11 = xRotate.M12 = xRotate.M13 = xRotate.M20 = xRotate.M21 = xRotate.M33 = xRotate.M30 = xRotate.M31 = xRotate.M32 = 0;
							xRotate.M10 = xRotate.M22 = xRotate.M33 = 1;
							xRotate.M01 = -1;

							if ( yUp ) //Detect if Y is pointing up
							{
								rbPlane.Transform( xformYup );
							}

							rbPlane.Transform( xRotate );
							rBodyPlanes.Add( rbPlane );
						} else {
							Log.Add( rb.Name.ToString() + " is not tracked in current frame." );
						}
					}
				}
			}

			//if (Skeleton)
			//{
			//    /* Parsing Skeleton Frame Data  */
			//    for (int i = 0; i < mSkeletons.Count; i++)      // Fetching skeleton IDs from the saved descriptions
			//    {
			//        int sklID = mSkeletons[i].ID;

			// for (int j = 0; j < data.nSkeletons; j++) { if (sklID == data.Skeletons[j].ID) //
			// When skeleton ID of the description matches skeleton ID of the frame data. {
			// NatNetML.Skeleton skl = mSkeletons[i]; // Saved skeleton descriptions
			// NatNetML.SkeletonData sklData = data.Skeletons[j]; // Received skeleton frame
			// data

			// Console.WriteLine("\tSkeleton ({0}):", skl.Name); Console.WriteLine("\t\tSegment
			// count: {0}", sklData.nRigidBodies);

			// /* Now, for each of the skeleton segments */ for (int k = 0; k <
			// sklData.nRigidBodies; k++) { NatNetML.RigidBodyData boneData =
			// sklData.RigidBodies[k];

			// /* Decoding skeleton bone ID */ int skeletonID = HighWord(boneData.ID); int
			// rigidBodyID = LowWord(boneData.ID); int uniqueID = skeletonID * 1000 +
			// rigidBodyID; int key = uniqueID.GetHashCode();

			// NatNetML.RigidBody bone = (RigidBody)mHtSkelRBs[key]; //Fetching saved skeleton
			// bone descriptions

			//                    //Outputting only the hip segment data for the purpose of this sample.
			//                    if (k == 0)
			//                        Console.WriteLine("\t\t{0:N3}: pos({1:N3}, {2:N3}, {3:N3})", bone.Name, boneData.x, boneData.y, boneData.z);
			//                }
			//            }
			//        }
			//    }
			//}

			//if (ForcePlate)
			//{
			//    /*  Parsing Force Plate Frame Data  */
			//    for (int i = 0; i < mForcePlates.Count; i++)
			//    {
			//        int fpID = mForcePlates[i].ID;                  // Fetching force plate IDs from the saved descriptions

			// for (int j = 0; j < data.nForcePlates; j++) { if (fpID == data.ForcePlates[j].ID)
			// // When force plate ID of the descriptions matches force plate ID of the frame
			// data. { NatNetML.ForcePlate fp = mForcePlates[i]; // Saved force plate
			// descriptions NatNetML.ForcePlateData fpData = data.ForcePlates[i]; // Received
			// forceplate frame data

			// Console.WriteLine("\tForce Plate ({0}):", fp.Serial);

			//                // Here we will be printing out only the first force plate "subsample" (index 0) that was collected with the mocap frame.
			//                for (int k = 0; k < fpData.nChannels; k++)
			//                {
			//                    Console.WriteLine("\t\tChannel {0}: {1}", fp.ChannelNames[k], fpData.ChannelData[k].Values[0]);
			//                }
			//            }
			//        }
			//    }
			//}
		}

		/// <summary>
		/// [NatNet] Establish a NatNet Client connection (if not established already).
		/// </summary>
		private static void ConnectToServer() {
			// [NatNet] Instantiate the client object
			mNatNet = new NatNetClientML();

			// [NatNet] Checking verions of the NatNet SDK library
			int[] verNatNet = new int[ 4 ];           // Saving NatNet SDK version number
			verNatNet = mNatNet.NatNetVersion();
			Console.WriteLine( "NatNet SDK Version: {0}.{1}.{2}.{3}", verNatNet[ 0 ],
			verNatNet[ 1 ], verNatNet[ 2 ], verNatNet[ 3 ] );

			// [NatNet] Connecting to the Server
			Console.WriteLine( "\nConnecting...\n\tLocal IP address: {0}\n\tServer IP Address: {1}\n\n", mStrLocalIP, mStrServerIP );

			NatNetClientML.ConnectParams connectParams = new NatNetClientML.ConnectParams {
				ConnectionType = mConnectionType,
				ServerAddress = mStrServerIP,
				LocalAddress = mStrLocalIP
			};

			mNatNet.Connect( connectParams );
		}

		/// <summary>
		/// [NatNet] Fetch server description to confirm connection.
		/// </summary>
		/// <returns> </returns>
		private static bool FetchServerDescriptor() {
			ServerDescription m_ServerDescriptor = new ServerDescription();
			int errorCode = mNatNet.GetServerDescription( m_ServerDescriptor );

			if ( errorCode == 0 ) {
				Log.Add( "Success: Connected to the server." );
				ParseSeverDescriptor( m_ServerDescriptor );
				return true;
			} else {
				Log.Add( "Error: Failed to connect. Check the connection settings." );
				Log.Add( "Program terminated." );
				return false;
			}
		}

		/// <summary>
		/// [NatNet] Parse server information and display in console window.
		/// </summary>
		/// <param name="server"> </param>
		private static void ParseSeverDescriptor( ServerDescription server ) {
			Log.Add( "Server Info:" );
			Log.Add( "Host: " + server.HostComputerName );
			Log.Add( "Application Name: " + server.HostApp );
			// Console.WriteLine("\tApplication Version: {0}.{1}.{2}.{3}",
			// server.HostAppVersion[0], server.HostAppVersion[1], server.HostAppVersion[2],
			// server.HostAppVersion[3]); Console.WriteLine("\tNatNet Version:
			// {0}.{1}.{2}.{3}\n", server.NatNetVersion[0], server.NatNetVersion[1],
			// server.NatNetVersion[2], server.NatNetVersion[3]);
		}

		/// <summary>
		/// [NatNet] Fetch Data Descriptions from the server and populate the appropriate
		/// lists.
		/// </summary>
		private static void FetchDataDescriptor() {
			// [NatNet] Fetch Data Descriptions. Instantiate objects for saving data
			// descriptions and frame data.
			bool result = mNatNet.GetDataDescriptions( out mDataDescriptor );
			if ( result ) {
				Log.Add( "Success: Data Descriptions obtained from the server." );
				ParseDataDescriptor( mDataDescriptor );
			} else {
				Log.Add( "Error: Could not get the Data Descriptions" );
			}
		}

		/// <summary>
		/// [NatNet] Parse Data Descriptions and display in console window.
		/// </summary>
		/// <param name="description"> </param>
		private static void ParseDataDescriptor( List<DataDescriptor> description ) {
			// [NatNet] Request a description of the Active Model List from the server. This
			// sample will list only names of the data sets, but you can access
			int numDataSet = description.Count;
			Log.Add( "Total " + numDataSet.ToString() + " data sets in the capture:" );

			for ( int i = 0; i < numDataSet; ++i ) {
				int dataSetType = description[ i ].type;
				// Parse Data Descriptions for each data sets and save them in the delcared lists
				// and hashtables for later uses.
				switch ( dataSetType ) {
					case (int) DataDescriptorType.eMarkerSetData:
					MarkerSet mkset = (MarkerSet) description[ i ];
					Log.Add( "MarkerSet (" + mkset.Name + ")" );
					break;

					case (int) DataDescriptorType.eRigidbodyData:
					RigidBody rb = (RigidBody) description[ i ];
					Log.Add( "RigidBody (" + rb.Name + ")" );

					// Saving Rigid Body Descriptions
					mRigidBodies.Add( rb );
					break;

					//case ((int)NatNetML.DataDescriptorType.eSkeletonData):
					//    NatNetML.Skeleton skl = (NatNetML.Skeleton)description[i];
					//    Log.Add("Skeleton (" + skl.Name + ", Bones:");

					// //Saving Skeleton Descriptions mSkeletons.Add(skl);

					// // Saving Individual Bone Descriptions for (int j = 0; j < skl.nRigidBodies;
					// j++) { Console.WriteLine("\t\t{0}. {1}", j + 1, skl.RigidBodies[j].Name); int
					// uniqueID = skl.ID * 1000 + skl.RigidBodies[j].ID; int key =
					// uniqueID.GetHashCode(); mHtSkelRBs.Add(key, skl.RigidBodies[j]); //Saving the
					// bone segments onto the hashtable } break;

					//case ((int)NatNetML.DataDescriptorType.eForcePlateData):
					//    NatNetML.ForcePlate fp = (NatNetML.ForcePlate)description[i];
					//    Console.WriteLine("\tForcePlate ({0})", fp.Serial);

					// // Saving Force Plate Channel Names mForcePlates.Add(fp);

					// for (int j = 0; j < fp.ChannelCount; j++) { Console.WriteLine("\t\tChannel
					// {0}: {1}", j + 1, fp.ChannelNames[j]); } break;

					default:
					// When a Data Set does not match any of the descriptions provided by the SDK.
					Log.Add( "Error: Invalid Data Set" );
					break;
				}
			}
		}

		// The following functions append menu items and then handle the item clicked event.
		protected override void AppendAdditionalComponentMenuItems( ToolStripDropDown menu ) {
			ToolStripMenuItem cSystem = Menu_AppendItem( menu, "Y up", Menu_yUp, true, yUp );
			cSystem.ToolTipText = "When checked, component will expect Y up";

			ToolStripMenuItem rBody = Menu_AppendItem( menu, "Rigid Body", Menu_rBodyClick, true, RigidBody ); // Append the item to the menu.
			rBody.ToolTipText = "When checked, component will stream Rigid Body data."; // Specifically assign a tooltip text to the menu item.

			ToolStripMenuItem skeleton = Menu_AppendItem( menu, "Skeleton", Menu_skeletonClick, true, Skeleton );
			skeleton.ToolTipText = "When checked, component will stream Skeleton data.";

			ToolStripMenuItem fPlate = Menu_AppendItem( menu, "Force Plate", Menu_fPlateClick, true, ForcePlate );
			fPlate.ToolTipText = "When checked, component will stream Force Plate data.";
		}

		/// <summary>
		/// Handle the item clicked event of the menu item.
		/// </summary>
		/// <param name="sender"> </param>
		/// <param name="e">      </param>
		private void Menu_yUp( object sender, EventArgs e ) {
			RecordUndoEvent( "Y Up" );
			yUp = !yUp;
			ExpireSolution( true );
		}

		/// <summary>
		/// Handle the item clicked event of the menu item.
		/// </summary>
		/// <param name="sender"> </param>
		/// <param name="e">      </param>
		private void Menu_rBodyClick( object sender, EventArgs e ) {
			RecordUndoEvent( "Rigid Body" );
			RigidBody = !RigidBody;
			ExpireSolution( true );
		}

		/// <summary>
		/// Handle the item clicked event of the menu item.
		/// </summary>
		/// <param name="sender"> </param>
		/// <param name="e">      </param>
		private void Menu_skeletonClick( object sender, EventArgs e ) {
			RecordUndoEvent( "Skeleton" );
			Skeleton = !Skeleton;
			ExpireSolution( true );
		}

		/// <summary>
		/// Handle the item clicked event of the menu item.
		/// </summary>
		/// <param name="sender"> </param>
		/// <param name="e">      </param>
		private void Menu_fPlateClick( object sender, EventArgs e ) {
			RecordUndoEvent( "Force Plate" );
			ForcePlate = !ForcePlate;
			ExpireSolution( true );
		}

		/// <summary>
		/// Retuns a bitmap to represent the component.
		/// </summary>
		protected override System.Drawing.Bitmap Icon {
			get {
				return Properties.Icons.Tracker;
			}
		}

		/// <summary>
		/// Gets the unique ID for this component. Do not copy this.
		/// </summary>
		public override Guid ComponentGuid {
			get { return new Guid( "D1C724B2-FEB4-405E-9570-382F8FD553F8" ); }
		}
	}
}