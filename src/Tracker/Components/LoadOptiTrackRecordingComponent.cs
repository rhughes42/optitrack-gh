using System;

using Grasshopper.Kernel;

using OptiTrack.Recording;
using OptiTrack.Telemetry;

namespace Tracker.Components {

	public sealed class LoadOptiTrackRecordingComponent : GH_Component {

		public LoadOptiTrackRecordingComponent()
			: base( "Load OptiTrack Recording", "LoadRec", "Load a recorded OptiTrack JSON file for replay without Motive.", "Tracker", "Recording" ) {
		}

		protected override void RegisterInputParams( GH_InputParamManager pManager ) {
			pManager.AddTextParameter( "Recording File", "File", "Path to a recording JSON file.", GH_ParamAccess.item );
			pManager.AddBooleanParameter( "Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false );
		}

		protected override void RegisterOutputParams( GH_OutputParamManager pManager ) {
			pManager.AddGenericParameter( "Recording", "Rec", "Loaded recording object.", GH_ParamAccess.item );
			pManager.AddTextParameter( "Metadata Summary", "Meta", "Summary of recording metadata.", GH_ParamAccess.item );
			pManager.AddIntegerParameter( "Frame Count", "Count", "Number of frames in recording.", GH_ParamAccess.item );
			pManager.AddTextParameter( "Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item );
		}

		protected override void SolveInstance( IGH_DataAccess DA ) {
			string filePath = string.Empty;
			bool enableTelemetry = false;
			if ( !DA.GetData( 0, ref filePath ) ) {
				return;
			}

			DA.GetData( 1, ref enableTelemetry );
			ITelemetryService telemetry = TelemetryServiceProvider.GetService( enableTelemetry );
			TelemetryContext context = new TelemetryContext()
				.SetTag( "component", "load_recording" )
				.SetTag( "format", "json" );

			using ( telemetry.StartSpan( "load_recording_json", context ) ) {
				try {
					OptiTrackRecording recording = OptiTrackRecordingSerializer.LoadJson( filePath );
					context.SetMetric( "frame_count", recording.Frames.Count );
					context.SetTag( "format_version", recording.FormatVersion );

					string summary = string.Format( "Version {0}, Source {1}, Units {2}, Frames {3}",
						recording.FormatVersion,
						recording.Metadata.Source,
						recording.Metadata.Units,
						recording.Metadata.FrameCount );

					DA.SetData( 0, recording );
					DA.SetData( 1, summary );
					DA.SetData( 2, recording.Metadata.FrameCount );
					DA.SetData( 3, telemetry.Status );
				} catch ( Exception exception ) {
					telemetry.CaptureException( exception, new TelemetryContext()
						.SetTag( "operation", "load_recording_json" )
						.SetTag( "error_type", exception.GetType().Name ) );
					AddRuntimeMessage( GH_RuntimeMessageLevel.Error, "Failed to load recording JSON. Confirm the file exists and matches the Tracker recording format." );
					DA.SetData( 3, telemetry.Status );
				}
			}
		}

		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid( "A9844A2D-C939-4A7F-8823-A8AA3FC96085" ); }
		}
	}
}
