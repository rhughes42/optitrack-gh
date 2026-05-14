/*
 * File: OptiTrackRecordingSession.cs
 * Purpose: In-memory recording accumulator for live OptiTrack frames.
 * Scope: Replay
 * Notes: Stores cloned frames so downstream processing does not mutate recording contents.
 */

using System;
using System.Collections.Generic;
using System.Reflection;

using OptiTrack.Core;


namespace OptiTrack.Recording {

	/// <summary>
	/// Stateful recorder that captures frame snapshots and emits <see cref="OptiTrackRecording"/>.
	/// </summary>
	public sealed class OptiTrackRecordingSession {

		readonly List<OptiTrackFrame> frames     = new List<OptiTrackFrame>();
		DateTime                      startedUtc = DateTime.MinValue;

		public bool IsRecording { get; private set; }

		public int FrameCount {
			get { return frames.Count; }
		}


		/// <summary>
		/// Starts a new recording session and clears prior captured frames.
		/// </summary>
		public void Start() {
			frames.Clear();
			startedUtc  = DateTime.UtcNow;
			IsRecording = true;
		}


		/// <summary>
		/// Stops recording while keeping captured frames in memory.
		/// </summary>
		public void Stop() {
			IsRecording = false;
		}


		/// <summary>
		/// Appends a frame snapshot when recording is active.
		/// </summary>
		/// <param name="frame">Frame to append.</param>
		public void AppendFrame(OptiTrackFrame? frame) {
			if (!IsRecording || (frame == null)) {
				return;
			}

			frames.Add(OptiTrackFrameSnapshot.Clone(frame)!);
		}


		/// <summary>
		/// Builds an immutable recording payload from captured frames.
		/// </summary>
		/// <param name="units">Unit label for metadata.</param>
		/// <param name="source">Source label for metadata.</param>
		/// <returns>Recording payload ready for serialization or replay.</returns>
		public OptiTrackRecording BuildRecording(string units, string source) {
			if (startedUtc == DateTime.MinValue) {
				startedUtc = DateTime.UtcNow;
			}

			OptiTrackRecording recording = new OptiTrackRecording();
			recording.Metadata.PluginVersion   = GetPluginVersion();
			recording.Metadata.RecordedUtc     = startedUtc;
			recording.Metadata.Units           = string.IsNullOrWhiteSpace(units) ? "meters" : units.Trim();
			recording.Metadata.Source          = string.IsNullOrWhiteSpace(source) ? "live" : source.Trim();
			recording.Metadata.FrameCount      = frames.Count;
			recording.Metadata.DurationSeconds = CalculateDurationSeconds();
			recording.Frames                   = new List<OptiTrackFrame>(frames);

			return recording;
		}


		double CalculateDurationSeconds() {
			if (frames.Count <= 1) {
				return 0;
			}

			double start    = frames[0].TimestampSeconds;
			double end      = frames[frames.Count - 1].TimestampSeconds;
			double duration = end - start;

			return duration < 0 ? 0 : duration;
		}


		static string GetPluginVersion() {
			Assembly                              assembly  = typeof(OptiTrackRecordingSession).Assembly;
			AssemblyInformationalVersionAttribute attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

			return (attribute == null) || string.IsNullOrWhiteSpace(attribute.InformationalVersion) ? "unknown" : attribute.InformationalVersion;
		}

	}

}
