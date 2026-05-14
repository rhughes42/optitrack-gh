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

		private static readonly object            SyncRoot         = new object();
		private static          ITelemetryService telemetryService = new NoOpTelemetryService();
		private static          bool              telemetryEnabled;


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


		private static void DisposeCurrent() {
			IDisposable disposable = telemetryService as IDisposable;

			if (disposable != null) {
				disposable.Dispose();
			}
		}

	}

}
