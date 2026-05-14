using System;
using System.Globalization;
using System.IO;
using System.Reflection;

using Newtonsoft.Json.Linq;


namespace OptiTrack.Telemetry {

	public sealed class SentryTelemetryOptions {

		public string Dsn { get; set; } = string.Empty;

		public string Environment { get; set; } = string.Empty;

		public string Release { get; set; } = BuildDefaultReleaseName();

		public double? TracesSampleRate { get; set; } = 0.0;


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


		public bool HasValidDsn() {
			return !string.IsNullOrWhiteSpace(Dsn) && Uri.TryCreate(Dsn, UriKind.Absolute, out _);
		}


		private static string BuildDefaultReleaseName() {
			string version = typeof(SentryTelemetryOptions).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "1.11.0";
			return "optitrack-gh@" + version;
		}

	}

}
