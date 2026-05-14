/*
 * File: OptiTrackRecordingSerializer.cs
 * Purpose: JSON load/save helpers for OptiTrack recordings.
 * Scope: Replay
 * Notes: Performs normalization checks for missing metadata/frame collections after deserialization.
 */

using System;
using System.IO;

using Newtonsoft.Json;


namespace OptiTrack.Recording {

	/// <summary>
	/// JSON serialization helpers for <see cref="OptiTrackRecording"/>.
	/// </summary>
	public static class OptiTrackRecordingSerializer {

		/// <summary>
		/// Saves a recording to JSON on disk.
		/// </summary>
		/// <param name="recording">Recording payload to save.</param>
		/// <param name="filePath">Destination JSON path.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="recording"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty.</exception>
		public static void SaveJson(OptiTrackRecording recording, string filePath) {
			if (recording == null) {
				throw new ArgumentNullException(nameof(recording));
			}

			if (string.IsNullOrWhiteSpace(filePath)) {
				throw new ArgumentException("File path is required.", nameof(filePath));
			}

			string json = JsonConvert.SerializeObject(recording, Formatting.Indented);
			File.WriteAllText(filePath, json);
		}


		/// <summary>
		/// Loads a recording from JSON on disk.
		/// </summary>
		/// <param name="filePath">Path to the recording JSON file.</param>
		/// <returns>Normalized recording object.</returns>
		/// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty.</exception>
		/// <exception cref="InvalidDataException">Thrown when JSON cannot be deserialized into recording payload.</exception>
		public static OptiTrackRecording LoadJson(string filePath) {
			if (string.IsNullOrWhiteSpace(filePath)) {
				throw new ArgumentException("File path is required.", nameof(filePath));
			}

			string             json      = File.ReadAllText(filePath);
			OptiTrackRecording recording = JsonConvert.DeserializeObject<OptiTrackRecording>(json);

			if (recording == null) {
				throw new InvalidDataException("Recording file could not be parsed.");
			}

			if (recording.Metadata == null) {
				recording.Metadata = new OptiTrackRecordingMetadata();
			}

			if (recording.Frames == null) {
				recording.Frames = new System.Collections.Generic.List<Core.OptiTrackFrame>();
			}

			recording.Metadata.FrameCount = recording.Frames.Count;

			return recording;
		}

	}

}
