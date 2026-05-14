using NUnit.Framework;

using OptiTrack.Telemetry;

namespace Tracker.Tests {

	[TestFixture]
	public class TelemetrySanitizerTests {

		[Test]
		public void SanitizeKey_WhenGivenMixedFormatting_ReturnsNormalizedKey() {
			// Arrange
			const string key = " Frame Count ";

			// Act
			var result = TelemetrySanitizer.SanitizeKey(key);

			// Assert
			Assert.That(result, Is.EqualTo("frame_count"));
		}

		[Test]
		public void SanitizeValue_WhenGivenSensitiveContent_RedactsKnownPatterns() {
			// Arrange
			const string value = "user@example.com 192.168.1.20 C:\\lab\\capture.json";

			// Act
			var result = TelemetrySanitizer.SanitizeValue(value);

			// Assert
			Assert.That(result, Does.Not.Contain("user@example.com"));
			Assert.That(result, Does.Not.Contain("192.168.1.20"));
			Assert.That(result, Does.Not.Contain("C:\\lab\\capture.json"));
			Assert.That(result, Does.Contain("[redacted-email]"));
			Assert.That(result, Does.Contain("[redacted-ip]"));
			Assert.That(result, Does.Contain("[redacted-path]"));
		}

		[Test]
		public void SanitizeTagValue_WhenKeyIsSensitive_ReturnsRedactedValue() {
			// Arrange

			// Act
			var result = TelemetrySanitizer.SanitizeTagValue("file_path", "C:\\lab\\capture.json");

			// Assert
			Assert.That(result, Is.EqualTo("[redacted]"));
		}

		[Test]
		public void SanitizeTagValue_WhenKeyIsOnSafeAllowList_PreservesAggregateValue() {
			// Arrange

			// Act
			var result = TelemetrySanitizer.SanitizeTagValue("marker_count", "42");

			// Assert
			Assert.That(result, Is.EqualTo("42"));
		}
	}
}
