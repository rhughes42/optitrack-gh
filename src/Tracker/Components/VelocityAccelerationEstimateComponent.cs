using System;
using System.Collections.Concurrent;

using Grasshopper.Kernel;

using OptiTrack.Telemetry;

using Rhino.Geometry;


namespace Tracker.Components {

	public sealed class VelocityAccelerationEstimateComponent : GH_Component {

		private sealed class MotionState {

			public Point3d  LastPoint;
			public Vector3d LastVelocity;
			public double   LastTimestamp;

		}


		private static readonly ConcurrentDictionary<Guid, MotionState> StateByComponent = new ConcurrentDictionary<Guid, MotionState>();


		public VelocityAccelerationEstimateComponent() : base(
				"Velocity / Acceleration Estimate",
				"VelAcc",
				"Estimate velocity and acceleration from point samples and timestamps.",
				"Tracker",
				"Calibration") { }


		protected override void RegisterInputParams(GH_InputParamManager pManager) {
			pManager.AddPointParameter("Point", "P", "Current tracked point.", GH_ParamAccess.item);
			pManager.AddNumberParameter("Timestamp", "T", "Current timestamp in seconds.", GH_ParamAccess.item);
			pManager.AddBooleanParameter("Reset", "R", "Reset estimator state for this component.", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false);
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
			pManager.AddVectorParameter("Velocity", "V", "Estimated velocity vector (units/sec).", GH_ParamAccess.item);
			pManager.AddVectorParameter("Acceleration", "A", "Estimated acceleration vector (units/sec^2).", GH_ParamAccess.item);
			pManager.AddNumberParameter("Speed", "Speed", "Velocity magnitude.", GH_ParamAccess.item);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA) {
			Point3d point           = Point3d.Origin;
			double  timestamp       = 0.0;
			bool    reset           = false;
			bool    enableTelemetry = false;

			if (!DA.GetData(0, ref point) || !DA.GetData(1, ref timestamp)) {
				return;
			}

			DA.GetData(2, ref reset);
			DA.GetData(3, ref enableTelemetry);

			ITelemetryService telemetry = TelemetryServiceProvider.GetService(enableTelemetry);
			TelemetryContext  context   = new TelemetryContext().SetTag("component", "velocity_acceleration_estimate");

			using (telemetry.StartSpan("velocity_acceleration_estimate", context)) {
				try {
					if (reset) {
						MotionState removed;
						StateByComponent.TryRemove(InstanceGuid, out removed);
					}

					MotionState state;

					if (!StateByComponent.TryGetValue(InstanceGuid, out state)) {
						state                          = new MotionState { LastPoint = point, LastVelocity = Vector3d.Zero, LastTimestamp = timestamp };
						StateByComponent[InstanceGuid] = state;
						DA.SetData(0, Vector3d.Zero);
						DA.SetData(1, Vector3d.Zero);
						DA.SetData(2, 0.0);
						DA.SetData(3, telemetry.Status);

						return;
					}

					double dt = timestamp - state.LastTimestamp;

					if (dt <= 0.0) {
						AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Timestamp must increase between samples for velocity estimation.");
						DA.SetData(0, state.LastVelocity);
						DA.SetData(1, Vector3d.Zero);
						DA.SetData(2, state.LastVelocity.Length);
						DA.SetData(3, telemetry.Status);

						return;
					}

					Vector3d velocity     = (point - state.LastPoint) / dt;
					Vector3d acceleration = (velocity - state.LastVelocity) / dt;

					state.LastPoint     = point;
					state.LastVelocity  = velocity;
					state.LastTimestamp = timestamp;
					context.SetMetric("sample_count", 1);

					DA.SetData(0, velocity);
					DA.SetData(1, acceleration);
					DA.SetData(2, velocity.Length);
					DA.SetData(3, telemetry.Status);
				}
				catch (Exception exception) {
					telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "velocity_acceleration_estimate"));
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to estimate velocity and acceleration.");
				}
			}
		}


		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid("ABECDE91-E351-4EBA-A02F-423A87DBA176"); }
		}

	}

}
