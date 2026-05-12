# Setup

## Build Locally

1. Install Visual Studio with .NET Framework 4.8 targeting support.
2. Restore NuGet packages for `Tracker.sln`.
3. Build `Tracker.sln` in Release mode.
4. Confirm the output folder contains:

   - `Tracker.gha`
   - `NatNetML.dll`
   - `NatNetLib.dll`

The project copies bundled NatNet files from `lib/NatNet` to the build output.

## Install Into Grasshopper

1. Close Rhino.
2. Create a folder under `%APPDATA%\Grasshopper\Libraries`, for example:

   `%APPDATA%\Grasshopper\Libraries\Tracker`

3. Copy `Tracker.gha`, `NatNetML.dll`, and `NatNetLib.dll` into that folder.
4. Unblock downloaded files if Windows marks them as blocked.
5. Start Rhino and open Grasshopper.
6. Look for the **Tracker** tab/category.

## Configure Motive

1. Start Motive and load or create the tracking project.
2. Confirm cameras and tracked assets are active.
3. Enable NatNet streaming/broadcasting in Motive.
4. Note the server IP address shown by Motive.
5. Use the same network adapter address as `Local IP` in Grasshopper.

## Grasshopper Component

1. Place **OptiTrack Stream** from the Tracker category.
2. Set `Local IP` and `Server IP`.
3. Set `Activate` to `true`.
4. Use the component context menu to enable rigid-body output when required.

The plugin should not require telemetry or Sentry configuration to run.
