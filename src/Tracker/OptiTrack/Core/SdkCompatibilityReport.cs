using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace OptiTrack.Core {

	public sealed class SdkCompatibilityReport {

		public string PluginVersion { get; set; } = "unknown";

		public string AdapterName { get; set; } = "unknown";

		public string NatNetAssemblyVersion { get; set; } = "not_loaded";

		public string ConnectionMode { get; set; } = "unknown";

		public string RhinoVersion { get; set; } = "unknown";

		public string GrasshopperVersion { get; set; } = "unknown";

		public string SentrySdkVersion { get; set; } = "not_loaded";

		public string SdkLoadFailureType { get; set; } = string.Empty;


		public List<string> ToDiagnosticsLines() {
			return new List<string> {
					"adapter_name=" + AdapterName,
					"plugin_version=" + PluginVersion,
					"natnet_assembly_version=" + NatNetAssemblyVersion,
					"connection_mode=" + ConnectionMode,
					"rhino_version=" + RhinoVersion,
					"grasshopper_version=" + GrasshopperVersion,
					"sentry_sdk_version=" + SentrySdkVersion,
					"sdk_load_failure_type=" + (string.IsNullOrWhiteSpace(SdkLoadFailureType) ? "none" : SdkLoadFailureType)
			};
		}


		public static SdkCompatibilityReport Collect(string adapterName, string connectionMode) {
			SdkCompatibilityReport report = new SdkCompatibilityReport {
					AdapterName    = adapterName ?? "unknown",
					ConnectionMode = connectionMode ?? "unknown",
					PluginVersion  = typeof(SdkCompatibilityReport).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown"
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
				report.NatNetAssemblyVersion = FindAssemblyVersion("NatNetML");
			}
			catch (Exception exception) {
				report.SdkLoadFailureType = exception.GetType().Name;
			}

			try {
				report.SentrySdkVersion = FindAssemblyVersion("Sentry");
			}
			catch { }

			return report;
		}


		private static string FindAssemblyVersion(string assemblyName) {
			Assembly loaded = AppDomain.CurrentDomain.GetAssemblies()
									 .FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));

			if (loaded != null) {
				return loaded.GetName().Version?.ToString() ?? "unknown";
			}

			try {
				Assembly onDemand = Assembly.Load(assemblyName);
				return onDemand.GetName().Version?.ToString() ?? "unknown";
			}
			catch {
				return "not_loaded";
			}
		}

	}

}
