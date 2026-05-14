using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace OptiTrack.Core {

	public sealed class SdkCompatibilityReport {

		public string PluginVersion { get; set; } = "unknown";

		public string AdapterName { get; set; } = "unknown";

		public string AdapterVersion { get; set; } = "unknown";

		public string SupportedSdkVersion { get; set; } = "unknown";

		public string LoadedNatNetAssembly { get; set; } = "NatNetML";

		public string NatNetAssemblyVersion { get; set; } = "not_loaded";

		public string SdkLoadResult { get; set; } = "unknown";

		public string FrameSchemaVersion { get; set; } = "unknown";

		public string ConnectionMode { get; set; } = "unknown";

		public string RhinoVersion { get; set; } = "unknown";

		public string GrasshopperVersion { get; set; } = "unknown";

		public string SentrySdkVersion { get; set; } = "not_loaded";

		public string SdkExceptionType { get; set; } = string.Empty;


		public List<string> ToDiagnosticsLines() {
			return new List<string> {
					"adapter_name=" + AdapterName,
					"adapter_version=" + AdapterVersion,
					"plugin_version=" + PluginVersion,
					"loaded_natnet_assembly=" + LoadedNatNetAssembly,
					"natnet_assembly_version=" + NatNetAssemblyVersion,
					"supported_sdk_version=" + SupportedSdkVersion,
					"sdk_load_result=" + SdkLoadResult,
					"connection_mode=" + ConnectionMode,
					"frame_schema_version=" + FrameSchemaVersion,
					"rhino_version=" + RhinoVersion,
					"grasshopper_version=" + GrasshopperVersion,
					"sentry_sdk_version=" + SentrySdkVersion,
					"sdk_exception_type=" + (string.IsNullOrWhiteSpace(SdkExceptionType) ? "none" : SdkExceptionType)
			};
		}


		public static SdkCompatibilityReport Collect(string adapterName, string adapterVersion, string connectionMode, string supportedSdkVersion, string frameSchemaVersion) {
			SdkCompatibilityReport report = new SdkCompatibilityReport {
					AdapterName    = adapterName ?? "unknown",
					AdapterVersion = adapterVersion ?? "unknown",
					ConnectionMode = connectionMode ?? "unknown",
					SupportedSdkVersion = supportedSdkVersion ?? "unknown",
					FrameSchemaVersion  = frameSchemaVersion ?? "unknown",
					PluginVersion       = typeof(SdkCompatibilityReport).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown"
			};

			try {
				report.RhinoVersion = Rhino.RhinoApp.Version.ToString();
			}
			catch { }

			try {
				report.GrasshopperVersion = typeof(Grasshopper.Kernel.GH_Component).Assembly.GetName().Version?.ToString() ?? "unknown";
			}
			catch { }

			try {
				report.NatNetAssemblyVersion = FindAssemblyVersion("NatNetML", out string loadResult);
				report.SdkLoadResult         = loadResult;
			}
			catch (Exception exception) {
				report.SdkLoadResult   = "exception";
				report.SdkExceptionType = exception.GetType().Name;
			}

			try {
				report.SentrySdkVersion = FindAssemblyVersion("Sentry");
			}
			catch { }

			return report;
		}


		private static string FindAssemblyVersion(string assemblyName, out string loadResult) {
			Assembly loaded = AppDomain.CurrentDomain.GetAssemblies()
									 .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));

			if (loaded != null) {
				loadResult = "already_loaded";
				return loaded.GetName().Version?.ToString() ?? "unknown";
			}

			try {
				Assembly onDemand = Assembly.Load(assemblyName);
				loadResult = "loaded_on_demand";
				return onDemand.GetName().Version?.ToString() ?? "unknown";
			}
			catch {
				loadResult = "not_loaded";
				return "not_loaded";
			}
		}

	}

}
