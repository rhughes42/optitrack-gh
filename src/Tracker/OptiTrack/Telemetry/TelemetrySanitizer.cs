using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OptiTrack.Telemetry {

	/// <summary>
	/// Provides helpers for sanitizing telemetry keys and values.
	/// </summary>
	public static class TelemetrySanitizer {
		private static readonly Regex IPv4Pattern = new Regex( @"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled );
		private static readonly Regex WindowsPathPattern = new Regex( @"[A-Za-z]:\\[^\s]+", RegexOptions.Compiled );
		private static readonly Regex EmailPattern = new Regex( @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase );

		private static readonly HashSet<string> SensitiveKeyParts = new HashSet<string>( StringComparer.OrdinalIgnoreCase ) {
			"address",
			"body",
			"capture",
			"file",
			"host",
			"ip",
			"machine",
			"marker",
			"model",
			"name",
			"path",
			"position",
			"project",
			"rigidbody",
			"token",
			"user"
		};

		/// <summary>
		/// Normalizes a telemetry key to a safe lowercase format.
		/// </summary>
		/// <param name="key">The source key.</param>
		/// <returns>A normalized key value.</returns>
		public static string SanitizeKey( string key ) {
			if ( string.IsNullOrWhiteSpace( key ) ) {
				return "unknown";
			}

			return key.Trim().Replace( " ", "_" ).ToLowerInvariant();
		}

		/// <summary>
		/// Redacts sensitive value patterns such as IP addresses, email addresses, and Windows paths.
		/// </summary>
		/// <param name="value">The source value.</param>
		/// <returns>A sanitized value suitable for telemetry payloads.</returns>
		public static string SanitizeValue( string value ) {
			if ( string.IsNullOrEmpty( value ) ) {
				return string.Empty;
			}

			string sanitized = EmailPattern.Replace( value, "[redacted-email]" );
			sanitized = IPv4Pattern.Replace( sanitized, "[redacted-ip]" );
			sanitized = WindowsPathPattern.Replace( sanitized, "[redacted-path]" );
			return sanitized;
		}

		/// <summary>
		/// Determines whether a key appears sensitive and should be redacted.
		/// </summary>
		/// <param name="key">The key to evaluate.</param>
		/// <returns><c>true</c> when the key appears sensitive; otherwise, <c>false</c>.</returns>
		public static bool IsSensitiveKey( string key ) {
			if ( string.IsNullOrWhiteSpace( key ) ) {
				return false;
			}

			foreach ( string part in SensitiveKeyParts ) {
				if ( key.IndexOf( part, StringComparison.OrdinalIgnoreCase ) >= 0 ) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Sanitizes a tag value and fully redacts values for sensitive keys.
		/// </summary>
		/// <param name="key">Tag key.</param>
		/// <param name="value">Tag value.</param>
		/// <returns>A sanitized or redacted tag value.</returns>
		public static string SanitizeTagValue( string key, string value ) {
			if ( IsSensitiveKey( key ) ) {
				return "[redacted]";
			}

			return SanitizeValue( value );
		}
	}
}
