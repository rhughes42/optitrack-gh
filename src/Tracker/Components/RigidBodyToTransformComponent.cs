using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using OptiTrack.Geometry;
using OptiTrack.Telemetry;

using Rhino.Geometry;

namespace Tracker.Components
{

	public sealed class RigidBodyToTransformComponent : GH_Component
	{

		public RigidBodyToTransformComponent()
			: base("Rigid Body To Transform", "RB->XForm", "Convert OptiTrack rigid body pose to a Rhino Transform.", "Tracker", "Geometry")
		{
		}

		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddPointParameter("Origin", "O", "Rigid body origin in NatNet units (meters).", GH_ParamAccess.item);
			pManager.AddNumberParameter("Quaternion WXYZ", "Q", "Quaternion values as W,X,Y,Z.", GH_ParamAccess.list);
			pManager.AddNumberParameter("Scale Factor", "S", "Scale factor applied to the resulting transform.", GH_ParamAccess.item, 1.0);
			pManager.AddBooleanParameter("Y Up", "YUp", "Apply Y-up remap before output.", GH_ParamAccess.item, false);
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false);
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddTransformParameter("Transform", "X", "Converted rigid body transform.", GH_ParamAccess.item);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Point3d origin = Point3d.Origin;
			double scaleFactor = 1.0;
			bool yUp = false;
			bool enableTelemetry = false;
			List<double> quaternionValues = new List<double>();

			if (!DA.GetData(0, ref origin) || !DA.GetDataList(1, quaternionValues))
			{
				return;
			}

			DA.GetData(2, ref scaleFactor);
			DA.GetData(3, ref yUp);
			DA.GetData(4, ref enableTelemetry);

			ITelemetryService telemetry = TelemetryServiceProvider.GetService(enableTelemetry);
			TelemetryContext context = new TelemetryContext().SetTag("component", "rigid_body_to_transform");
			context.SetMetric("rigid_body_count", 1);

			using (telemetry.StartSpan("rigid_body_to_transform", context))
			{
				try
				{
					if (quaternionValues.Count != 4)
					{
						AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Quaternion input must contain exactly four values (W,X,Y,Z).");
						return;
					}

					Quaternion quaternion = new Quaternion(quaternionValues[0], quaternionValues[1], quaternionValues[2], quaternionValues[3]);
					Transform transform = OptiTrackGeometryConverter.RigidBodyPoseToTransform(origin, quaternion, scaleFactor, yUp, AxisRemapMode.None, Transform.Identity);
					DA.SetData(0, transform);
					DA.SetData(1, telemetry.Status);
				}
				catch (Exception exception)
				{
					telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "rigid_body_to_transform"));
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to convert rigid body pose to transform.");
				}
			}
		}

		protected override System.Drawing.Bitmap Icon
		{
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid
		{
			get { return new Guid("F9E53AF4-7638-4636-8D40-7B9A59DF5371"); }
		}
	}
}
