# Compatibility

Tracker is a Rhino/Grasshopper plugin for OptiTrack Motive through NatNet.

For SDK policy and upgrade workflow, see [sdk-compatibility.md](sdk-compatibility.md), [sdk-upgrade-notes.md](sdk-upgrade-notes.md), and [sdk-migration-notes.md](sdk-migration-notes.md).

## Runtime and SDK Matrix (v1.11.0)

| Area | Version / Value | Tested Status | Notes |
| --- | --- | --- | --- |
| Windows | Microsoft Windows 11 Home 10.0.26200 | Verified in local build environment | Captured from local system. |
| Rhino | Rhino 8.x expected | Runtime-detected only | Actual version depends on user installation. |
| Grasshopper | Rhino 8 bundled GH | Runtime-detected only | Reported at runtime by diagnostics component. |
| Motive | Not verified in this environment | Not tested | Hardware/software unavailable in this workspace. |
| NatNet managed assembly file version | `3.0.0.0` (`NatNetML.dll`) | Verified from local files | Latest available SDK artifact in this environment. |
| NatNet native assembly file version | `3.0.0.0` (`NatNetLib.dll`) | Verified from local files | Latest available SDK artifact in this environment. |
| NatNet4 adapter | `OptiTrack.NatNet4Adapter` | Supported | Legacy compatibility mode retained. |
| NatNet latest adapter | `OptiTrack.NatNetLatestAdapter` | Supported for latest local SDK artifact | Uses same frame/domain model path with updated diagnostics. |
| .NET target framework | .NET Framework 4.8 x64 | Source-controlled | Defined in `src/Tracker/Tracker.csproj`. |
| Sentry SDK | 6.5.0 package reference | Source-controlled | Optional and disabled unless configured. |

## Adapter Selection

Runtime adapter selection is environment-driven:

- `TRACKER_NATNET_ADAPTER=latest` selects `NatNetLatestAdapter`
- Any other value (or unset) selects `NatNet4Adapter`

This keeps existing behavior stable while allowing explicit selection of the latest local SDK adapter path.

## Safe Compatibility Diagnostics

Tracker emits non-sensitive compatibility diagnostics fields:

- `adapter_name`
- `adapter_version`
- `loaded_natnet_assembly`
- `natnet_assembly_version`
- `supported_sdk_version`
- `sdk_load_result`
- `connection_mode`
- `frame_schema_version`
- `sdk_exception_type`
- `plugin_version`
- `rhino_version`
- `grasshopper_version`
- `sentry_sdk_version`

Sensitive fields (IPs, file paths, usernames, machine names, marker/rigid-body content, raw frames, project/model names) are excluded.
