using System;
using System.Collections.Generic;
using System.Threading;

using Grasshopper.Kernel;

using OptiTrack.Core;
using OptiTrack.Recording;
using OptiTrack.Telemetry;


namespace Tracker.Components {

	public sealed class ReplayOptiTrackRecordingComponent : GH_Component {

		private static readonly object                sync         = new object();
		private static readonly OptiTrackReplayClient replayClient = new OptiTrackReplayClient();
		private static          OptiTrackRecording    lastRecording;
		private static          OptiTrackFrame        currentFrame;
		private static readonly List<string>          status = new List<string>();
		private static          bool                  handlersAttached;


		public ReplayOptiTrackRecordingComponent() : base(
				"Replay OptiTrack Recording",
				"ReplayRec",
				"Replay recorded OptiTrack frames without a live Motive connection.",
				"Tracker",
				"Recording") { }


		protected override void RegisterInputParams(GH_InputParamManager pManager) {
			pManager.AddGenericParameter("Recording", "Rec", "Recording object from Load OptiTrack Recording.", GH_ParamAccess.item);
			pManager.AddBooleanParameter("Play", "Play", "Start/continue playback while true.", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Pause", "Pause", "Pause playback without disconnecting.", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Loop", "Loop", "Loop playback when the last frame is reached.", GH_ParamAccess.item, false);
			pManager.AddNumberParameter("Speed", "Speed", "Playback speed multiplier. 1.0 = realtime.", GH_ParamAccess.item, 1.0);
			pManager.AddIntegerParameter("Scrub Index", "Index", "Optional frame index scrub. -1 keeps current playback position.", GH_ParamAccess.item, -1);
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false);
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
			pManager.AddGenericParameter("Frame", "F", "Current replay frame as OptiTrackFrame.", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Frame Number", "Frame", "Current frame number.", GH_ParamAccess.item);
			pManager.AddNumberParameter("Timestamp", "Time", "Current frame timestamp in seconds.", GH_ParamAccess.item);
			pManager.AddBooleanParameter("Is Playing", "Playing", "True while replay client is connected.", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Late Frames", "Late", "Count of replay frames that were delayed significantly.", GH_ParamAccess.item);
			pManager.AddTextParameter("Status", "Status", "Replay status messages.", GH_ParamAccess.list);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA) {
			object recordingValue  = null;
			bool   play            = false;
			bool   pause           = false;
			bool   loop            = false;
			bool   enableTelemetry = false;
			double speed           = 1.0;
			int    scrubIndex      = -1;

			if (!DA.GetData(0, ref recordingValue)) {
				return;
			}

			DA.GetData(1, ref play);
			DA.GetData(2, ref pause);
			DA.GetData(3, ref loop);
			DA.GetData(4, ref speed);
			DA.GetData(5, ref scrubIndex);
			DA.GetData(6, ref enableTelemetry);

			OptiTrackRecording recording = recordingValue as OptiTrackRecording;

			if (recording == null) {
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input Recording is not valid.");

				return;
			}

			ITelemetryService telemetry = TelemetryServiceProvider.GetService(enableTelemetry);

			TelemetryContext context = new TelemetryContext().SetTag("component", "replay_recording").SetTag("format_version", recording.FormatVersion)
															 .SetMetric("frame_count", recording.Frames.Count);

			using (telemetry.StartSpan("replay_component_solve", context)) {
				try {
					EnsureHandlers();

					lock (sync) {
						if (!ReferenceEquals(recording, lastRecording)) {
							lastRecording = recording;
							replayClient.LoadRecording(recording);
							status.Clear();
							status.Add("Replay recording loaded.");

							telemetry.CaptureMessage(
									"replay_loaded",
									TelemetrySeverity.Info,
									new TelemetryContext().SetTag("operation", "replay_load").SetTag("format_version", recording.FormatVersion)
														  .SetMetric("frame_count", recording.Frames.Count));
						}

						replayClient.LoopPlayback  = loop;
						replayClient.PlaybackSpeed = speed <= 0 ? 1.0 : speed;

						if (scrubIndex >= 0) {
							replayClient.SetFrameIndex(scrubIndex);
						}

						if (pause) {
							replayClient.Pause();
						}
						else {
							replayClient.Resume();
						}

						if (play && !replayClient.IsConnected) {
							replayClient.ConnectAsync(new OptiTrackConnectionOptions(), CancellationToken.None).GetAwaiter().GetResult();
							status.Add("Replay started.");
						}

						if (!play && replayClient.IsConnected) {
							replayClient.DisconnectAsync().GetAwaiter().GetResult();
							status.Add("Replay stopped.");
						}
					}
				}
				catch (Exception exception) {
					telemetry.CaptureException(
							exception,
							new TelemetryContext().SetTag("operation", "replay_component").SetTag("error_type", exception.GetType().Name));

					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Replay operation failed.");
				}
			}

			DA.SetData(0, currentFrame);
			DA.SetData(1, currentFrame == null ? 0 : currentFrame.FrameNumber);
			DA.SetData(2, currentFrame == null ? 0 : currentFrame.TimestampSeconds);
			DA.SetData(3, replayClient.IsConnected);
			DA.SetData(4, replayClient.DroppedOrLateFrames);
			DA.SetDataList(5, status);
			DA.SetData(6, telemetry.Status);

			if (play) {
				ExpireSolution(true);
			}
		}


		private void EnsureHandlers() {
			if (handlersAttached) {
				return;
			}

			replayClient.FrameReceived += (sender, args) => { currentFrame = args.Frame; };

			replayClient.ConnectionChanged += (sender, args) => {
												  if (!string.IsNullOrWhiteSpace(args.Message)) {
													  status.Add(args.Message);
												  }
											  };

			handlersAttached = true;
		}


		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid("DCAFFE40-B3A2-4DCD-8620-E66F9968D87C"); }
		}

	}

}
