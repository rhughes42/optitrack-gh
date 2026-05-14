using NUnit.Framework;

using OptiTrack.Core;
using OptiTrack.Recording;

namespace Tracker.Tests {

	[TestFixture]
	public class OptiTrackRecordingSessionTests {

		[Test]
		public void AppendFrame_WhenSessionIsNotRecording_DoesNotCaptureFrame() {
			// Arrange
			var sut = new OptiTrackRecordingSession();
			var frame = CreateFrame(frameNumber: 10, timestampSeconds: 1.5);

			// Act
			sut.AppendFrame(frame);

			// Assert
			Assert.That(sut.FrameCount, Is.EqualTo(0));
		}

		[Test]
		public void AppendFrame_WhenFrameIsMutatedAfterCapture_PreservesRecordedSnapshot() {
			// Arrange
			var sut = new OptiTrackRecordingSession();
			var frame = CreateFrame(frameNumber: 1, timestampSeconds: 0.25);
			sut.Start();

			// Act
			sut.AppendFrame(frame);
			frame.Markers[0].X = 99.0;
			frame.RigidBodies[0].Name = "mutated";
			var recording = sut.BuildRecording(units: "meters", source: "live");

			// Assert
			Assert.That(recording.Frames, Has.Count.EqualTo(1));
			Assert.That(recording.Frames[0].Markers[0].X, Is.EqualTo(1.0));
			Assert.That(recording.Frames[0].RigidBodies[0].Name, Is.EqualTo("rb-1"));
		}

		[Test]
		public void BuildRecording_WhenUnitsAndSourceAreBlank_UsesDefaultMetadataValues() {
			// Arrange
			var sut = new OptiTrackRecordingSession();
			sut.Start();
			sut.AppendFrame(CreateFrame(frameNumber: 1, timestampSeconds: 2.0));

			// Act
			var recording = sut.BuildRecording(units: " ", source: null!);

			// Assert
			Assert.That(recording.Metadata.Units, Is.EqualTo("meters"));
			Assert.That(recording.Metadata.Source, Is.EqualTo("live"));
			Assert.That(recording.Metadata.FrameCount, Is.EqualTo(1));
			Assert.That(recording.Metadata.DurationSeconds, Is.EqualTo(0));
		}

		[Test]
		public void BuildRecording_WhenFramesHaveIncreasingTimestamps_ComputesDurationFromFirstToLastFrame() {
			// Arrange
			var sut = new OptiTrackRecordingSession();
			sut.Start();
			sut.AppendFrame(CreateFrame(frameNumber: 1, timestampSeconds: 1.25));
			sut.AppendFrame(CreateFrame(frameNumber: 2, timestampSeconds: 3.75));

			// Act
			var recording = sut.BuildRecording(units: "meters", source: "replay");

			// Assert
			Assert.That(recording.Metadata.DurationSeconds, Is.EqualTo(2.5).Within(1e-12));
		}

		static OptiTrackFrame CreateFrame(int frameNumber, double timestampSeconds) =>
				new OptiTrackFrame(
						frameNumber,
						timestampSeconds,
						0.0,
						false,
						false,
						new[] { new OptiTrackMarker { Id = 1, Label = "m1", X = 1.0, Y = 2.0, Z = 3.0 } },
						new[] {
								new OptiTrackRigidBody {
										Id = 1,
										Name = "rb-1",
										IsTracked = true,
										X = 1.0,
										Y = 2.0,
										Z = 3.0,
										Qx = 0.0,
										Qy = 0.0,
										Qz = 0.0,
										Qw = 1.0
								}
						},
						new[] { "ok" });
	}
}
