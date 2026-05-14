using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace OptiTrack.Telemetry {

	public static class TelemetrySanitizer {

		private static readonly Regex IPv4Pattern        = new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
		private static readonly Regex WindowsPathPattern = new Regex(@"[A-Za-z]:\\[^\s]+", RegexOptions.Compiled);
		private static readonly Regex EmailPattern       = new Regex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static readonly HashSet<string> SensitiveKeyParts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
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
			sanitized = WindowsPathPattern.Replace(sanitized, "[redacted-path]");

			return sanitized;
		}


		public static bool IsSensitiveKey(string key) {
			if (string.IsNullOrWhiteSpace(key)) {
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
