using System;
using System.Threading;
using System.Threading.Tasks;

using OptiTrack.Core;


namespace OptiTrack.Recording {

	public sealed class OptiTrackReplayClient : IOptiTrackClient {

		private readonly object                  sync = new object();
		private          CancellationTokenSource playbackCancellation;
		private          Task                    playbackTask = Task.CompletedTask;
		private          int                     currentIndex;
		private          bool                    pauseRequested;

		public bool LoopPlayback { get; set; }

		public double PlaybackSpeed { get; set; } = 1.0;

		public bool IsConnected { get; private set; }

		public int DroppedOrLateFrames { get; private set; }

		public OptiTrackRecording Recording { get; private set; }

		public OptiTrackConnectionInfo ConnectionInfo { get; private set; } = new OptiTrackConnectionInfo {
				Status = OptiTrackConnectionStatus.Disconnected, AdapterName = "ReplayClient", Message = "Replay client disconnected."
		};

		public event EventHandler<OptiTrackFrameEventArgs> FrameReceived;

		public event EventHandler<OptiTrackConnectionEventArgs> ConnectionChanged;


		public void LoadRecording(OptiTrackRecording recording) {
			if (recording == null) {
				throw new ArgumentNullException(nameof(recording));
			}

			Recording           = recording;
			currentIndex        = 0;
			DroppedOrLateFrames = 0;
		}


		public void Pause() {
			pauseRequested = true;
		}


		public void Resume() {
			pauseRequested = false;
		}


		public void SetFrameIndex(int index) {
			if (Recording == null || Recording.Frames.Count == 0) {
				return;
			}

			int bounded = index < 0 ? 0 : index;

			if (bounded >= Recording.Frames.Count) {
				bounded = Recording.Frames.Count - 1;
			}

			lock (sync) {
				currentIndex = bounded;
			}
		}


		public Task ConnectAsync(OptiTrackConnectionOptions options, CancellationToken cancellationToken) {
			if (Recording == null || Recording.Frames.Count == 0) {
				throw new InvalidOperationException("Replay recording has not been loaded or has zero frames.");
			}

			if (IsConnected) {
				return Task.CompletedTask;
			}

			IsConnected = true;

			ConnectionInfo = new OptiTrackConnectionInfo {
					Status = OptiTrackConnectionStatus.Connected, AdapterName = "ReplayClient", Message = "Replay connected."
			};

			RaiseConnectionChanged("Replay connected.");

			playbackCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			playbackTask         = Task.Run(() => PlaybackLoopAsync(playbackCancellation.Token), playbackCancellation.Token);

			return Task.CompletedTask;
		}


		public async Task DisconnectAsync() {
			if (!IsConnected) {
				return;
			}

			IsConnected = false;

			if (playbackCancellation != null) {
				playbackCancellation.Cancel();
			}

			try {
				await playbackTask.ConfigureAwait(false);
			}
			catch (OperationCanceledException) { }

			ConnectionInfo = new OptiTrackConnectionInfo {
					Status = OptiTrackConnectionStatus.Disconnected, AdapterName = "ReplayClient", Message = "Replay disconnected."
			};

			RaiseConnectionChanged("Replay disconnected.");
		}


		private async Task PlaybackLoopAsync(CancellationToken cancellationToken) {
			while (!cancellationToken.IsCancellationRequested && Recording != null && Recording.Frames.Count > 0) {
				if (pauseRequested) {
					await Task.Delay(20, cancellationToken).ConfigureAwait(false);

					continue;
				}

				int frameIndex;

				lock (sync) {
					frameIndex = currentIndex;
				}

				if (frameIndex >= Recording.Frames.Count) {
					if (LoopPlayback) {
						lock (sync) {
							currentIndex = 0;
						}

						continue;
					}

					await DisconnectAsync().ConfigureAwait(false);

					return;
				}

				OptiTrackFrame frame = Recording.Frames[frameIndex];
				FrameReceived?.Invoke(this, new OptiTrackFrameEventArgs(OptiTrackFrameSnapshot.Clone(frame)));

				int    nextIndex    = frameIndex + 1;
				double delaySeconds = 1.0 / 60.0;

				if (nextIndex < Recording.Frames.Count) {
					double delta = Recording.Frames[nextIndex].TimestampSeconds - frame.TimestampSeconds;

					if (delta > 0) {
						delaySeconds = delta;
					}
				}

				double   speed       = PlaybackSpeed <= 0.0001 ? 1.0 : PlaybackSpeed;
				int      delayMs     = (int)Math.Max(1, delaySeconds * 1000.0 / speed);
				DateTime waitStarted = DateTime.UtcNow;
				await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
				double actualDelayMs = DateTime.UtcNow.Subtract(waitStarted).TotalMilliseconds;

				if (actualDelayMs > delayMs * 1.5) {
					DroppedOrLateFrames++;
				}

				lock (sync) {
					currentIndex = nextIndex;
				}
			}
		}


		private void RaiseConnectionChanged(string message) {
			ConnectionChanged?.Invoke(this, new OptiTrackConnectionEventArgs(ConnectionInfo, message));
		}

	}

}
