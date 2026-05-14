using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using OptiTrack.Geometry;
using OptiTrack.Telemetry;

using Rhino.Geometry;

namespace Tracker.Components {

	public sealed class MarkersToPointsComponent : GH_Component {

		public MarkersToPointsComponent()
			: base( "Markers To Points", "Markers->Pts", "Convert marker positions to Rhino points with explicit unit scaling.", "Tracker", "Geometry" ) {
		}

		protected override void RegisterInputParams( GH_InputParamManager pManager ) {
			pManager.AddPointParameter( "Markers", "M", "Marker points in NatNet units (meters).", GH_ParamAccess.list );
			pManager.AddNumberParameter( "Scale Factor", "S", "Scale factor applied to marker points.", GH_ParamAccess.item, 1.0 );
			pManager.AddTextParameter( "Axis Remap", "Axis", "Axis remap mode: None or ZUpToYUp.", GH_ParamAccess.item, "None" );
			pManager.AddTransformParameter( "World Transform", "W", "Optional world transform.", GH_ParamAccess.item );
			pManager.AddBooleanParameter( "Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false );
		}

		protected override void RegisterOutputParams( GH_OutputParamManager pManager ) {
			pManager.AddPointParameter( "Points", "P", "Converted marker points.", GH_ParamAccess.list );
			pManager.AddTextParameter( "Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item );
		}

		protected override void SolveInstance( IGH_DataAccess DA ) {
			List<Point3d> markers = new List<Point3d>();
			double scaleFactor = 1.0;
			string axisRemapText = "None";
			Transform worldTransform = Transform.Identity;
			bool enableTelemetry = false;

			if ( !DA.GetDataList( 0, markers ) ) {
				return;
			}

			DA.GetData( 1, ref scaleFactor );
			DA.GetData( 2, ref axisRemapText );
			DA.GetData( 3, ref worldTransform );
			DA.GetData( 4, ref enableTelemetry );

			ITelemetryService telemetry = TelemetryServiceProvider.GetService( enableTelemetry );
			TelemetryContext context = new TelemetryContext().SetTag( "component", "markers_to_points" );
			context.SetMetric( "marker_count", markers.Count );

			using ( telemetry.StartSpan( "markers_to_points", context ) ) {
				try {
					AxisRemapMode axisRemapMode = ParseAxisRemapMode( axisRemapText );
					List<Point3d> outputPoints = OptiTrackGeometryConverter.MarkersToPoints( markers, scaleFactor, axisRemapMode, worldTransform );
					DA.SetDataList( 0, outputPoints );
					DA.SetData( 1, telemetry.Status );
				} catch ( Exception exception ) {
					telemetry.CaptureException( exception, new TelemetryContext().SetTag( "operation", "markers_to_points" ) );
					AddRuntimeMessage( GH_RuntimeMessageLevel.Error, "Failed to convert marker points." );
				}
			}
		}

		private static AxisRemapMode ParseAxisRemapMode( string value ) {
			AxisRemapMode mode;
			return Enum.TryParse( value, true, out mode ) ? mode : AxisRemapMode.None;
		}

		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid( "64A7E86F-29B7-4444-8D04-4A2C424EA5F7" ); }
		}
	}
}
