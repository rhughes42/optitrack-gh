/*
 * File: TelemetryScope.cs
 * Purpose: Disposable helper that measures operation duration and reports aggregate timing.
 * Scope: Telemetry
 * Notes: Emits only sanitized aggregate metrics; does not capture payload data.
 */

using System;
using System.Diagnostics;


namespace OptiTrack.Telemetry {

	/// <summary>
	/// Disposable telemetry scope for operation timing.
	/// </summary>
	public sealed class TelemetryScope : IDisposable {

		readonly ITelemetryService telemetryService;
		readonly string            operationName;
		readonly TelemetryContext  context;
		readonly Stopwatch         stopwatch;
		bool                       disposed;


		/// <summary>
		/// Starts a timing scope for a telemetry operation.
		/// </summary>
		/// <param name="telemetryService">Telemetry service that receives scope completion event.</param>
		/// <param name="operationName">Low-cardinality operation name.</param>
		/// <param name="context">Optional telemetry context.</param>
		public TelemetryScope(ITelemetryService telemetryService, string operationName, TelemetryContext context) {
			this.telemetryService = telemetryService;
			this.operationName    = TelemetrySanitizer.SanitizeValue(operationName);
			this.context          = context ?? new TelemetryContext();
			stopwatch             = Stopwatch.StartNew();
		}


		/// <summary>
		/// Completes the scope and emits aggregate duration metric.
		/// </summary>
		public void Dispose() {
			if (disposed) {
				return;
			}

			disposed = true;
			stopwatch.Stop();
			context.SetMetric("duration_ms", stopwatch.Elapsed.TotalMilliseconds);
			telemetryService.CaptureMessage(operationName, TelemetrySeverity.Debug, context);
		}

	}

}
