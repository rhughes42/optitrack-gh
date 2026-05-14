using System;
using System.Collections.Concurrent;

using Grasshopper.Kernel;

using OptiTrack.Telemetry;

using Rhino.Geometry;


namespace Tracker.Components {

	/// <summary>
	/// Applies exponential smoothing to incoming pose/point samples.
	/// </summary>
	public sealed class SmoothPoseStreamComponent : GH_Component {

		private static readonly ConcurrentDictionary<Guid, Plane> LastPlaneByComponent = new ConcurrentDictionary<Guid, Plane>();


		public SmoothPoseStreamComponent() : base(
				"Smooth Pose Stream",
				"SmoothPose",
				"Apply exponential smoothing to a pose plane stream.",
				"Tracker",
				"Calibration") { }


		protected override void RegisterInputParams(GH_InputParamManager pManager) {
			pManager.AddPlaneParameter("Plane", "P", "Incoming pose plane.", GH_ParamAccess.item, Plane.WorldXY);
			pManager.AddNumberParameter("Smoothing Alpha", "A", "Alpha in [0,1]. Higher values follow input more closely.", GH_ParamAccess.item, 0.35);
			pManager.AddBooleanParameter("Reset", "R", "Reset smoothing state for this component.", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false);
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
			pManager.AddPlaneParameter("Smoothed Plane", "SP", "Smoothed pose plane.", GH_ParamAccess.item);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA) {
			Plane  current         = Plane.WorldXY;
			double alpha           = 0.35;
			bool   reset           = false;
			bool   enableTelemetry = false;

			if (!DA.GetData(0, ref current)) {
				return;
			}

			DA.GetData(1, ref alpha);
			DA.GetData(2, ref reset);
			DA.GetData(3, ref enableTelemetry);

			alpha = Math.Max(0.0, Math.Min(1.0, alpha));
			ITelemetryService telemetry = TelemetryServiceProvider.GetService(enableTelemetry);
			TelemetryContext  context   = new TelemetryContext().SetTag("component", "smooth_pose_stream");

			using (telemetry.StartSpan("smooth_pose_stream", context)) {
				try {
					if (reset) {
						Plane removed;
						LastPlaneByComponent.TryRemove(InstanceGuid, out removed);
					}

					Plane previous;

					if (!LastPlaneByComponent.TryGetValue(InstanceGuid, out previous)) {
						LastPlaneByComponent[InstanceGuid] = current;
						DA.SetData(0, current);
						DA.SetData(1, telemetry.Status);

						return;
					}

					Point3d  origin   = LerpPoint(previous.Origin, current.Origin, alpha);
					Vector3d xAxis    = LerpVector(previous.XAxis, current.XAxis, alpha);
					Vector3d yAxis    = LerpVector(previous.YAxis, current.YAxis, alpha);
					Plane    smoothed = new Plane(origin, xAxis, yAxis);
					LastPlaneByComponent[InstanceGuid] = smoothed;

					DA.SetData(0, smoothed);
					DA.SetData(1, telemetry.Status);
				}
				catch (Exception exception) {
					telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "smooth_pose_stream"));
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to smooth pose stream.");
				}
			}
		}


		private static Point3d LerpPoint(Point3d a, Point3d b, double t) {
			return new Point3d(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
		}


		private static Vector3d LerpVector(Vector3d a, Vector3d b, double t) {
			Vector3d vector = new Vector3d(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
			vector.Unitize();

			return vector;
		}


		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid("2D39DDDA-1948-44E9-AED9-6E0B3FF4F080"); }
		}

	}

}
