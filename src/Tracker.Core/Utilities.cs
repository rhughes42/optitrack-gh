using System;

namespace Tracker {

	/// <summary>
	/// Contains utility methods for the Tracker namespace.
	/// </summary>
	public static class Utilities {

		/// <summary>
		/// Converts an angle in radians to degrees.
		/// </summary>
		/// <param name="dRads"> The angle in radians. </param>
		/// <returns> The angle in degrees. </returns>
		public static double RadiansToDegrees(double dRads) => dRads * (180.0 / Math.PI);

		/// <summary>
		/// Retrieves the low word of a 32-bit integer.
		/// </summary>
		/// <param name="number"> The 32-bit integer. </param>
		/// <returns> The low word of the integer. </returns>
		public static int LowWord(int number) => number & 0xFFFF;

		/// <summary>
		/// Retrieves the high word of a 32-bit integer.
		/// </summary>
		/// <param name="number"> The 32-bit integer. </param>
		/// <returns> The high word of the integer. </returns>
		public static int HighWord(int number) => (number >> 16) & 0xFFFF;
	}
}
