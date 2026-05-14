using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Grasshopper.Kernel;

using OptiTrack.Telemetry;

using Rhino.Geometry;

namespace Tracker.Components {

	public sealed class FilterRigidBodiesComponent : GH_Component {

		public FilterRigidBodiesComponent()
			: base( "Filter Rigid Bodies", "FilterRB", "Filter rigid body lists by include/exclude regular expression patterns.", "Tracker", "Geometry" ) {
		}

		protected override void RegisterInputParams( GH_InputParamManager pManager ) {
			pManager.AddTextParameter( "Names", "N", "Rigid body names.", GH_ParamAccess.list );
			pManager.AddPlaneParameter( "Planes", "P", "Rigid body planes matching Names order.", GH_ParamAccess.list );
			pManager.AddTextParameter( "Include Pattern", "In", "Regex include pattern. Empty includes all.", GH_ParamAccess.item, string.Empty );
			pManager.AddTextParameter( "Exclude Pattern", "Ex", "Regex exclude pattern. Empty excludes none.", GH_ParamAccess.item, string.Empty );
			pManager.AddBooleanParameter( "Case Sensitive", "Case", "Regex case sensitivity.", GH_ParamAccess.item, false );
			pManager.AddBooleanParameter( "Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false );
		}

		protected override void RegisterOutputParams( GH_OutputParamManager pManager ) {
			pManager.AddTextParameter( "Filtered Names", "FN", "Filtered rigid body names.", GH_ParamAccess.list );
			pManager.AddPlaneParameter( "Filtered Planes", "FP", "Filtered rigid body planes.", GH_ParamAccess.list );
			pManager.AddTextParameter( "Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item );
		}

		protected override void SolveInstance( IGH_DataAccess DA ) {
			List<string> names = new List<string>();
			List<Plane> planes = new List<Plane>();
			string includePattern = string.Empty;
			string excludePattern = string.Empty;
			bool caseSensitive = false;
			bool enableTelemetry = false;

			if ( !DA.GetDataList( 0, names ) || !DA.GetDataList( 1, planes ) ) {
				return;
			}

			DA.GetData( 2, ref includePattern );
			DA.GetData( 3, ref excludePattern );
			DA.GetData( 4, ref caseSensitive );
			DA.GetData( 5, ref enableTelemetry );

			if ( names.Count != planes.Count ) {
				AddRuntimeMessage( GH_RuntimeMessageLevel.Error, "Names and Planes input counts must match." );
				return;
			}

			ITelemetryService telemetry = TelemetryServiceProvider.GetService( enableTelemetry );
			TelemetryContext context = new TelemetryContext().SetTag( "component", "filter_rigid_bodies" );
			context.SetMetric( "rigid_body_count", names.Count );

			using ( telemetry.StartSpan( "filter_rigid_bodies", context ) ) {
				try {
					RegexOptions options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
					Regex include = string.IsNullOrWhiteSpace( includePattern ) ? null : new Regex( includePattern, options );
					Regex exclude = string.IsNullOrWhiteSpace( excludePattern ) ? null : new Regex( excludePattern, options );

					List<string> filteredNames = new List<string>();
					List<Plane> filteredPlanes = new List<Plane>();
					for ( int i = 0; i < names.Count; i++ ) {
						bool includeMatch = include == null || include.IsMatch( names[ i ] );
						bool excludeMatch = exclude != null && exclude.IsMatch( names[ i ] );
						if ( includeMatch && !excludeMatch ) {
							filteredNames.Add( names[ i ] );
							filteredPlanes.Add( planes[ i ] );
						}
					}

					context.SetMetric( "filtered_count", filteredNames.Count );
					DA.SetDataList( 0, filteredNames );
					DA.SetDataList( 1, filteredPlanes );
					DA.SetData( 2, telemetry.Status );
				} catch ( Exception exception ) {
					telemetry.CaptureException( exception, new TelemetryContext().SetTag( "operation", "filter_rigid_bodies" ) );
					AddRuntimeMessage( GH_RuntimeMessageLevel.Error, "Failed to filter rigid bodies." );
				}
			}
		}

		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid( "CE38C4E0-8EE5-48DA-8C87-57D86479E2D1" ); }
		}
	}
}
