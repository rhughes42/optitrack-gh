using System;
using System.Diagnostics;

namespace OptiTrack.Telemetry {

	public sealed class TelemetryScope : IDisposable {
		private readonly ITelemetryService telemetryService;
		private readonly string operationName;
		private readonly TelemetryContext context;
		private readonly Stopwatch stopwatch;
		private bool disposed;

		public TelemetryScope( ITelemetryService telemetryService, string operationName, TelemetryContext context ) {
			this.telemetryService = telemetryService;
			this.operationName = TelemetrySanitizer.SanitizeValue( operationName );
			this.context = context ?? new TelemetryContext();
			stopwatch = Stopwatch.StartNew();
		}

		public void Dispose() {
			if ( disposed ) {
				return;
			}

			disposed = true;
			stopwatch.Stop();
			context.SetMetric( "duration_ms", stopwatch.Elapsed.TotalMilliseconds );
			telemetryService.CaptureMessage( operationName, TelemetrySeverity.Debug, context );
		}
	}
}
