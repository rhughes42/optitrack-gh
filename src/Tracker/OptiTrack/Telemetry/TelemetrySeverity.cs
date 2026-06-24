namespace OptiTrack.Telemetry {

	/// <summary>
	/// Defines telemetry event severity levels.
	/// </summary>
	public enum TelemetrySeverity {
		/// <summary>
		/// Verbose diagnostic information.
		/// </summary>
		Debug,
		/// <summary>
		/// Informational status message.
		/// </summary>
		Info,
		/// <summary>
		/// Non-fatal warning condition.
		/// </summary>
		Warning,
		/// <summary>
		/// Error condition that impacted normal operation.
		/// </summary>
		Error,
		/// <summary>
		/// Critical condition requiring immediate attention.
		/// </summary>
		Fatal
	}
}
