using System;
using System.Drawing;

using Grasshopper.Kernel;

namespace Tracker {

	/// <summary>
	/// Represents the assembly information for the Tracker plugin in Grasshopper.
	/// </summary>
	public class TrackerInfo : GH_AssemblyInfo {

		/// <summary>
		/// Gets the name of the Tracker plugin.
		/// </summary>
		/// <returns> The name of the plugin. </returns>
		public override string Name {
			get {
				return "Tracker";
			}
		}

		/// <summary>
		/// Gets the icon of the Tracker plugin.
		/// </summary>
		/// <returns> The icon of the plugin. </returns>
		public override Bitmap Icon {
			get {
				//Return a 24x24 pixel bitmap to represent this GHA library.
				return null;
			}
		}

		/// <summary>
		/// Gets the description of the Tracker plugin.
		/// </summary>
		/// <returns> The description of the plugin. </returns>
		public override string Description {
			get {
				//Return a short string describing the purpose of this GHA library.
				return "Supports real-time motion tracking with OptiTrack camera systems using NaturalPoint's NatNet API.";
			}
		}

		/// <summary>
		/// Gets the ID of the Tracker plugin.
		/// </summary>
		/// <returns> The ID of the plugin. </returns>
		public override Guid Id {
			get {
				return new Guid( "CF473A4D-483F-4556-B3FA-91ED21096B82" );
			}
		}

		/// <summary>
		/// Gets the author name of the Tracker plugin.
		/// </summary>
		/// <returns> The author name of the plugin. </returns>
		public override string AuthorName {
			get {
				//Return a string identifying you or your company.
				return "Graph Technologies";
			}
		}

		/// <summary>
		/// Gets the author contact of the Tracker plugin.
		/// </summary>
		/// <returns> The author contact of the plugin. </returns>
		public override string AuthorContact {
			get {
				//Return a string representing your preferred contact details.
				return "tech@graphconsult.xyz";
			}
		}
	}
}