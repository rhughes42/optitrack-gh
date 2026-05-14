using System;


namespace OptiTrack.NatNetLatestAdapter {

	/// <summary>
	/// Placeholder for a future validated adapter targeting a newer NatNet SDK.
	/// This type is intentionally not wired into runtime connection logic.
	/// </summary>
	public static class NatNetLatestAdapterPlaceholder {

		public const string AdapterName = "NatNetLatestAdapter";


		public static void ThrowNotSupported() {
			throw new NotSupportedException("NatNet latest adapter is not implemented. Complete SDK verification before enabling.");
		}

	}

}
