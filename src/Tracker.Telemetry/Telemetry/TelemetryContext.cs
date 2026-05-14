/*
 * File: TelemetryContext.cs
 * Purpose: Sanitized telemetry tag/metric carrier for spans and events.
 * Scope: Telemetry
 * Notes: Keys/values are sanitized immediately on insertion to reduce accidental unsafe emissions.
 */

using System.Collections.Generic;


namespace OptiTrack.Telemetry {

	/// <summary>
	/// Mutable telemetry context containing tags and numeric metrics.
	/// </summary>
	public sealed class TelemetryContext {

		/// <summary>
		/// Initializes empty tag and metric stores.
		/// </summary>
		public TelemetryContext() {
			Tags    = new Dictionary<string, string>();
			Metrics = new Dictionary<string, double>();
		}


		/// <summary>
		/// Gets sanitized telemetry tags.
		/// </summary>
		public IDictionary<string, string> Tags { get; private set; }

		/// <summary>
		/// Gets numeric telemetry metrics.
		/// </summary>
		public IDictionary<string, double> Metrics { get; private set; }


		/// <summary>
		/// Adds or replaces a sanitized tag.
		/// </summary>
		/// <param name="key">Tag key.</param>
		/// <param name="value">Tag value.</param>
		/// <returns>Current context instance.</returns>
		public TelemetryContext SetTag(string key, string value) {
			Tags[TelemetrySanitizer.SanitizeKey(key)] = TelemetrySanitizer.SanitizeTagValue(key, value);

			return this;
		}


		/// <summary>
		/// Adds or replaces a numeric metric.
		/// </summary>
		/// <param name="key">Metric key.</param>
		/// <param name="value">Metric value.</param>
		/// <returns>Current context instance.</returns>
		public TelemetryContext SetMetric(string key, double value) {
			Metrics[TelemetrySanitizer.SanitizeKey(key)] = value;

			return this;
		}

	}

}
