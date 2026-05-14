/*
 * File: OptiTrackRecordingModels.cs
 * Purpose: Recording container models for serialization and replay.
 * Scope: Replay
 * Notes: Recording files may contain sensitive capture payloads and should be treated as local lab data.
 */

using System;
using System.Collections.Generic;

using OptiTrack.Core;


namespace OptiTrack.Recording {

	/// <summary>
	/// Serialized recording root object.
	/// </summary>
	public sealed class OptiTrackRecording {

		public OptiTrackRecording() {
			Metadata = new OptiTrackRecordingMetadata();
			Frames   = new List<OptiTrackFrame>();
		}


		public string FormatVersion { get; set; } = "1.0";

		public OptiTrackRecordingMetadata Metadata { get; set; }

		public List<OptiTrackFrame> Frames { get; set; }

	}


	/// <summary>
	/// Metadata describing a recorded stream snapshot.
	/// </summary>
	public sealed class OptiTrackRecordingMetadata {

		public string PluginVersion { get; set; } = "unknown";

		public DateTime RecordedUtc { get; set; } = DateTime.UtcNow;

		public string Units { get; set; } = "meters";

		public string Source { get; set; } = "live";

		public int FrameCount { get; set; }

		public double DurationSeconds { get; set; }

	}

}
