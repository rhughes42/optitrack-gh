# Telemetry and Error Reporting Policy

Tracker must remain usable without telemetry. Error reporting is optional and disabled unless explicitly enabled in Grasshopper and configured by the user or deployment owner.

## Defaults

- Telemetry is optional.
- Error reporting is disabled by default.
- No Sentry DSN is hard-coded.
- No secrets, API keys, auth tokens, or organization/project-specific Sentry settings are stored in source control.
- The plugin must run normally when no telemetry configuration exists.

## Future Sentry Configuration

Tracker v1.7.0 documentation includes optional Sentry support through the official Sentry .NET SDK. Configuration should come from one of these sources:

- Environment variables
- A local config file excluded from source control
- A user setting controlled by the user or deployment owner

Recognized placeholder settings:

- `SENTRY_DSN` - optional DSN; telemetry remains disabled when absent or empty.
- `SENTRY_ENVIRONMENT` - optional environment name such as `development`, `lab`, or `production`.
- `SENTRY_RELEASE` - optional release identifier, for example `tracker@1.6.0`.
- `SENTRY_TRACES_SAMPLE_RATE` - optional numeric sample rate for aggregate performance telemetry.

A local config file named `tracker.telemetry.local.json` may be placed next to `Tracker.gha`. It is ignored by git and may contain:

```json
{
  "SENTRY_DSN": "",
  "SENTRY_ENVIRONMENT": "local",
  "SENTRY_RELEASE": "tracker@1.6.0",
  "SENTRY_TRACES_SAMPLE_RATE": "0"
}
```

## Current v1.9.0 Boundary

Tracker includes an internal telemetry boundary:

- `ITelemetryService`
- `NoOpTelemetryService`
- `TelemetryEvent`
- `TelemetryContext`
- `TelemetryScope`
- `TelemetrySanitizer`

The Grasshopper component and NatNet adapter use this boundary for sanitized exception and operation hooks. The default implementation is no-op, so no data leaves the process. `SentryTelemetryService` is used only when the component telemetry input is enabled and a valid DSN is present.

For setup steps, see [sentry.md](sentry.md).

Sentry exceptions are reported with sanitized exception type/message data rather than raw capture payloads. Context tags and metrics pass through `TelemetrySanitizer` before reporting.

## Safe vs Unsafe Payloads

Safe payload examples:

- `component=markers_to_points`
- `duration_ms=2.84`
- `marker_count=128`
- `rigid_body_count=4`
- `operation=calibrate_optitrack_frame`

Unsafe payload examples (must never be sent):

- marker coordinates or point cloud arrays
- rigid body names or skeleton names
- quaternion/pose payload values
- raw Rhino geometry serialization
- document paths or project/model names

## Sentry Plugin Operations

Repository maintainers may use the Codex Sentry plugin for read-only issue and event inspection during maintenance. That workflow is separate from Tracker runtime telemetry.

- The Sentry plugin requires a locally configured `SENTRY_AUTH_TOKEN` with read-only scopes.
- Do not paste Sentry tokens into chat or commit them to the repository.
- Optional local defaults such as `SENTRY_ORG`, `SENTRY_PROJECT`, and `SENTRY_BASE_URL` must stay outside source control.
- Sentry issue summaries shared in commits, issues, or release notes must redact emails, IP addresses, usernames, file paths, machine names, project names, and any capture-system identifiers.
- Do not copy raw stack traces or event payloads into documentation when they contain sensitive data.

## Data That Must Not Be Sent

Do not send:

- Raw OptiTrack or NatNet frame data
- Marker positions
- Rigid body names
- Skeleton, force plate, or asset names
- File paths
- IP addresses
- Machine names
- Usernames
- Rhino model names
- Project names
- Secrets, tokens, or license identifiers

## Scrubbing Requirements

Any future telemetry integration must scrub sensitive data before an event leaves the process. Error events should prefer coarse exception type, sanitized stack information, plugin version, and non-identifying state.

## Performance Monitoring

Performance monitoring, if enabled, uses low-cardinality spans and aggregate counters only. v1.8.0 span names:

- `natnet.connect`
- `natnet.disconnect`
- `natnet.frame_received`
- `grasshopper.solve`
- `grasshopper.geometry_conversion`
- `replay.frame_load`
- `replay.frame_step`
- `frame_buffer.consume`

Safe metrics include:

- `frame_count`
- `rigid_body_count`
- `marker_count`
- `skipped_frame_count`
- `dropped_frame_count`
- `solve_duration_ms`
- `conversion_duration_ms`
- `buffer_age_ms`
- `reconnect_count`

Acceptable aggregate examples include:

- Component solve duration buckets
- Connection attempt counts
- Sanitized error category counts
- NatNet connection state transitions without IP addresses or asset names

Do not attach frame payloads or per-marker/per-rigid-body values.

## SDK Compatibility Diagnostics (v1.9.0)

Tracker can emit sanitized compatibility diagnostics when telemetry is enabled and configured. Allowed compatibility tags:

- `adapter_name`
- `natnet_assembly_version`
- `plugin_version`
- `rhino_major_version`
- `grasshopper_version`
- `connection_mode`
- `sdk_load_failure_type`

Compatibility diagnostics must not include IP addresses, file paths, machine names, usernames, project/model names, or raw frame content.

## Recording and Replay Rules (v1.9.0)

Allowed recording/replay telemetry:

- `recording_start` / `recording_stop` event counts
- replay load success/failure counts
- parse error type categories
- aggregate frame counts
- replay playback duration and late-frame counters
- recording `format_version`

Prohibited recording/replay telemetry:

- recording file path
- recording file name from user input
- marker coordinates or point arrays
- rigid body names
- raw serialized frames
- project/model identifiers

## Disable Completely

Telemetry is disabled when `Enable Telemetry` is false, when `SENTRY_DSN` is missing or empty, or when local configuration is invalid. Users can remove `tracker.telemetry.local.json`, clear related environment variables, or set the component input to false to disable reporting completely.
