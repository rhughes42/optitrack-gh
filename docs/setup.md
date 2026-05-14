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
3. Keep `Connection Type` as `Multicast` unless the Motive/network setup requires `Unicast`.
4. Keep NatNet ports at `1510` command and `1511` data unless Motive is configured differently.
5. Set `Connect` to `true`.
6. Use the component context menu to enable rigid-body output when required.
7. Inspect `Status`, `Warnings`, `Frame Number`, and `Telemetry Status`.

Optional adapter selection:

- default uses `NatNet4Adapter`
- set environment variable `TRACKER_NATNET_ADAPTER=latest` before launching Rhino to use `NatNetLatestAdapter`

The plugin does not require telemetry or Sentry configuration to run.

## Optional Sentry Configuration

Sentry is disabled by default. To enable it for sanitized error reporting:

1. Set `Enable Telemetry` to `true` on the component.
2. Provide a DSN through `SENTRY_DSN` or a local `tracker.telemetry.local.json` file next to the plugin output.
3. Optionally set `SENTRY_ENVIRONMENT`, `SENTRY_RELEASE`, and `SENTRY_TRACES_SAMPLE_RATE`.
4. If `SENTRY_RELEASE` is not set, Tracker uses `optitrack-gh@<plugin-version>`.

Do not commit local telemetry config files or DSNs.
