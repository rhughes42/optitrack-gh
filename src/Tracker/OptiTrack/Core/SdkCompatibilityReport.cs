/*
 * File: SdkCompatibilityReport.cs
 * Purpose: Collects runtime SDK/adapter compatibility diagnostics for UI and optional telemetry.
 * Scope: Core
 * Notes: Report fields are intentionally low-cardinality and exclude sensitive host/user/capture content.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace OptiTrack.Core {

	/// <summary>
	/// Runtime compatibility snapshot used by diagnostics and optional telemetry.
	/// </summary>
	public sealed class SdkCompatibilityReport {

		/// <summary>
		/// Gets or sets plugin informational version.
		/// </summary>
		public string PluginVersion { get; set; } = "unknown";

		/// <summary>
		/// Gets or sets selected adapter name.
		/// </summary>
		public string AdapterName { get; set; } = "unknown";

		/// <summary>
		/// Gets or sets selected adapter version.
		/// </summary>
		public string AdapterVersion { get; set; } = "unknown";

		/// <summary>
		/// Gets or sets declared SDK version supported by selected adapter.
		/// </summary>
		public string SupportedSdkVersion { get; set; } = "unknown";

		/// <summary>
		/// Gets or sets loaded NatNet assembly name.
		/// </summary>
		public string LoadedNatNetAssembly { get; set; } = "NatNetML";

		/// <summary>
		/// Gets or sets loaded NatNet assembly version.
		/// </summary>
		public string NatNetAssemblyVersion { get; set; } = "not_loaded";

		/// <summary>
		/// Gets or sets SDK load result classification.
		/// </summary>
		public string SdkLoadResult { get; set; } = "unknown";

		/// <summary>
		/// Gets or sets frame schema version label.
		/// </summary>
		public string FrameSchemaVersion { get; set; } = "unknown";

		/// <summary>
		/// Gets or sets active connection mode label.
		/// </summary>
		public string ConnectionMode { get; set; } = "unknown";

		/// <summary>
		/// Gets or sets Rhino version string.
		/// </summary>
		public string RhinoVersion { get; set; } = "unknown";

		/// <summary>
		/// Gets or sets Grasshopper version string.
		/// </summary>
		public string GrasshopperVersion { get; set; } = "unknown";

		/// <summary>
		/// Gets or sets Sentry SDK version string when present.
		/// </summary>
		public string SentrySdkVersion { get; set; } = "not_loaded";

		/// <summary>
		/// Gets or sets SDK exception type classification.
		/// </summary>
		public string SdkExceptionType { get; set; } = string.Empty;


		/// <summary>
		/// Formats this report into diagnostics lines for Grasshopper output.
		/// </summary>
		/// <returns>Stable low-cardinality diagnostics lines.</returns>
		public List<string> ToDiagnosticsLines() => new List<string> {
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


		/// <summary>
		/// Collects a compatibility report from loaded assemblies and runtime hosts.
		/// </summary>
		/// <param name="adapterName">Selected adapter name.</param>
		/// <param name="adapterVersion">Selected adapter version.</param>
		/// <param name="connectionMode">Connection mode label.</param>
		/// <param name="supportedSdkVersion">Declared SDK support label.</param>
		/// <param name="frameSchemaVersion">Frame schema label for diagnostics.</param>
		/// <returns>Compatibility report instance.</returns>
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


		static string FindAssemblyVersion(string assemblyName, out string loadResult) {
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
