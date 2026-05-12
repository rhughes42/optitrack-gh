using System;

namespace OptiTrack.Telemetry {

	public static class TelemetryServiceProvider {
		private static readonly object SyncRoot = new object();
		private static ITelemetryService telemetryService = new NoOpTelemetryService();
		private static bool telemetryEnabled;

		public static ITelemetryService GetService( bool enableTelemetry ) {
			lock ( SyncRoot ) {
				if ( telemetryEnabled == enableTelemetry ) {
					return telemetryService;
				}

				telemetryEnabled = enableTelemetry;
				DisposeCurrent();
				telemetryService = SentryTelemetryService.Create( enableTelemetry );
				return telemetryService;
			}
		}

		private static void DisposeCurrent() {
			IDisposable disposable = telemetryService as IDisposable;
			if ( disposable != null ) {
				disposable.Dispose();
			}
		}
	}
}
