/*
 * File: SentryTelemetryService.cs
 * Purpose: Optional Sentry-backed telemetry implementation.
 * Scope: Telemetry
 * Notes: Must never block plugin operation. Initialization/capture failures downgrade to local no-op behavior.
 */

using System;
using System.Reflection;

using Sentry;


namespace OptiTrack.Telemetry {

	/// <summary>
	/// Optional <see cref="ITelemetryService"/> implementation backed by Sentry SDK.
	/// </summary>
	/// <remarks>
	/// Telemetry is only active when explicitly enabled and a valid DSN is provided.
	/// This implementation enforces redaction and low-cardinality tags; raw OptiTrack frame content must never be emitted.
	/// </remarks>
	public sealed class SentryTelemetryService : ITelemetryService, IDisposable {

		private readonly IDisposable sentry;
		private          bool        disposed;


		private SentryTelemetryService(IDisposable sentry, string status) {
			this.sentry = sentry;
			Status      = status;
		}


		/// <summary>
		/// Gets telemetry backend status for diagnostics.
		/// </summary>
		public string Status { get; private set; }


		/// <summary>
		/// Creates either an active Sentry-backed service or a no-op service.
		/// </summary>
		/// <param name="enabled">Whether telemetry is enabled by user input.</param>
		/// <returns>Telemetry service implementation.</returns>
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
																				  // Drop server/request envelopes to avoid accidental host/request metadata leakage.
																				  evt.ServerName = null;
																				  evt.Request    = null;

																				  return evt;
																			  });
													});

				SentrySdk.SetTag("plugin_version", GetPluginVersion());
				SentrySdk.SetTag("adapter_name", ResolveAdapterName());
				SentrySdk.SetTag("rhino_major_version", GetRhinoMajorVersion());
				SentrySdk.SetTag("sentry_sdk_version", typeof(SentrySdk).Assembly.GetName().Version?.ToString() ?? "unknown");

				return new SentryTelemetryService(sentry, "active");
			}
			catch {
				// Telemetry initialization must not crash plugin startup paths.
				return new NoOpTelemetryService("failed: Sentry configuration could not be initialized");
			}
		}


		/// <summary>
		/// Captures a sanitized exception classification.
		/// </summary>
		/// <param name="exception">Exception to capture.</param>
		/// <param name="context">Optional telemetry context.</param>
		public void CaptureException(Exception exception, TelemetryContext context) {
			if (exception == null) {
				return;
			}

			try {
				// Intentionally avoid forwarding exception message text because it may contain user-specific names or file identifiers.
				Exception sanitizedException = new Exception(TelemetrySanitizer.SanitizeValue(exception.GetType().Name));

				SentrySdk.CaptureException(
						sanitizedException,
						scope => {
							ApplyContext(scope, context);
							scope.SetTag("exception_type", TelemetrySanitizer.SanitizeValue(exception.GetType().Name));
						});
			}
			catch {
				// Capture failures are intentionally non-fatal.
				Status = "failed: Sentry capture failed";
			}
		}


		/// <summary>
		/// Captures a sanitized message event.
		/// </summary>
		/// <param name="message">Message/operation name.</param>
		/// <param name="severity">Message severity.</param>
		/// <param name="context">Optional telemetry context.</param>
		public void CaptureMessage(string message, TelemetrySeverity severity, TelemetryContext context) {
			try {
				SentrySdk.CaptureMessage(TelemetrySanitizer.SanitizeValue(message), scope => ApplyContext(scope, context), ToSentryLevel(severity));
			}
			catch {
				// Capture failures are intentionally non-fatal.
				Status = "failed: Sentry capture failed";
			}
		}


		/// <summary>
		/// Starts a timing scope represented as a low-cardinality message/span.
		/// </summary>
		/// <param name="operationName">Operation identifier.</param>
		/// <param name="context">Optional telemetry context.</param>
		/// <returns>Disposable telemetry scope.</returns>
		public TelemetryScope StartSpan(string operationName, TelemetryContext context) {
			return new TelemetryScope(this, operationName, context);
		}


		/// <summary>
		/// Disposes the underlying Sentry SDK handle.
		/// </summary>
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


		private static string GetRhinoMajorVersion() {
			try {
				string version = Rhino.RhinoApp.Version.ToString();
				int    split   = version.IndexOf('.');
				return split > 0 ? version.Substring(0, split) : version;
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
