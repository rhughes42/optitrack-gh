using System;

using Grasshopper.Kernel;

using OptiTrack.Core;
using OptiTrack.Recording;
using OptiTrack.Telemetry;


namespace Tracker.Components {

	/// <summary>
	/// Records incoming frame objects and writes recording JSON files.
	/// </summary>
	public sealed class RecordOptiTrackStreamComponent : GH_Component {

		private static readonly object                    sync    = new object();
		private static readonly OptiTrackRecordingSession session = new OptiTrackRecordingSession();
		private static          bool                      lastRecordState;


		public RecordOptiTrackStreamComponent() : base(
				"Record OptiTrack Stream",
				"RecordRec",
				"Record OptiTrack frame objects to JSON for replay and offline testing.",
				"Tracker",
				"Recording") { }


		protected override void RegisterInputParams(GH_InputParamManager pManager) {
			pManager.AddGenericParameter("Frame", "F", "OptiTrackFrame input from stream or replay component.", GH_ParamAccess.item);
			pManager.AddBooleanParameter("Record", "Rec", "Start/continue recording while true.", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Save", "Save", "When true, writes current recording to disk.", GH_ParamAccess.item, false);
			pManager.AddTextParameter("Output File", "File", "Output JSON file path.", GH_ParamAccess.item, "optitrack-recording.json");
			pManager.AddTextParameter("Units", "Units", "Metadata units label (for example meters or millimeters).", GH_ParamAccess.item, "meters");
			pManager.AddTextParameter("Source", "Src", "Metadata source label (for example live or replay).", GH_ParamAccess.item, "live");
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false);
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
			pManager.AddBooleanParameter("Is Recording", "Rec", "True while recording is active.", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Frame Count", "Count", "Current buffered frame count.", GH_ParamAccess.item);
			pManager.AddGenericParameter("Recording", "Recording", "Current in-memory recording object.", GH_ParamAccess.item);
			pManager.AddTextParameter("Status", "Status", "Recording status messages.", GH_ParamAccess.item);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA) {
			object frameValue      = null;
			bool   record          = false;
			bool   save            = false;
			bool   enableTelemetry = false;
			string outputFile      = "optitrack-recording.json";
			string units           = "meters";
			string source          = "live";

			DA.GetData(0, ref frameValue);
			DA.GetData(1, ref record);
			DA.GetData(2, ref save);
			DA.GetData(3, ref outputFile);
			DA.GetData(4, ref units);
			DA.GetData(5, ref source);
			DA.GetData(6, ref enableTelemetry);

			ITelemetryService telemetry = TelemetryServiceProvider.GetService(enableTelemetry);
			TelemetryContext  context   = new TelemetryContext().SetTag("component", "record_optitrack_stream");

			string         status = "Idle.";
			OptiTrackFrame frame  = frameValue as OptiTrackFrame;

			lock (sync) {
				if (record && !lastRecordState) {
					session.Start();
					telemetry.CaptureMessage("recording_started", TelemetrySeverity.Info, new TelemetryContext().SetTag("operation", "record_start"));
					status = "Recording started.";
				}
				else if (!record && lastRecordState) {
					session.Stop();
					telemetry.CaptureMessage("recording_stopped", TelemetrySeverity.Info, new TelemetryContext().SetTag("operation", "record_stop"));
					status = "Recording stopped.";
				}

				lastRecordState = record;
				session.AppendFrame(frame);

				if (save) {
					using (telemetry.StartSpan("save_recording_json", context)) {
						try {
							OptiTrackRecording recording = session.BuildRecording(units, source);
							context.SetMetric("frame_count", recording.Metadata.FrameCount);
							context.SetTag("format_version", recording.FormatVersion);
							OptiTrackRecordingSerializer.SaveJson(recording, outputFile);
							status = "Recording saved.";
						}
						catch (Exception exception) {
							telemetry.CaptureException(
									exception,
									new TelemetryContext().SetTag("operation", "save_recording_json").SetTag("error_type", exception.GetType().Name));

							AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to save recording JSON.");
							status = "Save failed.";
						}
					}
				}

				OptiTrackRecording currentRecording = session.BuildRecording(units, source);
				DA.SetData(0, session.IsRecording);
				DA.SetData(1, currentRecording.Metadata.FrameCount);
				DA.SetData(2, currentRecording);
				DA.SetData(3, status);
				DA.SetData(4, telemetry.Status);
			}
		}


		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid("D6FCB90D-C9FC-4A20-952A-C5F238EF8C03"); }
		}

	}

}
