using System;

namespace OptiTrack.Telemetry {

	/// <summary>
	/// Defines telemetry capture operations used by Tracker runtime services.
	/// </summary>
	public interface ITelemetryService {

		/// <summary>
		/// Gets a short status string for diagnostics and UI reporting.
		/// </summary>
		string Status { get; }

		/// <summary>
		/// Captures an exception event.
		/// </summary>
		/// <param name="exception">The exception to capture.</param>
		/// <param name="context">Additional tags and metrics for the event.</param>
		void CaptureException( Exception exception, TelemetryContext context );

		/// <summary>
		/// Captures a message event.
		/// </summary>
		/// <param name="message">The message to capture.</param>
		/// <param name="severity">The message severity level.</param>
		/// <param name="context">Additional tags and metrics for the event.</param>
		void CaptureMessage( string message, TelemetrySeverity severity, TelemetryContext context );

		/// <summary>
		/// Starts a timed telemetry span for an operation.
		/// </summary>
		/// <param name="operationName">The operation name to report.</param>
		/// <param name="context">Initial span context.</param>
		/// <returns>A disposable scope that emits duration telemetry when disposed.</returns>
		TelemetryScope StartSpan( string operationName, TelemetryContext context );
	}
}
