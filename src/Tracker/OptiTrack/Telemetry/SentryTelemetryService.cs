using System;
using System.Reflection;

using Sentry;


namespace OptiTrack.Telemetry {

	public sealed class SentryTelemetryService : ITelemetryService, IDisposable {

		private readonly IDisposable sentry;
		private          bool        disposed;


		private SentryTelemetryService(IDisposable sentry, string status) {
			this.sentry = sentry;
			Status      = status;
		}


		public string Status { get; private set; }


		public static ITelemetryService Create(bool enabled) {
			if (!enabled) {
				return new NoOpTelemetryService("disabled");
			}

			try {
				SentryTelemetryOptions configuration = SentryTelemetryOptions.Load();

				if (!configuration.HasValidDsn()) {
					return new NoOpTelemetryService("disabled: Sentry DSN missing or invalid");
				}

				IDisposable sentry = SentrySdk.Init(options => {
														options.Dsn = configuration.Dsn;

														options.Environment = string.IsNullOrWhiteSpace(configuration.Environment)
																? "local"
																: TelemetrySanitizer.SanitizeValue(configuration.Environment);

														options.Release          = TelemetrySanitizer.SanitizeValue(configuration.Release);
														options.SendDefaultPii   = false;
														options.AttachStacktrace = true;
														options.TracesSampleRate = ClampSampleRate(configuration.TracesSampleRate);

														options.SetBeforeSend(evt => {
																				  evt.ServerName = null;
																				  evt.Request    = null;

																				  return evt;
																			  });
													});

				SentrySdk.SetTag("plugin_version", GetPluginVersion());
				SentrySdk.SetTag("adapter_name", ResolveAdapterName());
				SentrySdk.SetTag("rhino_version", GetRhinoVersion());
				SentrySdk.SetTag("sentry_sdk_version", typeof(SentrySdk).Assembly.GetName().Version?.ToString() ?? "unknown");

				return new SentryTelemetryService(sentry, "active");
			}
			catch {
				return new NoOpTelemetryService("failed: Sentry configuration could not be initialized");
			}
		}


		public void CaptureException(Exception exception, TelemetryContext context) {
			if (exception == null) {
				return;
			}

			try {
				Exception sanitizedException = new Exception(TelemetrySanitizer.SanitizeValue(exception.GetType().Name + ": " + exception.Message));

				SentrySdk.CaptureException(
						sanitizedException,
						scope => {
							ApplyContext(scope, context);
							scope.SetTag("exception_type", TelemetrySanitizer.SanitizeValue(exception.GetType().Name));
						});
			}
			catch {
				Status = "failed: Sentry capture failed";
			}
		}


		public void CaptureMessage(string message, TelemetrySeverity severity, TelemetryContext context) {
			try {
				SentrySdk.CaptureMessage(TelemetrySanitizer.SanitizeValue(message), scope => ApplyContext(scope, context), ToSentryLevel(severity));
			}
			catch {
				Status = "failed: Sentry capture failed";
			}
		}


		public TelemetryScope StartSpan(string operationName, TelemetryContext context) {
			return new TelemetryScope(this, operationName, context);
		}


		public void Dispose() {
			if (disposed) {
				return;
			}

			disposed = true;
			sentry.Dispose();
		}


		private static void ApplyContext(Scope scope, TelemetryContext context) {
			if (context == null) {
				return;
			}

			foreach (var tag in context.Tags) {
				if (TelemetrySanitizer.IsSensitiveKey(tag.Key)) {
					continue;
				}

				scope.SetTag(TelemetrySanitizer.SanitizeKey(tag.Key), TelemetrySanitizer.SanitizeValue(tag.Value));
			}

			foreach (var metric in context.Metrics) {
				if (TelemetrySanitizer.IsSensitiveKey(metric.Key)) {
					continue;
				}

				scope.SetExtra(TelemetrySanitizer.SanitizeKey(metric.Key), metric.Value);
			}
		}


		private static double? ClampSampleRate(double? sampleRate) {
			if (!sampleRate.HasValue) {
				return null;
			}

			return Math.Max(0.0, Math.Min(1.0, sampleRate.Value));
		}


		private static SentryLevel ToSentryLevel(TelemetrySeverity severity) {
			switch (severity) {
				case TelemetrySeverity.Debug: return SentryLevel.Debug;

				case TelemetrySeverity.Warning: return SentryLevel.Warning;

				case TelemetrySeverity.Error: return SentryLevel.Error;

				case TelemetrySeverity.Fatal: return SentryLevel.Fatal;

				default: return SentryLevel.Info;
			}
		}


		private static string GetPluginVersion() {
			return typeof(SentryTelemetryService).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
		}


		private static string GetRhinoVersion() {
			try {
				return Rhino.RhinoApp.Version.ToString();
			}
			catch {
				return "unknown";
			}
		}


		private static string ResolveAdapterName() {
			string mode = Environment.GetEnvironmentVariable("TRACKER_NATNET_ADAPTER") ?? string.Empty;
			if (string.Equals(mode, "latest", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(mode, "natnetlatest", StringComparison.OrdinalIgnoreCase)) {
				return "NatNetLatestAdapter";
			}

			return "NatNet4Adapter";
		}

	}

}
