using System.Collections.Generic;
using System.Linq;

using OptiTrack.Core;


namespace OptiTrack.Recording {

	static class OptiTrackFrameSnapshot {

		public static OptiTrackFrame Clone(OptiTrackFrame frame) {
			if (frame == null) {
				return null;
			}

			return new OptiTrackFrame(
					frameNumber: frame.FrameNumber,
					timestampSeconds: frame.TimestampSeconds,
					latencySeconds: frame.LatencySeconds,
					isRecording: frame.IsRecording,
					assetsChanged: frame.AssetsChanged,
					markers: frame.Markers
								  .Select(marker => new OptiTrackMarker { Id = marker.Id, Label = marker.Label, X = marker.X, Y = marker.Y, Z = marker.Z })
								  .ToList(),
					rigidBodies: frame.RigidBodies.Select(rigidBody => new OptiTrackRigidBody {
							Id        = rigidBody.Id,
							Name      = rigidBody.Name,
							IsTracked = rigidBody.IsTracked,
							X         = rigidBody.X,
							Y         = rigidBody.Y,
							Z         = rigidBody.Z,
							Qx        = rigidBody.Qx,
							Qy        = rigidBody.Qy,
							Qz        = rigidBody.Qz,
							Qw        = rigidBody.Qw
					}).ToList(),
					skeletons: frame.Skeletons.Select(skeleton => new OptiTrackSkeleton { Id = skeleton.Id, Name = skeleton.Name }).ToList(),
					statusMessages: new List<string>(frame.StatusMessages));
		}

	}

}
