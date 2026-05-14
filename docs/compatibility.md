# Compatibility

Tracker is a Rhino/Grasshopper plugin for OptiTrack Motive through NatNet.

For SDK-specific policy and upgrade workflow, see [sdk-compatibility.md](sdk-compatibility.md) and [sdk-upgrade-notes.md](sdk-upgrade-notes.md).

## Runtime and SDK Matrix (v1.10.0)

| Area | Current / Expected | Tested | Notes |
| --- | --- | --- | --- |
| Rhino version | Rhino 8 | TBD | Runtime diagnostics include Rhino version. |
| Grasshopper version | Rhino 8 bundled Grasshopper | TBD | Runtime diagnostics include Grasshopper version. |
| Windows version | Windows 10/11 | TBD | NatNet and Rhino plugin workflow are Windows-oriented. |
| Motive version | Compatible with NatNet 4.0 | TBD | Validate against deployment environment. |
| NatNet SDK support | NatNet 4.0 adapter baseline | TBD | Active adapter is `OptiTrack.NatNet4Adapter`. |
| Candidate latest SDK support | Not enabled | N/A | Placeholder only; requires formal verification. |
| .NET target framework | .NET Framework 4.8, x64 | Source-controlled | Required by `src/Tracker/Tracker.csproj`. |
| Sentry SDK version | 6.5.0 | Build validated | Optional and disabled unless configured. |

## Current Adapter Strategy

- Active adapter boundary: `OptiTrack.NatNet4Adapter`
- Existing implementation retained: `OptiTrack.NatNet`
- Future adapter placeholder: `OptiTrack.NatNetLatestAdapter`

## Compatibility Diagnostics

Tracker v1.10.0 emits safe compatibility diagnostics (component diagnostics and optional telemetry), including:

- `adapter_name`
- `plugin_version`
- `natnet_assembly_version`
- `connection_mode`
- `rhino_version`
- `grasshopper_version`
- `sentry_sdk_version`
- `sdk_load_failure_type`

Sensitive fields such as IPs, file paths, usernames, machine names, and frame payload data are excluded.

