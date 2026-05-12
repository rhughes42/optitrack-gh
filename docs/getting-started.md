# Getting Started

This guide gets Tracker running in Grasshopper with either live Motive data or replayed sample data.

## Prerequisites

- Windows
- Rhino 8 with Grasshopper
- Tracker build artifacts (`Tracker.gha`, `NatNetML.dll`, `NatNetLib.dll`)
- For live capture: Motive broadcasting NatNet

## Install

1. Build Tracker in `Release`, or download a release package.
2. Unblock downloaded files in Windows file properties if needed.
3. Copy `Tracker.gha`, `NatNetML.dll`, and `NatNetLib.dll` to:
   - `%APPDATA%\Grasshopper\Libraries\Tracker`
4. Restart Rhino and Grasshopper.

## First Live Connection

1. Open Grasshopper.
2. Add `Tracker > OptiTrack > OptiTrack Stream`.
3. Set:
   - `Connect = true`
   - `Local IP = 127.0.0.1` (single-machine) or your NIC IP
   - `Server IP = Motive host IP`
   - `Connection Type = Multicast` (default)
4. Confirm frame updates on `Frame Number` and geometry outputs.

## First Offline Replay (No Motive Required)

1. Add `Load OptiTrack Recording`.
2. Set file path to `examples/data/sample-rigid-body-recording.json`.
3. Connect output to `Replay OptiTrack Recording`.
4. Set `Play = true`.
5. Route replay `Frame` into geometry components for local testing.

## Next Steps

- Component reference: [grasshopper-components.md](grasshopper-components.md)
- Networking guidance: [optitrack-networking.md](optitrack-networking.md)
- Example walkthroughs: `../examples`
- Optional telemetry/Sentry: [telemetry.md](telemetry.md), [sentry.md](sentry.md)
