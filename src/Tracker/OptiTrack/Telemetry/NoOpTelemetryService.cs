using System;

namespace OptiTrack.Telemetry {

	/// <summary>
	/// Telemetry implementation that performs no external capture.
	/// </summary>
	public sealed class NoOpTelemetryService : ITelemetryService {

		/// <summary>
		/// Initializes a disabled no-op telemetry service.
		/// </summary>
		public NoOpTelemetryService()
			: this( "disabled" ) {
		}

		/// <summary>
		/// Initializes a no-op telemetry service with a custom status label.
		/// </summary>
		/// <param name="status">Status text used for diagnostics.</param>
		public NoOpTelemetryService( string status ) {
			Status = status;
		}

		/// <summary>
		/// Gets the telemetry status label.
		/// </summary>
		public string Status { get; private set; }

		/// <summary>
		/// Ignores exception capture requests.
		/// </summary>
		/// <param name="exception">The exception to ignore.</param>
		/// <param name="context">The context to ignore.</param>
		public void CaptureException( Exception exception, TelemetryContext context ) {
		}

		/// <summary>
		/// Ignores message capture requests.
		/// </summary>
		/// <param name="message">The message to ignore.</param>
		/// <param name="severity">The severity to ignore.</param>
		/// <param name="context">The context to ignore.</param>
		public void CaptureMessage( string message, TelemetrySeverity severity, TelemetryContext context ) {
		}

		/// <summary>
		/// Starts a local scope that still records duration metrics to context.
		/// </summary>
		/// <param name="operationName">The operation name.</param>
		/// <param name="context">The context to enrich.</param>
		/// <returns>A telemetry scope instance.</returns>
		public TelemetryScope StartSpan( string operationName, TelemetryContext context ) {
			return new TelemetryScope( this, operationName, context );
		}
	}
}
