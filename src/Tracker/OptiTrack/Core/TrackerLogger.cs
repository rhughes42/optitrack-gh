/*
 * File: TrackerLogger.cs
 * Purpose: Lightweight in-memory diagnostic logger for component status output.
 * Scope: Core
 * Notes: Not a persistent logging backend; entries are intended for short-lived Grasshopper UI diagnostics.
 */

using System;
using System.Collections.Generic;


namespace OptiTrack.Core {

	/// <summary>
	/// Timestamped logger with optional debug-level emission.
	/// </summary>
	public sealed class TrackerLogger {

		private readonly object       sync    = new object();
		private readonly List<string> entries = new List<string>();

		/// <summary>
		/// Gets or sets whether debug messages should be emitted.
		/// </summary>
		public bool DebugEnabled { get; set; }


		/// <summary>
		/// Adds an informational log line.
		/// </summary>
		/// <param name="message">Message text.</param>
		public void Info(string message) {
			Add("INFO", message);
		}


		/// <summary>
		/// Adds a warning log line.
		/// </summary>
		/// <param name="message">Message text.</param>
		public void Warn(string message) {
			Add("WARN", message);
		}


		/// <summary>
		/// Adds a debug log line when <see cref="DebugEnabled"/> is true.
		/// </summary>
		/// <param name="message">Message text.</param>
		public void Debug(string message) {
			if (!DebugEnabled) {
				return;
			}

			Add("DEBUG", message);
		}


		/// <summary>
		/// Returns a thread-safe snapshot of current log entries.
		/// </summary>
		/// <returns>Copy of in-memory entries.</returns>
		public List<string> Snapshot() {
			lock (sync) {
				return new List<string>(entries);
			}
		}


		/// <summary>
		/// Clears all in-memory log entries.
		/// </summary>
		public void Clear() {
			lock (sync) {
				entries.Clear();
			}
		}


		private void Add(string level, string message) {
			if (string.IsNullOrWhiteSpace(message)) {
				return;
			}

			lock (sync) {
				entries.Add(DateTime.UtcNow.ToString("HH:mm:ss") + " [" + level + "] " + message.Trim());
			}
		}

	}

}
