using System;
using NUnit.Framework;
using Tracker;

namespace TrackerTests
{
	[TestFixture]
	public class UtilitiesTests
	{
		[TestCase(0.0, 0.0)]
		[TestCase(Math.PI / 2.0, 90.0)]
		[TestCase(Math.PI, 180.0)]
		[TestCase(-Math.PI, -180.0)]
		public void RadiansToDegrees_WhenGivenCommonAngles_ReturnsExpectedDegrees(double radians, double expectedDegrees)
		{
			// Arrange

			// Act
			var result = Utilities.RadiansToDegrees(radians);

			// Assert
			Assert.That(result, Is.EqualTo(expectedDegrees).Within(1e-12));
		}

		[Test]
		public void RadiansToDegrees_WhenGivenNaN_ReturnsNaN()
		{
			// Arrange
			const double radians = double.NaN;

			// Act
			var result = Utilities.RadiansToDegrees(radians);

			// Assert
			Assert.That(result, Is.NaN);
		}

		[Test]
		public void RadiansToDegrees_WhenGivenPositiveInfinity_ReturnsPositiveInfinity()
		{
			// Arrange
			const double radians = double.PositiveInfinity;

			// Act
			var result = Utilities.RadiansToDegrees(radians);

			// Assert
			Assert.That(result, Is.EqualTo(double.PositiveInfinity));
		}

		[TestCase(0x12345678, 0x5678)]
		[TestCase(-1, 0xFFFF)]
		[TestCase(int.MinValue, 0x0000)]
		[TestCase(int.MaxValue, 0xFFFF)]
		public void LowWord_WhenCalled_ReturnsLower16Bits(int input, int expectedLowWord)
		{
			// Arrange

			// Act
			var result = Utilities.LowWord(input);

			// Assert
			Assert.That(result, Is.EqualTo(expectedLowWord));
		}

		[TestCase(0x12345678, 0x1234)]
		[TestCase(-1, 0xFFFF)]
		[TestCase(int.MinValue, 0x8000)]
		[TestCase(int.MaxValue, 0x7FFF)]
		public void HighWord_WhenCalled_ReturnsUpper16Bits(int input, int expectedHighWord)
		{
			// Arrange

			// Act
			var result = Utilities.HighWord(input);

			// Assert
			Assert.That(result, Is.EqualTo(expectedHighWord));
		}

		[TestCase(0)]
		[TestCase(1)]
		[TestCase(-1)]
		[TestCase(0x12345678)]
		[TestCase(int.MinValue)]
		[TestCase(int.MaxValue)]
		public void HighWordAndLowWord_WhenRecombined_ReconstructOriginalBitPattern(int input)
		{
			// Arrange

			// Act
			var low = Utilities.LowWord(input);
			var high = Utilities.HighWord(input);
			var recombined = (high << 16) | low;

			// Assert
			Assert.That(recombined, Is.EqualTo(input));
		}
	}
}