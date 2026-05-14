using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using OptiTrack.Core;
using OptiTrack.NatNet4Adapter;
using OptiTrack.Telemetry;


namespace Tracker.Components {

	public sealed class InspectSdkCompatibilityComponent : GH_Component {

		public InspectSdkCompatibilityComponent() : base(
				"Inspect SDK Compatibility",
				"SDKCompat",
				"Reports NatNet/Rhino/Grasshopper compatibility diagnostics without sensitive data.",
				"Tracker",
				"Diagnostics") { }


		protected override void RegisterInputParams(GH_InputParamManager pManager) {
			pManager.AddTextParameter("Connection Mode", "Mode", "Connection mode label for report context (Multicast/Unicast).", GH_ParamAccess.item, "Multicast");
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized compatibility telemetry.", GH_ParamAccess.item, false);
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
			pManager.AddTextParameter("Compatibility Report", "Report", "Runtime SDK compatibility fields.", GH_ParamAccess.list);
			pManager.AddTextParameter("Adapter Name", "Adapter", "Active adapter label.", GH_ParamAccess.item);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA) {
			string connectionMode  = "Multicast";
			bool   enableTelemetry = false;

			DA.GetData(0, ref connectionMode);
			DA.GetData(1, ref enableTelemetry);

			ITelemetryService     telemetry = TelemetryServiceProvider.GetService(enableTelemetry);
			SdkCompatibilityReport report   = NatNet4OptiTrackClient.BuildCompatibilityReport(ParseMode(connectionMode));
			List<string>           lines    = report.ToDiagnosticsLines();

			telemetry.CaptureMessage(
					"sdk.compatibility",
					TelemetrySeverity.Debug,
					new TelemetryContext().SetTag("adapter_name", report.AdapterName)
										  .SetTag("natnet_assembly_version", report.NatNetAssemblyVersion)
										  .SetTag("plugin_version", report.PluginVersion)
										  .SetTag("rhino_major_version", ParseRhinoMajor(report.RhinoVersion))
										  .SetTag("grasshopper_version", report.GrasshopperVersion)
										  .SetTag("connection_mode", report.ConnectionMode)
										  .SetTag("sdk_load_failure_type", string.IsNullOrWhiteSpace(report.SdkLoadFailureType) ? "none" : report.SdkLoadFailureType));

			DA.SetDataList(0, lines);
			DA.SetData(1, report.AdapterName);
			DA.SetData(2, telemetry.Status);
		}


		private static OptiTrackConnectionType ParseMode(string mode) {
			return string.Equals(mode, "Unicast", StringComparison.OrdinalIgnoreCase) ? OptiTrackConnectionType.Unicast : OptiTrackConnectionType.Multicast;
		}


		private static string ParseRhinoMajor(string rhinoVersion) {
			if (string.IsNullOrWhiteSpace(rhinoVersion)) {
				return "unknown";
			}

			int split = rhinoVersion.IndexOf('.');
			return split > 0 ? rhinoVersion.Substring(0, split) : rhinoVersion;
		}


		protected override System.Drawing.Bitmap Icon {
			get { return Properties.Icons.Tracker; }
		}

		public override Guid ComponentGuid {
			get { return new Guid("B9580C4B-F77E-40E0-9598-CA28FC59873A"); }
		}

	}

}
