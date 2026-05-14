using System;
using System.Collections.Generic;

using OptiTrack.Core;

namespace OptiTrack.Recording {

	public sealed class OptiTrackRecording {

		public OptiTrackRecording() {
			Metadata = new OptiTrackRecordingMetadata();
			Frames = new List<OptiTrackFrame>();
		}

		public string FormatVersion { get; set; } = "1.0";

		public OptiTrackRecordingMetadata Metadata { get; set; }

		public List<OptiTrackFrame> Frames { get; set; }
	}

	public sealed class OptiTrackRecordingMetadata {

		public string PluginVersion { get; set; } = "unknown";

		public DateTime RecordedUtc { get; set; } = DateTime.UtcNow;

		public string Units { get; set; } = "meters";

		public string Source { get; set; } = "live";

		public int FrameCount { get; set; }

		public double DurationSeconds { get; set; }
	}
}
