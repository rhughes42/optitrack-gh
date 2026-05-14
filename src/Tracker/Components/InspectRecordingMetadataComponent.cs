using System;

using Grasshopper.Kernel;

using OptiTrack.Recording;


namespace Tracker.Components {

	/// <summary>
	/// Reads non-sensitive metadata from a loaded recording object.
	/// </summary>
	public sealed class InspectRecordingMetadataComponent : GH_Component {

		public InspectRecordingMetadataComponent() : base(
				"Inspect Recording Metadata",
				"RecMeta",
				"Inspect metadata from a loaded OptiTrack recording.",
				"Tracker",
				"Recording") { }


		protected override void RegisterInputParams(GH_InputParamManager pManager) {
			pManager.AddGenericParameter("Recording", "Rec", "Recording object from Load OptiTrack Recording.", GH_ParamAccess.item);
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
			pManager.AddTextParameter("Format Version", "Fmt", "Recording file format version.", GH_ParamAccess.item);
			pManager.AddTextParameter("Plugin Version", "Plugin", "Plugin version captured in metadata.", GH_ParamAccess.item);
			pManager.AddTextParameter("Source", "Src", "Recording source.", GH_ParamAccess.item);
			pManager.AddTextParameter("Units", "Units", "Position units.", GH_ParamAccess.item);
			pManager.AddIntegerParameter("Frame Count", "Count", "Total number of frames.", GH_ParamAccess.item);
			pManager.AddNumberParameter("Duration Seconds", "Dur", "Approximate recording duration in seconds.", GH_ParamAccess.item);
			pManager.AddTextParameter("Recorded UTC", "UTC", "Recording start timestamp in UTC.", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA) {
			object recordingValue = null;

			if (!DA.GetData(0, ref recordingValue)) {
				return;
			}

			if (!(recordingValue is OptiTrackRecording recording)) {
				AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input is not a valid OptiTrack recording object.");

				return;
			}

			DA.SetData(0, recording.FormatVersion);
			DA.SetData(1, recording.Metadata.PluginVersion);
			DA.SetData(2, recording.Metadata.Source);
			DA.SetData(3, recording.Metadata.Units);
			DA.SetData(4, recording.Metadata.FrameCount);
			DA.SetData(5, recording.Metadata.DurationSeconds);
			DA.SetData(6, recording.Metadata.RecordedUtc.ToString("o"));
		}


		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid("4FA96D69-2F0A-4677-90AA-2CC2D676A0B0"); }
		}

	}

}
