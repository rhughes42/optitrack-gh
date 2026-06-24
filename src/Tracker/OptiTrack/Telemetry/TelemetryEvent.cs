using System;

namespace OptiTrack.Telemetry {

	/// <summary>
	/// Represents a sanitized telemetry payload.
	/// </summary>
	public sealed class TelemetryEvent {

		/// <summary>
		/// Initializes a telemetry event with sanitized message content and default context fallback.
		/// </summary>
		/// <param name="message">Event message text.</param>
		/// <param name="severity">Event severity level.</param>
		/// <param name="context">Event context tags and metrics.</param>
		public TelemetryEvent( string message, TelemetrySeverity severity, TelemetryContext context ) {
			Message = TelemetrySanitizer.SanitizeValue( message );
			Severity = severity;
			Context = context ?? new TelemetryContext();
			TimestampUtc = DateTime.UtcNow;
		}

		/// <summary>
		/// Gets the sanitized event message.
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		/// Gets the event severity.
		/// </summary>
		public TelemetrySeverity Severity { get; private set; }

		/// <summary>
		/// Gets the event context.
		/// </summary>
		public TelemetryContext Context { get; private set; }

		/// <summary>
		/// Gets the UTC timestamp when this event was created.
		/// </summary>
		public DateTime TimestampUtc { get; private set; }
	}
}
