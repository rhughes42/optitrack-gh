using System;
using System.Globalization;
using System.IO;

using Newtonsoft.Json.Linq;

namespace OptiTrack.Telemetry {

	/// <summary>
	/// Represents configuration for optional Sentry telemetry capture.
	/// </summary>
	public sealed class SentryTelemetryOptions {

		/// <summary>
		/// Gets or sets the Sentry DSN.
		/// </summary>
		public string Dsn { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the Sentry environment label.
		/// </summary>
		public string Environment { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the Sentry release label.
		/// </summary>
		public string Release { get; set; } = "tracker@1.4.0";

		/// <summary>
		/// Gets or sets the optional Sentry trace sample rate.
		/// </summary>
		public double? TracesSampleRate { get; set; }

		/// <summary>
		/// Loads Sentry options from environment variables and optional local JSON override file.
		/// </summary>
		/// <returns>The loaded Sentry telemetry options.</returns>
		public static SentryTelemetryOptions Load() {
			SentryTelemetryOptions options = new SentryTelemetryOptions {
				Dsn = System.Environment.GetEnvironmentVariable( "SENTRY_DSN" ) ?? string.Empty,
				Environment = System.Environment.GetEnvironmentVariable( "SENTRY_ENVIRONMENT" ) ?? string.Empty,
				Release = System.Environment.GetEnvironmentVariable( "SENTRY_RELEASE" ) ?? "tracker@1.4.0"
			};

			string tracesSampleRate = System.Environment.GetEnvironmentVariable( "SENTRY_TRACES_SAMPLE_RATE" );
			if ( double.TryParse( tracesSampleRate, NumberStyles.Float, CultureInfo.InvariantCulture, out double sampleRate ) ) {
				options.TracesSampleRate = sampleRate;
			}

			LoadLocalFile( options );
			return options;
		}

		private static void LoadLocalFile( SentryTelemetryOptions options ) {
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string assemblyLocation = typeof( SentryTelemetryOptions ).Assembly.Location;
			if ( !string.IsNullOrWhiteSpace( assemblyLocation ) ) {
				string assemblyDirectory = Path.GetDirectoryName( assemblyLocation );
				if ( !string.IsNullOrWhiteSpace( assemblyDirectory ) ) {
					baseDirectory = assemblyDirectory;
				}
			}

			string path = Path.Combine( baseDirectory, "tracker.telemetry.local.json" );
			if ( !File.Exists( path ) ) {
				return;
			}

			JObject json = JObject.Parse( File.ReadAllText( path ) );
			options.Dsn = (string) json[ "SENTRY_DSN" ] ?? options.Dsn;
			options.Environment = (string) json[ "SENTRY_ENVIRONMENT" ] ?? options.Environment;
			options.Release = (string) json[ "SENTRY_RELEASE" ] ?? options.Release;

			JToken tracesSampleRate = json[ "SENTRY_TRACES_SAMPLE_RATE" ];
			if ( tracesSampleRate != null && double.TryParse( tracesSampleRate.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double sampleRate ) ) {
				options.TracesSampleRate = sampleRate;
			}
		}

		/// <summary>
		/// Determines whether the configured DSN appears to be valid for telemetry initialization.
		/// </summary>
		/// <returns><c>true</c> when the DSN is non-empty and absolute; otherwise, <c>false</c>.</returns>
		public bool HasValidDsn() {
			return !string.IsNullOrWhiteSpace( Dsn ) && Uri.TryCreate( Dsn, UriKind.Absolute, out _ );
		}
	}
}
