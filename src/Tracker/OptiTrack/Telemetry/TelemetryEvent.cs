using System;


namespace OptiTrack.Telemetry {

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
