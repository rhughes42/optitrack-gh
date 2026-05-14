/*
 * File: TelemetryServiceProvider.cs
 * Purpose: Process-wide telemetry service selector and lifecycle manager.
 * Scope: Telemetry
 * Notes: Switching enablement disposes prior service to avoid stale Sentry SDK instances.
 */

using System;


namespace OptiTrack.Telemetry {

	/// <summary>
	/// Provides a singleton-style telemetry service for component-level callers.
	/// </summary>
	public static class TelemetryServiceProvider {

		static readonly object            SyncRoot         = new object();
		static          ITelemetryService telemetryService = new NoOpTelemetryService();
		static          bool              telemetryEnabled;


		public static ITelemetryService GetService(bool enableTelemetry) {
			lock (SyncRoot) {
				if (telemetryEnabled == enableTelemetry) {
					return telemetryService;
				}

				telemetryEnabled = enableTelemetry;
				DisposeCurrent();
				telemetryService = SentryTelemetryService.Create(enableTelemetry);

				return telemetryService;
			}
		}


		static void DisposeCurrent() {
			if (telemetryService is IDisposable disposable) {
				disposable.Dispose();
			}
		}

	}

}
