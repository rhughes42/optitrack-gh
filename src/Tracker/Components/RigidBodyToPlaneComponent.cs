using System;

using Grasshopper.Kernel;

using OptiTrack.Geometry;
using OptiTrack.Telemetry;

using Rhino.Geometry;


namespace Tracker.Components {

	/// <summary>
	/// Converts rigid body pose values into a Rhino plane.
	/// </summary>
	public sealed class RigidBodyToPlaneComponent : GH_Component {

		public RigidBodyToPlaneComponent() : base(
				"Rigid Body To Plane",
				"RB->Plane",
				"Convert OptiTrack rigid body pose to a Rhino Plane.",
				"Tracker",
				"Geometry") { }


		protected override void RegisterInputParams(GH_InputParamManager pManager) {
			pManager.AddPointParameter("Origin", "O", "Rigid body origin in NatNet units (meters).", GH_ParamAccess.item);
			pManager.AddNumberParameter("Quaternion WXYZ", "Q", "Quaternion values as W,X,Y,Z.", GH_ParamAccess.list);
			pManager.AddNumberParameter("Scale Factor", "S", "Scale factor applied to the resulting plane.", GH_ParamAccess.item, 1.0);
			pManager.AddBooleanParameter("Y Up", "YUp", "Apply Y-up remap before output.", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false);
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
			pManager.AddPlaneParameter("Plane", "P", "Converted rigid body plane.", GH_ParamAccess.item);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA) {
			Point3d                                 origin           = Point3d.Origin;
			double                                  scaleFactor      = 1.0;
			bool                                    yUp              = false;
			bool                                    enableTelemetry  = false;
			System.Collections.Generic.List<double> quaternionValues = new System.Collections.Generic.List<double>();

			if (!DA.GetData(0, ref origin) || !DA.GetDataList(1, quaternionValues)) {
				return;
			}

			DA.GetData(2, ref scaleFactor);
			DA.GetData(3, ref yUp);
			DA.GetData(4, ref enableTelemetry);

			ITelemetryService telemetry = TelemetryServiceProvider.GetService(enableTelemetry);
			TelemetryContext  context   = new TelemetryContext().SetTag("component", "rigid_body_to_plane");
			context.SetMetric("rigid_body_count", 1);

			using (telemetry.StartSpan("rigid_body_to_plane", context)) {
				try {
					if (quaternionValues.Count != 4) {
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Quaternion input must contain exactly four values (W,X,Y,Z).");

						return;
					}

					Quaternion quaternion = new Quaternion(quaternionValues[0], quaternionValues[1], quaternionValues[2], quaternionValues[3]);
					Plane plane = OptiTrackGeometryConverter.RigidBodyPoseToPlane(origin, quaternion, scaleFactor, yUp, AxisRemapMode.None, Transform.Identity);
					DA.SetData(0, plane);
					DA.SetData(1, telemetry.Status);
				}
				catch (Exception exception) {
					telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "rigid_body_to_plane"));
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to convert rigid body pose to plane.");
				}
			}
		}


		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid("0A58C10A-09DB-4AAF-A486-A44C9162CDB1"); }
		}

	}

}
