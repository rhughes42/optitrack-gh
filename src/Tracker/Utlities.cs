using System;

namespace Tracker {

	/// <summary>
	/// Contains utility methods for the Tracker namespace.
	/// </summary>
	public static class Utlities {

		/// <summary>
		/// Converts an angle in radians to degrees.
		/// </summary>
		/// <param name="dRads"> The angle in radians. </param>
		/// <returns> The angle in degrees. </returns>
		public static double RadiansToDegrees( double dRads ) {
			return dRads * ( 180.0f / Math.PI );
		}

		/// <summary>
		/// Retrieves the low word of a 32-bit integer.
		/// </summary>
		/// <param name="number"> The 32-bit integer. </param>
		/// <returns> The low word of the integer. </returns>
		public static int LowWord( int number ) {
			return number & 0xFFFF;
		}

		/// <summary>
		/// Retrieves the high word of a 32-bit integer.
		/// </summary>
		/// <param name="number"> The 32-bit integer. </param>
		/// <returns> The high word of the integer. </returns>
		public static int HighWord( int number ) {
			return ( ( number >> 16 ) & 0xFFFF );
		}
	}
}