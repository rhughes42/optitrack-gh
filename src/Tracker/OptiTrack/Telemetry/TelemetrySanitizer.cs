/*
 * File: TelemetrySanitizer.cs
 * Purpose: Central redaction rules for telemetry keys and values.
 * Scope: Telemetry
 * Notes: This boundary protects against accidental leakage of addresses, paths, and capture-identifying content.
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace OptiTrack.Telemetry {

	/// <summary>
	/// Sanitization helpers used by telemetry context and Sentry adapters.
	/// </summary>
	/// <remarks>
	/// Keep this file conservative. If uncertain whether data is safe, prefer redacting.
	/// </remarks>
	public static class TelemetrySanitizer {

		static readonly Regex IPv4Pattern        = new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
		static readonly Regex IPv6Pattern        = new Regex(@"\b(?:[A-F0-9]{1,4}:){2,7}[A-F0-9]{1,4}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static readonly Regex WindowsPathPattern = new Regex(@"[A-Za-z]:\\[^\s]+", RegexOptions.Compiled);
		static readonly Regex EmailPattern       = new Regex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		static readonly HashSet<string> SensitiveKeyParts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
				"address",
				"body",
				"capture",
				"file",
				"host",
				"ip",
				"machine",
				"marker",
				"model",
				"path",
				"position",
				"project",
				"rigidbody",
				"token",
				"user",
				"username",
				"machine_name",
				"document_name",
				"rigid_body_name",
				"model_name",
				"project_name"
		};
		// Explicitly allowed low-cardinality keys that remain safe even if they contain sensitive substrings (for example marker_count).
		static readonly HashSet<string> SafeKeyAllowList = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
				"adapter_name",
				"adapter_version",
				"buffer_age_ms",
				"component",
				"connection_mode",
				"connection_type",
				"conversion_duration_ms",
				"dropped_frame_count",
				"duration_ms",
				"error_type",
				"exception_type",
				"format",
				"format_version",
				"frame_count",
				"frame_schema_version",
				"grasshopper_version",
				"loaded_natnet_assembly",
				"marker_count",
				"natnet_assembly_version",
				"operation",
				"plugin_version",
				"reconnect_count",
				"rhino_major_version",
				"rigid_body_count",
				"sdk_exception_type",
				"sdk_load_result",
				"sentry_sdk_version",
				"skipped_frame_count",
				"solve_duration_ms",
				"supported_sdk_version",
				"update_interval_ms"
		};


		public static string SanitizeKey(string key) {
			if (string.IsNullOrWhiteSpace(key)) {
				return "unknown";
			}

			return key.Trim().Replace(" ", "_").ToLowerInvariant();
		}


		public static string SanitizeValue(string value) {
			if (string.IsNullOrEmpty(value)) {
				return string.Empty;
			}

			string sanitized = EmailPattern.Replace(value, "[redacted-email]");
			sanitized = IPv4Pattern.Replace(sanitized, "[redacted-ip]");
			sanitized = IPv6Pattern.Replace(sanitized, "[redacted-ip]");
			sanitized = WindowsPathPattern.Replace(sanitized, "[redacted-path]");

			return sanitized;
		}


		public static bool IsSensitiveKey(string key) {
			if (string.IsNullOrWhiteSpace(key)) {
				return false;
			}

			string normalizedKey = SanitizeKey(key);
			if (SafeKeyAllowList.Contains(normalizedKey)) {
				return false;
			}

			foreach (string part in SensitiveKeyParts) {
				if (key.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0) {
					return true;
				}
			}

			return false;
		}


		public static string SanitizeTagValue(string key, string value) {
			if (IsSensitiveKey(key)) {
				return "[redacted]";
			}

			return SanitizeValue(value);
		}

	}

}
