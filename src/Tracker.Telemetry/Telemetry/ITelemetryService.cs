/*
 * File: ITelemetryService.cs
 * Purpose: Telemetry abstraction boundary that keeps plugin runtime functional without external telemetry.
 * Scope: Telemetry
 * Notes: Implementations must enforce sanitization and never emit raw capture payloads.
 */

using System;


namespace OptiTrack.Telemetry {

	/// <summary>
	/// Telemetry boundary interface used throughout Tracker runtime operations.
	/// </summary>
	/// <remarks>
	/// This boundary allows a no-op implementation by default and an optional external telemetry implementation
	/// when explicitly enabled/configured.
	/// </remarks>
	public interface ITelemetryService {

		/// <summary>
		/// Gets current telemetry backend status for diagnostics output.
		/// </summary>
		string Status { get; }

		/// <summary>
		/// Captures a runtime exception using sanitized, low-cardinality context.
		/// </summary>
		/// <param name="exception">Exception to record.</param>
		/// <param name="context">Optional sanitized context tags/metrics.</param>
		void CaptureException(Exception exception, TelemetryContext context);

		/// <summary>
		/// Captures a structured telemetry message.
		/// </summary>
		/// <param name="message">Operation/message identifier.</param>
		/// <param name="severity">Normalized severity level.</param>
		/// <param name="context">Optional sanitized context tags/metrics.</param>
		void CaptureMessage(string message, TelemetrySeverity severity, TelemetryContext context);

		/// <summary>
		/// Starts a scoped operation timer/span.
		/// </summary>
		/// <param name="operationName">Low-cardinality operation name.</param>
		/// <param name="context">Optional sanitized context tags/metrics.</param>
		/// <returns>Disposable scope that ends span timing on dispose.</returns>
		TelemetryScope StartSpan(string operationName, TelemetryContext context);

	}

}
