using System;
using System.Collections.Generic;


namespace OptiTrack.Core {

	public sealed class TrackerLogger {

		private readonly object       sync = new object();
		private readonly List<string> entries = new List<string>();

		public bool DebugEnabled { get; set; }


		public void Info(string message) {
			Add("INFO", message);
		}


		public void Warn(string message) {
			Add("WARN", message);
		}


		public void Debug(string message) {
			if (!DebugEnabled) {
				return;
			}

			Add("DEBUG", message);
		}


		public List<string> Snapshot() {
			lock (sync) {
				return new List<string>(entries);
			}
		}


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
