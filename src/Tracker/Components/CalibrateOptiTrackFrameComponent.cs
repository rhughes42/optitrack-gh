using System;

using Grasshopper.Kernel;

using OptiTrack.Geometry;
using OptiTrack.Telemetry;

using Rhino.Geometry;

namespace Tracker.Components
{

	public sealed class CalibrateOptiTrackFrameComponent : GH_Component
	{

		public CalibrateOptiTrackFrameComponent()
			: base("Calibrate OptiTrack Frame", "Calibrate", "Build a calibration transform from source and target coordinate frames.", "Tracker", "Calibration")
		{
		}

		protected override void RegisterInputParams(GH_InputParamManager pManager)
		{
			pManager.AddPlaneParameter("Source Frame", "S", "Source OptiTrack frame plane.", GH_ParamAccess.item, Plane.WorldXY);
			pManager.AddPlaneParameter("Target Frame", "T", "Target world/robot frame plane.", GH_ParamAccess.item, Plane.WorldXY);
			pManager.AddPlaneParameter("Plane To Calibrate", "P", "Optional plane to transform with calibration.", GH_ParamAccess.item, Plane.WorldXY);
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized telemetry.", GH_ParamAccess.item, false);
		}

		protected override void RegisterOutputParams(GH_OutputParamManager pManager)
		{
			pManager.AddTransformParameter("Calibration Transform", "X", "Transform from source frame to target frame.", GH_ParamAccess.item);
			pManager.AddPlaneParameter("Calibrated Plane", "CP", "Input plane transformed by calibration transform.", GH_ParamAccess.item);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item);
		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			Plane source = Plane.WorldXY;
			Plane target = Plane.WorldXY;
			Plane input = Plane.WorldXY;
			bool enableTelemetry = false;

			if (!DA.GetData(0, ref source) || !DA.GetData(1, ref target) || !DA.GetData(2, ref input))
			{
				return;
			}

			DA.GetData(3, ref enableTelemetry);
			ITelemetryService telemetry = TelemetryServiceProvider.GetService(enableTelemetry);
			TelemetryContext context = new TelemetryContext().SetTag("component", "calibrate_optitrack_frame");

			using (telemetry.StartSpan("calibrate_optitrack_frame", context))
			{
				try
				{
					Transform calibrationTransform = OptiTrackGeometryConverter.BuildCalibrationTransform(source, target);
					Plane calibratedPlane = OptiTrackGeometryConverter.ApplyCalibration(input, calibrationTransform);
					DA.SetData(0, calibrationTransform);
					DA.SetData(1, calibratedPlane);
					DA.SetData(2, telemetry.Status);
				}
				catch (Exception exception)
				{
					telemetry.CaptureException(exception, new TelemetryContext().SetTag("operation", "calibrate_optitrack_frame"));
					AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to calibrate OptiTrack frame.");
				}
			}
		}

		protected override System.Drawing.Bitmap Icon
		{
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid
		{
			get { return new Guid("F9A7F429-322B-4A9D-9BD3-BF5B457E55AD"); }
		}
	}
}
