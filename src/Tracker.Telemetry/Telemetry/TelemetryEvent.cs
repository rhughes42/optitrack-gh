/*
 * File: TelemetryEvent.cs
 * Purpose: Internal telemetry event payload model.
 * Scope: Telemetry
 * Notes: Message content is sanitized at construction.
 */

using System;


namespace OptiTrack.Telemetry {

	/// <summary>
	/// Immutable telemetry event snapshot.
	/// </summary>
	public sealed class TelemetryEvent {

		public TelemetryEvent(string message, TelemetrySeverity severity, TelemetryContext context) {
			Message      = TelemetrySanitizer.SanitizeValue(message);
			Severity     = severity;
			Context      = context ?? new TelemetryContext();
			TimestampUtc = DateTime.UtcNow;
		}


		public string Message { get; private set; }

		public TelemetrySeverity Severity { get; private set; }

		public TelemetryContext Context { get; private set; }

		public DateTime TimestampUtc { get; private set; }

	}

}
