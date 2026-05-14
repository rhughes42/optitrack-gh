/*
 * File: InspectSdkCompatibilityComponent.cs
 * Purpose: Grasshopper diagnostics component for adapter/SDK compatibility visibility.
 * Scope: Grasshopper / Diagnostics / Telemetry
 * Notes: Emits only low-cardinality, sanitized compatibility tags when telemetry is enabled.
 */

using System;
using System.Collections.Generic;

using Grasshopper.Kernel;

using OptiTrack.Core;
using OptiTrack.NatNet4Adapter;
using OptiTrack.NatNetLatestAdapter;
using OptiTrack.Telemetry;


namespace Tracker.Components {

	/// <summary>
	/// Reports runtime SDK/adapter compatibility diagnostics without exposing capture payload or host identity data.
	/// </summary>
	public sealed class InspectSdkCompatibilityComponent : GH_Component {

		public InspectSdkCompatibilityComponent() : base(
				"Inspect SDK Compatibility",
				"SDKCompat",
				"Reports NatNet/Rhino/Grasshopper compatibility diagnostics without sensitive data.",
				"Tracker",
				"Diagnostics") { }


		protected override void RegisterInputParams(GH_InputParamManager pManager) {
			pManager.AddTextParameter("Connection Mode", "Mode", "Connection mode label for report context (Multicast/Unicast).", GH_ParamAccess.item, "Multicast");
			pManager.AddTextParameter("Adapter", "Adapter", "Adapter mode: NatNet4 or Latest.", GH_ParamAccess.item, "NatNet4");
			pManager.AddBooleanParameter("Enable Telemetry", "Telemetry", "Enable optional sanitized compatibility telemetry.", GH_ParamAccess.item, false);
		}


		protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
			pManager.AddTextParameter("Compatibility Report", "Report", "Runtime SDK compatibility fields.", GH_ParamAccess.list);
			pManager.AddTextParameter("Adapter Name", "Adapter", "Active adapter label.", GH_ParamAccess.item);
			pManager.AddTextParameter("Telemetry Status", "Telemetry", "Telemetry status string.", GH_ParamAccess.item);
		}


		protected override void SolveInstance(IGH_DataAccess DA) {
			string connectionMode  = "Multicast";
			string adapterMode     = "NatNet4";
			bool   enableTelemetry = false;

			DA.GetData(0, ref connectionMode);
			DA.GetData(1, ref adapterMode);
			DA.GetData(2, ref enableTelemetry);

			ITelemetryService     telemetry = TelemetryServiceProvider.GetService(enableTelemetry);
			SdkCompatibilityReport report = string.Equals(adapterMode, "Latest", StringComparison.OrdinalIgnoreCase)
					? NatNetLatestOptiTrackClient.BuildCompatibilityReport(ParseMode(connectionMode))
					: NatNet4OptiTrackClient.BuildCompatibilityReport(ParseMode(connectionMode));
			List<string>           lines    = report.ToDiagnosticsLines();

			telemetry.CaptureMessage(
					"sdk.compatibility",
					TelemetrySeverity.Debug,
					// Only compatibility metadata is emitted here; no frame payload, names, paths, or addresses.
					new TelemetryContext().SetTag("adapter_name", report.AdapterName)
										  .SetTag("adapter_version", report.AdapterVersion)
										  .SetTag("natnet_assembly_version", report.NatNetAssemblyVersion)
										  .SetTag("sdk_load_result", report.SdkLoadResult)
										  .SetTag("plugin_version", report.PluginVersion)
										  .SetTag("rhino_major_version", ParseRhinoMajor(report.RhinoVersion))
										  .SetTag("grasshopper_version", report.GrasshopperVersion)
										  .SetTag("connection_mode", report.ConnectionMode)
										  .SetTag("frame_schema_version", report.FrameSchemaVersion)
										  .SetTag("sdk_exception_type", string.IsNullOrWhiteSpace(report.SdkExceptionType) ? "none" : report.SdkExceptionType));

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
