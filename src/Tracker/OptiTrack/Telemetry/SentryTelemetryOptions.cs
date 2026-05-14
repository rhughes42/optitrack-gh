/*
 * File: SentryTelemetryOptions.cs
 * Purpose: Loads optional Sentry runtime configuration from environment and local override file.
 * Scope: Telemetry
 * Notes: No secrets are stored in source control; DSN/release/environment come from user environment or local ignored file.
 */

using System;
using System.Globalization;
using System.IO;
using System.Reflection;

using Newtonsoft.Json.Linq;


namespace OptiTrack.Telemetry {

	/// <summary>
	/// Runtime configuration values for optional Sentry telemetry.
	/// </summary>
	public sealed class SentryTelemetryOptions {

		/// <summary>
		/// Gets or sets the Sentry DSN used when telemetry is explicitly enabled.
		/// </summary>
		public string Dsn { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets telemetry environment name.
		/// </summary>
		public string Environment { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets release identifier.
		/// </summary>
		public string Release { get; set; } = BuildDefaultReleaseName();

		/// <summary>
		/// Gets or sets traces sample rate in [0, 1].
		/// </summary>
		public double? TracesSampleRate { get; set; } = 0.0;


		/// <summary>
		/// Loads telemetry settings from environment variables and local override file.
		/// </summary>
		/// <returns>Telemetry options instance.</returns>
		public static SentryTelemetryOptions Load() {
			SentryTelemetryOptions options = new SentryTelemetryOptions {
					Dsn         = System.Environment.GetEnvironmentVariable("SENTRY_DSN") ?? string.Empty,
					Environment = System.Environment.GetEnvironmentVariable("SENTRY_ENVIRONMENT") ?? string.Empty,
					Release     = System.Environment.GetEnvironmentVariable("SENTRY_RELEASE") ?? BuildDefaultReleaseName()
			};

			string tracesSampleRate = System.Environment.GetEnvironmentVariable("SENTRY_TRACES_SAMPLE_RATE");

			if (double.TryParse(tracesSampleRate, NumberStyles.Float, CultureInfo.InvariantCulture, out double sampleRate)) {
				options.TracesSampleRate = sampleRate;
			}

			LoadLocalFile(options);

			return options;
		}


		private static void LoadLocalFile(SentryTelemetryOptions options) {
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tracker.telemetry.local.json");

			if (!File.Exists(path)) {
				return;
			}

			JObject json = JObject.Parse(File.ReadAllText(path));
			options.Dsn         = (string)json["SENTRY_DSN"] ?? options.Dsn;
			options.Environment = (string)json["SENTRY_ENVIRONMENT"] ?? options.Environment;
			options.Release     = (string)json["SENTRY_RELEASE"] ?? options.Release;

			JToken tracesSampleRate = json["SENTRY_TRACES_SAMPLE_RATE"];

			if (tracesSampleRate != null
				&& double.TryParse(tracesSampleRate.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double sampleRate)) {
				options.TracesSampleRate = sampleRate;
			}
		}


		/// <summary>
		/// Determines whether the current DSN value appears usable.
		/// </summary>
		/// <returns>True when a non-empty absolute URI DSN is available.</returns>
		public bool HasValidDsn() {
			return !string.IsNullOrWhiteSpace(Dsn) && Uri.TryCreate(Dsn, UriKind.Absolute, out _);
		}


		private static string BuildDefaultReleaseName() {
			string version = typeof(SentryTelemetryOptions).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.11.0";
			return "optitrack-gh@" + version;
		}

	}

}
