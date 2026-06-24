using System.Collections.Generic;

namespace OptiTrack.Telemetry {

	/// <summary>
	/// Stores telemetry tags and numeric metrics for a capture event or span.
	/// </summary>
	public sealed class TelemetryContext {

		/// <summary>
		/// Initializes empty tag and metric collections.
		/// </summary>
		public TelemetryContext() {
			Tags = new Dictionary<string, string>();
			Metrics = new Dictionary<string, double>();
		}

		/// <summary>
		/// Gets the mutable tag collection.
		/// </summary>
		public IDictionary<string, string> Tags { get; private set; }

		/// <summary>
		/// Gets the mutable metric collection.
		/// </summary>
		public IDictionary<string, double> Metrics { get; private set; }

		/// <summary>
		/// Adds or replaces a sanitized tag entry.
		/// </summary>
		/// <param name="key">Tag key.</param>
		/// <param name="value">Tag value.</param>
		/// <returns>The current context for fluent chaining.</returns>
		public TelemetryContext SetTag( string key, string value ) {
			Tags[ TelemetrySanitizer.SanitizeKey( key ) ] = TelemetrySanitizer.SanitizeTagValue( key, value );
			return this;
		}

		/// <summary>
		/// Adds or replaces a metric entry.
		/// </summary>
		/// <param name="key">Metric key.</param>
		/// <param name="value">Metric value.</param>
		/// <returns>The current context for fluent chaining.</returns>
		public TelemetryContext SetMetric( string key, double value ) {
			Metrics[ TelemetrySanitizer.SanitizeKey( key ) ] = value;
			return this;
		}
	}
}
