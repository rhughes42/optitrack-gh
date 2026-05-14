using System.Collections.Generic;
using System.Linq;

using OptiTrack.Core;


namespace OptiTrack.Recording {

	internal static class OptiTrackFrameSnapshot {

		public static OptiTrackFrame Clone(OptiTrackFrame frame) {
			if (frame == null) {
				return null;
			}

			return new OptiTrackFrame {
					FrameNumber      = frame.FrameNumber,
					TimestampSeconds = frame.TimestampSeconds,
					LatencySeconds   = frame.LatencySeconds,
					IsRecording      = frame.IsRecording,
					AssetsChanged    = frame.AssetsChanged,
					Markers
							= frame.Markers.Select(marker => new OptiTrackMarker {
									Id = marker.Id, Label = marker.Label, X = marker.X, Y = marker.Y, Z = marker.Z
							}).ToList(),
					RigidBodies = frame.RigidBodies.Select(rigidBody => new OptiTrackRigidBody {
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
					Skeletons      = frame.Skeletons.Select(skeleton => new OptiTrackSkeleton { Id = skeleton.Id, Name = skeleton.Name }).ToList(),
					StatusMessages = new List<string>(frame.StatusMessages)
			};
		}

	}

}
