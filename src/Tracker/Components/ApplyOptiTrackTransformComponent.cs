using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using OptiTrack.Telemetry;

using Rhino.Geometry;


namespace Tracker.Components {

	public sealed class ApplyOptiTrackTransformComponent : GH_Component {

		public ApplyOptiTrackTransformComponent() : base(
				"Apply OptiTrack Transform",
				"ApplyXForm",
				"Apply a transform to geometry for calibration/world mapping workflows.",
				"Tracker",
				"Geometry") { }


		protected override void RegisterInputParams(GH_InputParamManager pManager) {
			pManager.AddGeometryParameter("Geometry", "G", "Geometry to transform.", GH_ParamAccess.list);
			pManager.AddTransformParameter("Transform", "X", "Transform to apply.", GH_ParamAccess.item);
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false);
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
			pManager.AddGeometryParameter("Transformed Geometry", "TG", "Transformed geometry output.", GH_ParamAccess.list);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA) {
			List<IGH_GeometricGoo> geometry        = new List<IGH_GeometricGoo>();
			Transform              transform       = Transform.Identity;
			bool                   enableTelemetry = false;

			if (!DA.GetDataList(0, geometry)) {
				return;
			}

			DA.GetData(1, ref transform);
			DA.GetData(2, ref enableTelemetry);

			ITelemetryService telemetry = TelemetryServiceProvider.GetService(enableTelemetry);
			TelemetryContext  context   = new TelemetryContext().SetTag("component", "apply_optitrack_transform");
			context.SetMetric("geometry_count", geometry.Count);

			using (telemetry.StartSpan("apply_optitrack_transform", context)) {
				try {
					List<IGH_GeometricGoo> output = new List<IGH_GeometricGoo>(geometry.Count);

					for (int i = 0; i < geometry.Count; i++) {
						IGH_GeometricGoo duplicate = geometry[i].DuplicateGeometry();
						duplicate.Transform(transform);
						output.Add(duplicate);
					}

					DA.SetDataList(0, output);
					DA.SetData(1, telemetry.Status);
				}
				catch (Exception exception) {
					telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "apply_optitrack_transform"));
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to apply transform to geometry.");
				}
			}
		}


		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid("2C8C1CA0-CE2D-4672-81A5-7F27DD7A5C00"); }
		}

	}

}
