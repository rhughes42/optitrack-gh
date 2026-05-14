using System;
using System.Collections.Generic;
using System.Reflection;

using OptiTrack.Core;

namespace OptiTrack.Recording {

	public sealed class OptiTrackRecordingSession {
		private readonly List<OptiTrackFrame> frames = new List<OptiTrackFrame>();
		private DateTime startedUtc = DateTime.MinValue;

		public bool IsRecording { get; private set; }

		public int FrameCount {
			get { return frames.Count; }
		}

		public void Start() {
			frames.Clear();
			startedUtc = DateTime.UtcNow;
			IsRecording = true;
		}

		public void Stop() {
			IsRecording = false;
		}

		public void AppendFrame( OptiTrackFrame frame ) {
			if ( !IsRecording || frame == null ) {
				return;
			}

			frames.Add( OptiTrackFrameSnapshot.Clone( frame ) );
		}

		public OptiTrackRecording BuildRecording( string units, string source ) {
			if ( startedUtc == DateTime.MinValue ) {
				startedUtc = DateTime.UtcNow;
			}

			OptiTrackRecording recording = new OptiTrackRecording();
			recording.Metadata.PluginVersion = GetPluginVersion();
			recording.Metadata.RecordedUtc = startedUtc;
			recording.Metadata.Units = string.IsNullOrWhiteSpace( units ) ? "meters" : units.Trim();
			recording.Metadata.Source = string.IsNullOrWhiteSpace( source ) ? "live" : source.Trim();
			recording.Metadata.FrameCount = frames.Count;
			recording.Metadata.DurationSeconds = CalculateDurationSeconds();
			recording.Frames = new List<OptiTrackFrame>( frames );
			return recording;
		}

		private double CalculateDurationSeconds() {
			if ( frames.Count <= 1 ) {
				return 0;
			}

			double start = frames[ 0 ].TimestampSeconds;
			double end = frames[ frames.Count - 1 ].TimestampSeconds;
			double duration = end - start;
			return duration < 0 ? 0 : duration;
		}

		private static string GetPluginVersion() {
			Assembly assembly = typeof( OptiTrackRecordingSession ).Assembly;
			AssemblyInformationalVersionAttribute attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			return attribute == null || string.IsNullOrWhiteSpace( attribute.InformationalVersion ) ? "unknown" : attribute.InformationalVersion;
		}
	}
}
