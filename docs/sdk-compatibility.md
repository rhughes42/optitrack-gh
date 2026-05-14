# SDK Compatibility (v1.9.0)

This document defines the currently supported OptiTrack SDK surface and the required verification process before any SDK upgrade.

## Current SDK Dependency

- Primary managed SDK assembly: `NatNetML.dll`
- Primary native runtime: `NatNetLib.dll`
- Current adapter in use: `OptiTrack.NatNet4Adapter`
- Underlying implementation: `OptiTrack.NatNet.NatNetOptiTrackClient`
- Target framework: `.NET Framework 4.8` (`x64`)

## Bundled SDK Files (Repository)

Current repository bundle in `lib/NatNet`:

- `NatNetML.dll`
- `NatNetLib.dll`
- `NatNetLib.lib`
- `NatNetML.xml`

Do not replace these binaries without completing the verification checklist below.

## Current Motive/NatNet Assumptions

- Motive is running and broadcasting NatNet.
- Connection can be `Multicast` or `Unicast`.
- NatNet connection uses command/data ports configured in component inputs.
- Data descriptors are fetched at connect and refreshed when tracking models change.
- Frame callback writes to latest-frame buffer; Grasshopper solve consumes at controlled cadence.

## NatNet SDK Usage Audit (Current Code)

- Assembly reference: `src/Tracker/Tracker.csproj` -> `NatNetML.dll` from `lib/NatNet`.
- Managed client class: `NatNetML.NatNetClientML`.
- Connection setup API: `NatNetClientML.ConnectParams` and `natNetClient.Connect(...)`.
- Frame callback API: `natNetClient.OnFrameReady += OnFrameReady`.
- Server descriptor API: `GetServerDescription(...)`.
- Data descriptor API: `GetDataDescriptions(out List<DataDescriptor>)`.
- Frame parsing path: `NatNetFrameConverter.ConvertFrame(...)` consuming `FrameOfMocapData`.
- Descriptor handling: marker set / rigid body / skeleton / force plate descriptor type checks.

## Compatibility Matrix

| Area | Supported Baseline | Verification Status | Notes |
| --- | --- | --- | --- |
| NatNet SDK | 4.0 bundle in `lib/NatNet` | Required per release | Current production adapter target. |
| Motive | Version compatible with NatNet 4.0 | Site-specific | Validate in deployment environment. |
| Rhino | 8.x | Required per release | Runtime-reported in diagnostics. |
| Grasshopper | Rhino 8 bundled version | Required per release | Runtime-reported in diagnostics. |
| .NET runtime target | .NET Framework 4.8 x64 | Source-controlled | Defined in `Tracker.csproj`. |

## Verification Checklist (Before Any SDK Upgrade)

1. Confirm legal/licensing redistribution terms for candidate SDK binaries.
2. Confirm managed/native assembly names and architecture compatibility (`x64`).
3. Validate load success for `NatNetML.dll` and native dependencies.
4. Verify connection setup API (`ConnectParams`, connection type, ports) remains compatible.
5. Verify frame callback API signature and threading behavior.
6. Verify frame parsing fields used by `NatNetFrameConverter` remain compatible.
7. Verify data descriptor API and descriptor type handling remain compatible.
8. Verify reconnect/disconnect cleanup and no duplicate event subscriptions.
9. Verify live capture + replay workflows still function with v1.8.0 buffering behavior.
10. Verify compatibility diagnostics and telemetry fields report expected versions safely.

## Upgrade Checklist (Managed Process)

1. Add/keep adapter boundary (do not directly swap all call sites).
2. Implement candidate adapter in `OptiTrack.NatNetLatestAdapter`.
3. Run manual compatibility matrix tests in a real Motive environment.
4. Keep NatNet 4 adapter operational until migration is verified.
5. Document failures/edge cases and rollback plan.
6. Promote candidate adapter only after explicit verification evidence.

## Manual Test Checklist

Use this checklist after any SDK or adapter change:

1. Motive running.
2. NatNet broadcast enabled.
3. Local/server IP set in component.
4. At least one rigid body visible/tracked in Motive.
5. Frame received and diagnostics updating.
6. Disconnect/reconnect cycle works repeatedly.
7. Replay mode still works with sample recording.
8. Telemetry disabled mode still works.
9. Telemetry enabled with test DSN emits sanitized diagnostics only.
