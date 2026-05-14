/*
 * File: OptiTrackGeometryConverter.cs
 * Purpose: Shared Rhino geometry conversion helpers for OptiTrack pose/marker data.
 * Scope: Geometry
 * Notes: Keeps axis remap and scale assumptions centralized for consistency across components.
 */

using System;
using System.Collections.Generic;

using Rhino.Geometry;


namespace OptiTrack.Geometry {

	/// <summary>
	/// Geometry conversion utilities used by Tracker components.
	/// </summary>
	public static class OptiTrackGeometryConverter {

		public static Plane RigidBodyPoseToPlane(Point3d       origin,
												 Quaternion    quaternion,
												 double        scaleFactor,
												 bool          yUp,
												 AxisRemapMode axisRemapMode,
												 Transform     worldTransform) {
			quaternion.GetRotation(out Plane plane);
			plane.Origin = origin;

			ApplyAxisRemap(ref plane, yUp, axisRemapMode);
			ApplyScale(ref plane, scaleFactor);
			ApplyWorldTransform(ref plane, worldTransform);

			return plane;
		}


		public static Transform RigidBodyPoseToTransform(Point3d       origin,
														 Quaternion    quaternion,
														 double        scaleFactor,
														 bool          yUp,
														 AxisRemapMode axisRemapMode,
														 Transform     worldTransform) {
			Plane plane = RigidBodyPoseToPlane(origin, quaternion, scaleFactor, yUp, axisRemapMode, worldTransform);

			return Transform.PlaneToPlane(Plane.WorldXY, plane);
		}


		public static List<Point3d> MarkersToPoints(IList<Point3d> markers, double scaleFactor, AxisRemapMode axisRemapMode, Transform worldTransform) {
			List<Point3d> points = new List<Point3d>(markers.Count);

			for (int i = 0; i < markers.Count; i++) {
				Point3d point = markers[i];
				ApplyAxisRemap(ref point, axisRemapMode);

				if (Math.Abs(scaleFactor - 1.0) > 0.000001) {
					point.Transform(Transform.Scale(Point3d.Origin, scaleFactor));
				}

				point.Transform(worldTransform);
				points.Add(point);
			}

			return points;
		}


		public static Transform BuildCalibrationTransform(Plane sourcePlane, Plane targetPlane) {
			return Transform.PlaneToPlane(sourcePlane, targetPlane);
		}


		public static Plane ApplyCalibration(Plane inputPlane, Transform calibrationTransform) {
			Plane outputPlane = inputPlane;
			outputPlane.Transform(calibrationTransform);

			return outputPlane;
		}


		private static void ApplyScale(ref Plane plane, double scaleFactor) {
			if (Math.Abs(scaleFactor - 1.0) <= 0.000001) {
				return;
			}

			plane.Transform(Transform.Scale(Point3d.Origin, scaleFactor));
		}


		private static void ApplyWorldTransform(ref Plane plane, Transform worldTransform) {
			if (worldTransform == Transform.Identity) {
				return;
			}

			plane.Transform(worldTransform);
		}


		private static void ApplyAxisRemap(ref Plane plane, bool yUp, AxisRemapMode axisRemapMode) {
			if (yUp || axisRemapMode == AxisRemapMode.ZUpToYUp) {
				Transform yUpTransform = Transform.Identity;
				yUpTransform.M00 = 1;
				yUpTransform.M11 = 0;
				yUpTransform.M12 = -1;
				yUpTransform.M21 = 1;
				yUpTransform.M22 = 0;
				plane.Transform(yUpTransform);
			}

			Transform xRotate = Transform.Identity;
			xRotate.M00 = 0;
			xRotate.M01 = -1;
			xRotate.M10 = 1;
			xRotate.M11 = 0;
			plane.Transform(xRotate);
		}


		private static void ApplyAxisRemap(ref Point3d point, AxisRemapMode axisRemapMode) {
			if (axisRemapMode == AxisRemapMode.None) {
				return;
			}

			double x = point.X;
			double y = point.Y;
			double z = point.Z;
			point = new Point3d(x, z, -y);
		}

	}

}
