using System;

namespace OptiTrack.Telemetry {

	public sealed class NoOpTelemetryService : ITelemetryService {

		public void CaptureException( Exception exception, TelemetryContext context ) {
		}

		public void CaptureMessage( string message, TelemetrySeverity severity, TelemetryContext context ) {
		}

		public TelemetryScope StartSpan( string operationName, TelemetryContext context ) {
			return new TelemetryScope( this, operationName, context );
		}
	}
}
