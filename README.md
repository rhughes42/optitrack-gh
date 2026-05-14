# Tracker

Tracker is a Grasshopper plugin for receiving real-time OptiTrack motion-capture data from NaturalPoint Motive through the NatNet API.

The plugin currently exposes an **OptiTrack Stream** component that connects to a running Motive session, receives NatNet frame data, and outputs Rhino/Grasshopper geometry such as marker points and rigid-body planes.

## Requirements

- Rhino 8 with Grasshopper
- Windows
- OptiTrack Motive running on the local network
- Motive configured to broadcast NatNet data
- NatNet SDK 4.0 managed/native runtime files

This repository currently bundles the NatNet 4.0 runtime files under `lib/NatNet`:

- `NatNetML.dll`
- `NatNetLib.dll`
- `NatNetLib.lib`
- `NatNetML.xml`

The bundled SDK license status should be reviewed before redistribution. The files are retained for build and runtime compatibility because the plugin depends on `NatNetML.dll`.

## Repository Layout

- `src/Tracker` - Grasshopper plugin source and project file
- `lib/NatNet` - bundled NatNet SDK runtime/reference files
- `docs` - setup, compatibility, troubleshooting, build, and telemetry notes
- `examples` - reserved for future example Grasshopper definitions

## Installation

1. Build the solution in Release mode, or use a packaged release.
2. If Windows marks the downloaded archive or files as blocked, right-click the `.zip`, `.gha`, or `.dll`, open **Properties**, select **Unblock**, and apply the change.
3. Copy the built `Tracker.gha` and required NatNet DLLs into your Grasshopper Libraries folder, commonly:

   `%APPDATA%\Grasshopper\Libraries\Tracker`

4. Restart Rhino and Grasshopper.

See [docs/setup.md](docs/setup.md) for more detail.

## Basic Usage

1. Start Motive and confirm the OptiTrack system is tracking.
2. Enable NatNet broadcasting in Motive.
3. In Grasshopper, place the **Tracker > OptiTrack > OptiTrack Stream** component.
4. Set `Local IP` to the network adapter address used by Rhino/Grasshopper.
5. Set `Server IP` to the Motive machine address.
6. Set `Activate` to `true`.
7. Use the component menu to enable rigid-body output when needed.

## Known Limitations

- NatNet connection mode is currently multicast in code.
- Local and server IP values default to `127.0.0.1`; multi-machine setups usually require explicit network adapter addresses.
- Skeleton and force-plate paths are present but not fully implemented.
- The project targets .NET Framework 4.8 and is intended for Rhino/Grasshopper on Windows.
- CI builds may be limited by Rhino/Grasshopper and NatNet runtime availability.

## Telemetry and Error Reporting

Telemetry/error reporting is optional and disabled unless explicitly enabled and configured. No Sentry DSN or project-specific telemetry setting is stored in source control.

Sentry configuration uses environment variables or a local config file such as:

- `SENTRY_DSN`
- `SENTRY_ENVIRONMENT`
- `SENTRY_RELEASE`
- `SENTRY_TRACES_SAMPLE_RATE`

Raw motion-capture data, marker positions, rigid-body names, file paths, IP addresses, machine names, usernames, model names, and project names must not be sent to telemetry. See [docs/telemetry.md](docs/telemetry.md).

Maintainers may use the Codex Sentry plugin for read-only issue review, but that requires a local `SENTRY_AUTH_TOKEN` and is separate from runtime telemetry.

## Version

Current modernization target: `v1.4.0`.

## Contributors

- Ryan Hughes
- Povl-Sonne Frederiksen
