using System;
using System.Threading;


namespace OptiTrack.Core {

	public sealed class LatestFrameBuffer {

		private readonly object sync = new object();
		private          OptiTrackFrame latestFrame;
		private          DateTime       latestReceivedUtc = DateTime.MinValue;
		private          long           latestSequence;
		private          long           lastConsumedSequence;
		private          long           totalReceived;
		private          long           totalConsumed;


		public void Write(OptiTrackFrame frame, DateTime receivedUtc) {
			if (frame == null) {
				return;
			}

			lock (sync) {
				latestFrame       = frame;
				latestReceivedUtc = receivedUtc;
				latestSequence++;
				Interlocked.Increment(ref totalReceived);
			}
		}


		public bool TryConsumeLatest(out OptiTrackFrame frame, out DateTime receivedUtc, out bool skippedIntermediateFrames) {
			lock (sync) {
				frame       = latestFrame;
				receivedUtc = latestReceivedUtc;

				if (frame == null) {
					skippedIntermediateFrames = false;

					return false;
				}

				skippedIntermediateFrames = latestSequence - lastConsumedSequence > 1;
				lastConsumedSequence      = latestSequence;
				Interlocked.Increment(ref totalConsumed);

				return true;
			}
		}


		public long TotalReceived {
			get { return Interlocked.Read(ref totalReceived); }
		}

		public long TotalConsumed {
			get { return Interlocked.Read(ref totalConsumed); }
		}

		public long SkippedCount {
			get {
				long skipped = TotalReceived - TotalConsumed;

				return skipped < 0 ? 0 : skipped;
			}
		}

	}

}
