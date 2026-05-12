using System;

namespace OptiTrack.Telemetry {

	public interface ITelemetryService {

		string Status { get; }

		void CaptureException( Exception exception, TelemetryContext context );

		void CaptureMessage( string message, TelemetrySeverity severity, TelemetryContext context );

		TelemetryScope StartSpan( string operationName, TelemetryContext context );
	}
}
