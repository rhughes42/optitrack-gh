# Telemetry and Error Reporting Policy

Tracker must remain usable without telemetry. Error reporting is optional and disabled unless explicitly enabled in Grasshopper and configured by the user or deployment owner.

## Defaults

- Telemetry is optional.
- Error reporting is disabled by default.
- No Sentry DSN is hard-coded.
- No secrets, API keys, auth tokens, or organization/project-specific Sentry settings are stored in source control.
- The plugin must run normally when no telemetry configuration exists.

## Sentry Configuration

Tracker v1.11.0 includes optional Sentry support through the official Sentry .NET SDK.

Recognized settings:

- `SENTRY_DSN`
- `SENTRY_ENVIRONMENT`
- `SENTRY_RELEASE` (example: `optitrack-gh@1.11.0`)
- `SENTRY_TRACES_SAMPLE_RATE`

If `SENTRY_RELEASE` is absent, Tracker defaults to `optitrack-gh@<plugin-version>`.

## Compatibility Diagnostics (Safe Fields Only)

When telemetry is enabled and configured, SDK compatibility diagnostics may include only:

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
- `rhino_major_version`
- `grasshopper_version`

These tags are low-cardinality and sanitized.

## Data That Must Not Be Sent

Do not send:

- Raw OptiTrack/NatNet frame data
- Marker coordinates or point arrays
- Rigid body names
- Skeleton/asset names
- IP addresses
- File paths
- Machine names
- Usernames
- Project/model names
- Rhino document names
- Secrets/tokens

## Performance Monitoring

Performance monitoring remains aggregate-only and optional.

Allowed examples:

- solve/conversion durations
- aggregate frame counts
- reconnect/dropped/skipped counters

## Disable Completely

Telemetry is disabled when:

- component telemetry input is false,
- `SENTRY_DSN` is missing/invalid,
- or local configuration is absent.
