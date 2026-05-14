using System.Collections.Generic;


namespace OptiTrack.Core {

	public sealed class OptiTrackMarker {

		public int Id { get; set; }

		public string Label { get; set; } = string.Empty;

		public double X { get; set; }

		public double Y { get; set; }

		public double Z { get; set; }

	}


	public sealed class OptiTrackRigidBody {

		public int Id { get; set; }

		public string Name { get; set; } = string.Empty;

		public bool IsTracked { get; set; }

		public double X { get; set; }

		public double Y { get; set; }

		public double Z { get; set; }

		public double Qx { get; set; }

		public double Qy { get; set; }

		public double Qz { get; set; }

		public double Qw { get; set; }

	}


	public sealed class OptiTrackSkeleton {

		public int Id { get; set; }

		public string Name { get; set; } = string.Empty;

	}


	public sealed class OptiTrackFrame {

		public OptiTrackFrame() {
			Markers        = new List<OptiTrackMarker>();
			RigidBodies    = new List<OptiTrackRigidBody>();
			Skeletons      = new List<OptiTrackSkeleton>();
			StatusMessages = new List<string>();
		}


		public int FrameNumber { get; set; }

		public double TimestampSeconds { get; set; }

		public double LatencySeconds { get; set; }

		public bool IsRecording { get; set; }

		public bool AssetsChanged { get; set; }

		public IReadOnlyList<OptiTrackMarker> Markers { get; set; }

		public IReadOnlyList<OptiTrackRigidBody> RigidBodies { get; set; }

		public IReadOnlyList<OptiTrackSkeleton> Skeletons { get; set; }

		public IReadOnlyList<string> StatusMessages { get; set; }

	}

}
