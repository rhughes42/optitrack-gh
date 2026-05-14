using System.Collections.Generic;


namespace OptiTrack.Telemetry {

	public sealed class TelemetryContext {

		public TelemetryContext() {
			Tags    = new Dictionary<string, string>();
			Metrics = new Dictionary<string, double>();
		}


		public IDictionary<string, string> Tags { get; private set; }

		public IDictionary<string, double> Metrics { get; private set; }


		public TelemetryContext SetTag(string key, string value) {
			Tags[TelemetrySanitizer.SanitizeKey(key)] = TelemetrySanitizer.SanitizeTagValue(key, value);

			return this;
		}


		public TelemetryContext SetMetric(string key, double value) {
			Metrics[TelemetrySanitizer.SanitizeKey(key)] = value;

			return this;
		}

	}

}
