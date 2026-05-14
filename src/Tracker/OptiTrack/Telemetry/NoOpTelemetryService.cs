/*
 * File: NoOpTelemetryService.cs
 * Purpose: Default telemetry implementation that performs no outbound reporting.
 * Scope: Telemetry
 * Notes: Keeps Sentry optional and disabled unless explicitly configured.
 */

using System;


namespace OptiTrack.Telemetry {

	/// <summary>
	/// No-op telemetry implementation used when telemetry is disabled or unavailable.
	/// </summary>
	public sealed class NoOpTelemetryService : ITelemetryService {

		public NoOpTelemetryService() : this("disabled") { }


		public NoOpTelemetryService(string status) {
			Status = status;
		}


		public string Status { get; private set; }

		public void CaptureException(Exception exception, TelemetryContext context) { }

		public void CaptureMessage(string message, TelemetrySeverity severity, TelemetryContext context) { }


		public TelemetryScope StartSpan(string operationName, TelemetryContext context) {
			return new TelemetryScope(this, operationName, context);
		}

	}

}
