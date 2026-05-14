# Optional Sentry Setup (v1.11.0)

Sentry is optional and disabled by default.

## Configuration

Use environment variables or local `tracker.telemetry.local.json` (git-ignored):

- `SENTRY_DSN`
- `SENTRY_ENVIRONMENT`
- `SENTRY_RELEASE`
- `SENTRY_TRACES_SAMPLE_RATE`

Example:

```json
{
  "SENTRY_DSN": "",
  "SENTRY_ENVIRONMENT": "local",
  "SENTRY_RELEASE": "optitrack-gh@1.11.0",
  "SENTRY_TRACES_SAMPLE_RATE": "0"
}
```

If `SENTRY_RELEASE` is not set, Tracker uses `optitrack-gh@<plugin-version>`.

## Compatibility Diagnostics Validation

With telemetry enabled and a test DSN, verify only safe tags are emitted:

- `adapter_name`
- `adapter_version`
- `natnet_assembly_version`
- `sdk_load_result`
- `connection_mode`
- `frame_schema_version`
- `sdk_exception_type`

Do not emit IPs, file paths, machine/user names, marker/rigid-body payloads, raw frames, or project/model data.

## Required Safety Rule

Do not commit DSNs, auth tokens, org slugs, project slugs, or private release config to source control.
