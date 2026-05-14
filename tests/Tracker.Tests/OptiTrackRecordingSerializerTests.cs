using System;
using System.IO;

using NUnit.Framework;

using OptiTrack.Recording;

namespace Tracker.Tests {

	[TestFixture]
	public class OptiTrackRecordingSerializerTests {

		[Test]
		public void LoadJson_WhenMetadataAndFramesAreMissing_NormalizesRecordingShape() {
			// Arrange
			var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json");

			try {
				File.WriteAllText(filePath, "{ \"FormatVersion\": \"1.0\", \"Metadata\": null, \"Frames\": null }");

				// Act
				var recording = OptiTrackRecordingSerializer.LoadJson(filePath);

				// Assert
				Assert.That(recording.Metadata, Is.Not.Null);
				Assert.That(recording.Frames, Is.Not.Null);
				Assert.That(recording.Frames, Is.Empty);
				Assert.That(recording.Metadata.FrameCount, Is.EqualTo(0));
			}
			finally {
				if (File.Exists(filePath)) {
					File.Delete(filePath);
				}
			}
		}

		[Test]
		public void SaveJsonAndLoadJson_WhenGivenValidRecording_RoundTripsExpectedData() {
			// Arrange
			var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json");
			var recording = new OptiTrackRecording {
					FormatVersion = "1.0",
					Metadata = new OptiTrackRecordingMetadata {
							PluginVersion = "1.2.3",
							Units = "meters",
							Source = "live",
							FrameCount = 1,
							DurationSeconds = 0.5
					}
			};

			recording.Frames.Add(new OptiTrack.Core.OptiTrackFrame(frameNumber: 7, timestampSeconds: 4.5, latencySeconds: 0.01, isRecording: true, assetsChanged: false, markers: Array.Empty<OptiTrack.Core.OptiTrackMarker>(), rigidBodies: Array.Empty<OptiTrack.Core.OptiTrackRigidBody>(), statusMessages: Array.Empty<string>()));

			try {
				// Act
				OptiTrackRecordingSerializer.SaveJson(recording, filePath);
				var loaded = OptiTrackRecordingSerializer.LoadJson(filePath);

				// Assert
				Assert.That(loaded.FormatVersion, Is.EqualTo("1.0"));
				Assert.That(loaded.Metadata.PluginVersion, Is.EqualTo("1.2.3"));
				Assert.That(loaded.Metadata.FrameCount, Is.EqualTo(1));
				Assert.That(loaded.Frames, Has.Count.EqualTo(1));
				Assert.That(loaded.Frames[0].FrameNumber, Is.EqualTo(7));
			}
			finally {
				if (File.Exists(filePath)) {
					File.Delete(filePath);
				}
			}
		}

		[Test]
		public void SaveJson_WhenFilePathIsBlank_ThrowsArgumentException() {
			// Arrange
			var recording = new OptiTrackRecording();

			// Act
			var exception = Assert.Throws<ArgumentException>(() => OptiTrackRecordingSerializer.SaveJson(recording, " "));

			// Assert
			Assert.That(exception!.ParamName, Is.EqualTo("filePath"));
		}
	}
}
