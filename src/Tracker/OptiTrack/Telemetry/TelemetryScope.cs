using System;
using System.Diagnostics;

namespace OptiTrack.Telemetry {

	/// <summary>
	/// Times an operation and emits a duration metric when disposed.
	/// </summary>
	public sealed class TelemetryScope : IDisposable {
		private readonly ITelemetryService telemetryService;
		private readonly string operationName;
		private readonly TelemetryContext context;
		private readonly Stopwatch stopwatch;
		private bool disposed;

		/// <summary>
		/// Initializes a telemetry timing scope.
		/// </summary>
		/// <param name="telemetryService">Telemetry service used to emit the completion event.</param>
		/// <param name="operationName">Operation name for the completion event.</param>
		/// <param name="context">Context to enrich with duration metrics.</param>
		public TelemetryScope( ITelemetryService telemetryService, string operationName, TelemetryContext context ) {
			this.telemetryService = telemetryService;
			this.operationName = TelemetrySanitizer.SanitizeValue( operationName );
			this.context = context ?? new TelemetryContext();
			stopwatch = Stopwatch.StartNew();
		}

		/// <summary>
		/// Stops timing and emits completion telemetry once.
		/// </summary>
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
