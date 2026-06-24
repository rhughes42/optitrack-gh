using System;
using System.Reflection;

using Sentry;

namespace OptiTrack.Telemetry {

	/// <summary>
	/// Sentry-backed telemetry implementation with built-in data sanitization.
	/// </summary>
	public sealed class SentryTelemetryService : ITelemetryService, IDisposable {
		private readonly IDisposable sentry;
		private bool disposed;

		private SentryTelemetryService( IDisposable sentry, string status ) {
			this.sentry = sentry;
			Status = status;
		}

		/// <summary>
		/// Gets the current service status string.
		/// </summary>
		public string Status { get; private set; }

		/// <summary>
		/// Creates a telemetry service instance based on runtime configuration.
		/// </summary>
		/// <param name="enabled">Whether telemetry was enabled by user intent.</param>
		/// <returns>A configured Sentry service or a no-op fallback.</returns>
		public static ITelemetryService Create( bool enabled ) {
			if ( !enabled ) {
				return new NoOpTelemetryService( "disabled" );
			}

			try {
				SentryTelemetryOptions configuration = SentryTelemetryOptions.Load();
				if ( !configuration.HasValidDsn() ) {
					return new NoOpTelemetryService( "disabled: Sentry DSN missing or invalid" );
				}

				IDisposable sentry = SentrySdk.Init( options => {
					options.Dsn = configuration.Dsn;
					options.Environment = string.IsNullOrWhiteSpace( configuration.Environment ) ? "local" : TelemetrySanitizer.SanitizeValue( configuration.Environment );
					options.Release = TelemetrySanitizer.SanitizeValue( configuration.Release );
					options.SendDefaultPii = false;
					options.AttachStacktrace = true;
					options.TracesSampleRate = ClampSampleRate( configuration.TracesSampleRate );
					options.SetBeforeSend( evt => {
						evt.ServerName = null;
						evt.Request = null;
						return evt;
					} );
				} );

				SentrySdk.SetTag( "plugin_version", GetPluginVersion() );
				SentrySdk.SetTag( "adapter_name", "NatNet" );
				SentrySdk.SetTag( "rhino_version", GetRhinoVersion() );

				return new SentryTelemetryService( sentry, "active" );
			} catch {
				return new NoOpTelemetryService( "failed: Sentry configuration could not be initialized" );
			}
		}

		/// <summary>
		/// Captures a sanitized exception event in Sentry.
		/// </summary>
		/// <param name="exception">The exception to capture.</param>
		/// <param name="context">Additional event tags and metrics.</param>
		public void CaptureException( Exception exception, TelemetryContext context ) {
			if ( exception == null ) {
				return;
			}

			try {
				Exception sanitizedException = new Exception( TelemetrySanitizer.SanitizeValue( exception.GetType().Name + ": " + exception.Message ) );
				SentrySdk.CaptureException( sanitizedException, scope => {
					ApplyContext( scope, context );
					scope.SetTag( "exception_type", TelemetrySanitizer.SanitizeValue( exception.GetType().Name ) );
				} );
			} catch {
				Status = "failed: Sentry capture failed";
			}
		}

		/// <summary>
		/// Captures a sanitized message event in Sentry.
		/// </summary>
		/// <param name="message">The message text.</param>
		/// <param name="severity">The message severity level.</param>
		/// <param name="context">Additional event tags and metrics.</param>
		public void CaptureMessage( string message, TelemetrySeverity severity, TelemetryContext context ) {
			try {
				SentrySdk.CaptureMessage( TelemetrySanitizer.SanitizeValue( message ), scope => ApplyContext( scope, context ), ToSentryLevel( severity ) );
			} catch {
				Status = "failed: Sentry capture failed";
			}
		}

		/// <summary>
		/// Starts a timed telemetry span.
		/// </summary>
		/// <param name="operationName">The operation name.</param>
		/// <param name="context">Initial context associated with the span.</param>
		/// <returns>A telemetry scope that publishes duration when disposed.</returns>
		public TelemetryScope StartSpan( string operationName, TelemetryContext context ) {
			return new TelemetryScope( this, operationName, context );
		}

		/// <summary>
		/// Disposes the Sentry SDK session.
		/// </summary>
		public void Dispose() {
			if ( disposed ) {
				return;
			}

			disposed = true;
			sentry.Dispose();
		}

		private static void ApplyContext( Scope scope, TelemetryContext context ) {
			if ( context == null ) {
				return;
			}

			foreach ( var tag in context.Tags ) {
				if ( TelemetrySanitizer.IsSensitiveKey( tag.Key ) ) {
					continue;
				}

				scope.SetTag( TelemetrySanitizer.SanitizeKey( tag.Key ), TelemetrySanitizer.SanitizeValue( tag.Value ) );
			}

			foreach ( var metric in context.Metrics ) {
				if ( TelemetrySanitizer.IsSensitiveKey( metric.Key ) ) {
					continue;
				}

				scope.SetExtra( TelemetrySanitizer.SanitizeKey( metric.Key ), metric.Value );
			}
		}

		private static double? ClampSampleRate( double? sampleRate ) {
			if ( !sampleRate.HasValue ) {
				return null;
			}

			return Math.Max( 0.0, Math.Min( 1.0, sampleRate.Value ) );
		}

		private static SentryLevel ToSentryLevel( TelemetrySeverity severity ) {
			switch ( severity ) {
				case TelemetrySeverity.Debug:
				return SentryLevel.Debug;
				case TelemetrySeverity.Warning:
				return SentryLevel.Warning;
				case TelemetrySeverity.Error:
				return SentryLevel.Error;
				case TelemetrySeverity.Fatal:
				return SentryLevel.Fatal;
				default:
				return SentryLevel.Info;
			}
		}

		private static string GetPluginVersion() {
			return typeof( SentryTelemetryService ).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
		}

		private static string GetRhinoVersion() {
			try {
				return Rhino.RhinoApp.Version.ToString();
			} catch {
				return "unknown";
			}
		}
	}
}
