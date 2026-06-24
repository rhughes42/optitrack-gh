using System.Collections.Generic;

namespace OptiTrack.Core {

	/// <summary>
	/// Represents a single marker sample from an OptiTrack frame.
	/// </summary>
	public sealed class OptiTrackMarker {

		/// <summary>
		/// Gets or sets the marker identifier from NatNet.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the marker label, when available.
		/// </summary>
		public string Label { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the X coordinate in stream space.
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Gets or sets the Y coordinate in stream space.
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Gets or sets the Z coordinate in stream space.
		/// </summary>
		public double Z { get; set; }
	}

	/// <summary>
	/// Represents a rigid-body sample from an OptiTrack frame.
	/// </summary>
	public sealed class OptiTrackRigidBody {

		/// <summary>
		/// Gets or sets the rigid-body identifier from NatNet.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the rigid-body name.
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets a value indicating whether the rigid body was tracked in this frame.
		/// </summary>
		public bool IsTracked { get; set; }

		/// <summary>
		/// Gets or sets the X position coordinate.
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Gets or sets the Y position coordinate.
		/// </summary>
		public double Y { get; set; }

		/// <summary>
		/// Gets or sets the Z position coordinate.
		/// </summary>
		public double Z { get; set; }

		/// <summary>
		/// Gets or sets the quaternion X component.
		/// </summary>
		public double Qx { get; set; }

		/// <summary>
		/// Gets or sets the quaternion Y component.
		/// </summary>
		public double Qy { get; set; }

		/// <summary>
		/// Gets or sets the quaternion Z component.
		/// </summary>
		public double Qz { get; set; }

		/// <summary>
		/// Gets or sets the quaternion W component.
		/// </summary>
		public double Qw { get; set; }
	}

	/// <summary>
	/// Represents a skeleton sample from an OptiTrack frame.
	/// </summary>
	public sealed class OptiTrackSkeleton {

		/// <summary>
		/// Gets or sets the skeleton identifier from NatNet.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the skeleton name.
		/// </summary>
		public string Name { get; set; } = string.Empty;
	}

	/// <summary>
	/// Represents normalized frame data consumed by Tracker components.
	/// </summary>
	public sealed class OptiTrackFrame {

		/// <summary>
		/// Initializes frame collections to empty lists.
		/// </summary>
		public OptiTrackFrame() {
			Markers = new List<OptiTrackMarker>();
			RigidBodies = new List<OptiTrackRigidBody>();
			Skeletons = new List<OptiTrackSkeleton>();
			StatusMessages = new List<string>();
		}

		/// <summary>
		/// Gets or sets the NatNet frame number.
		/// </summary>
		public int FrameNumber { get; set; }

		/// <summary>
		/// Gets or sets the frame timestamp, in seconds, from the NatNet stream.
		/// </summary>
		public double TimestampSeconds { get; set; }

		/// <summary>
		/// Gets or sets stream latency in seconds.
		/// </summary>
		public double LatencySeconds { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether Motive reports active recording.
		/// </summary>
		public bool IsRecording { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether tracked asset definitions changed.
		/// </summary>
		public bool AssetsChanged { get; set; }

		/// <summary>
		/// Gets or sets the frame marker set.
		/// </summary>
		public IReadOnlyList<OptiTrackMarker> Markers { get; set; }

		/// <summary>
		/// Gets or sets the frame rigid-body set.
		/// </summary>
		public IReadOnlyList<OptiTrackRigidBody> RigidBodies { get; set; }

		/// <summary>
		/// Gets or sets the frame skeleton set.
		/// </summary>
		public IReadOnlyList<OptiTrackSkeleton> Skeletons { get; set; }

		/// <summary>
		/// Gets or sets adapter status messages emitted while processing this frame.
		/// </summary>
		public IReadOnlyList<string> StatusMessages { get; set; }
	}
}
