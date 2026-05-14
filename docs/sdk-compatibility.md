# SDK Compatibility (v1.11.0)

This document records what SDK support is actually verified in this repository environment.

## Local SDK Inspection Results

Files found in `lib/NatNet`:

- `NatNetML.dll` file/product version: `3.0.0.0`
- `NatNetLib.dll` file/product version: `3.0.0.0`
- `NatNetLib.lib`
- `NatNetML.xml`

No additional newer NatNet SDK binaries were found in this repository workspace.

## Current Supported Adapters

- `OptiTrack.NatNet4Adapter` (legacy compatibility mode, preserved)
- `OptiTrack.NatNetLatestAdapter` (latest available local SDK artifact mode)

Both adapters intentionally use the same internal NatNet transport and existing domain model path to preserve replay and conversion behavior.

## API Surface Comparison (NatNet4 Adapter vs Latest Adapter)

In this repository state, both adapters currently use the same SDK API surface because only one SDK binary set is available:

- Connection setup: `NatNetClientML.ConnectParams` + `Connect(...)`
- Local/server IP fields: unchanged (`LocalAddress`, `ServerAddress`)
- Multicast/unicast mapping: unchanged
- Frame callback: `OnFrameReady(FrameOfMocapData, NatNetClientML)`
- Rigid body/marker/skeleton parsing: unchanged converter path
- Data descriptor handling: unchanged (`GetDataDescriptions(...)`)
- Timestamp/frame number: unchanged (`iFrame`, `fTimestamp`, host timestamp helpers)

## Runtime Adapter Selection

Selection is runtime via environment variable:

- `TRACKER_NATNET_ADAPTER=latest` -> `NatNetLatestAdapter`
- default/other -> `NatNet4Adapter`

## Verification Scope and Claims

Verified here:

- SDK file presence and file versions
- Adapter selection and compatibility diagnostics path
- Non-sensitive telemetry field emission path (when telemetry is enabled)

Not verified in this environment:

- Live Motive streaming session against physical tracking hardware

Do not claim broader SDK/Motive compatibility until live hardware validation is completed.

## Manual Verification Checklist

1. Motive running.
2. NatNet broadcast enabled.
3. Local/server IP configured.
4. Rigid body visible.
5. Marker stream visible.
6. Reconnect cycle succeeds.
7. Replay mode still works.
8. Telemetry disabled default verified.
9. Telemetry enabled with test DSN emits only safe fields.
