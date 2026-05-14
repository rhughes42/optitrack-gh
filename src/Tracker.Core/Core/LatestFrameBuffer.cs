/*
 * File: LatestFrameBuffer.cs
 * Purpose: Thread-safe single-slot frame buffer used to decouple NatNet callback rate from GH solve cadence.
 * Scope: Core
 * Notes: Designed for overwrite semantics (latest frame wins) to avoid unbounded queue growth.
 */


using System;
using System.Threading;


namespace OptiTrack.Core {

	/// <summary>
	/// Single-item latest-frame buffer with receive/consume counters.
	/// </summary>
	/// <remarks>
	/// Writes are expected from NatNet callback threads; consumption is expected from Grasshopper solves.
	/// </remarks>
	public sealed class LatestFrameBuffer {

		readonly object sync = new object();
		OptiTrackFrame? latestFrame;
		DateTime        latestReceivedUtc = DateTime.MinValue;
		long            latestSequence;
		long            lastConsumedSequence;
		long            totalReceived;
		long            totalConsumed;


		/// <summary>
		/// Stores the latest frame and updates receive counters.
		/// </summary>
		/// <param name="frame">Frame instance received from the source callback.</param>
		/// <param name="receivedUtc">UTC timestamp captured when frame arrived.</param>
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


		/// <summary>
		/// Retrieves the most recently written frame without queueing historic frames.
		/// </summary>
		/// <param name="frame">Latest frame when available.</param>
		/// <param name="receivedUtc">Timestamp when latest frame was written.</param>
		/// <param name="skippedIntermediateFrames">True when one or more writes occurred since last consume.</param>
		/// <returns>True when a frame is available; otherwise false.</returns>
		public bool TryConsumeLatest(out OptiTrackFrame frame, out DateTime receivedUtc, out bool skippedIntermediateFrames) {
			lock (sync) {
				frame       = latestFrame!;
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


		/// <summary>
		/// Gets total number of accepted frame writes.
		/// </summary>
		public long TotalReceived {
			get { return Interlocked.Read(ref totalReceived); }
		}

		/// <summary>
		/// Gets total number of successful consumes.
		/// </summary>
		public long TotalConsumed {
			get { return Interlocked.Read(ref totalConsumed); }
		}

		/// <summary>
		/// Gets a derived skip count (received minus consumed).
		/// </summary>
		public long SkippedCount {
			get {
				long skipped = TotalReceived - TotalConsumed;

				return skipped < 0 ? 0 : skipped;
			}
		}

	}

}
