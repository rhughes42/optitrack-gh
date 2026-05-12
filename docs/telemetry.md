# Telemetry and Error Reporting Policy

Tracker must remain usable without telemetry. Error reporting is optional and disabled unless explicitly configured by the user or deployment owner.

## Defaults

- Telemetry is optional.
- Error reporting is disabled by default.
- No Sentry DSN is hard-coded.
- No secrets, API keys, auth tokens, or organization/project-specific Sentry settings are stored in source control.
- The plugin must run normally when no telemetry configuration exists.

## Future Sentry Configuration

If Sentry support is added later, configuration should come from one of these sources:

- Environment variables
- A local config file excluded from source control
- A user setting controlled by the user or deployment owner

Recognized placeholder settings:

- `SENTRY_DSN` - optional DSN; telemetry remains disabled when absent or empty.
- `SENTRY_ENVIRONMENT` - optional environment name such as `development`, `lab`, or `production`.
- `SENTRY_RELEASE` - optional release identifier, for example `tracker@1.2.0`.
- `SENTRY_TRACES_SAMPLE_RATE` - optional numeric sample rate for aggregate performance telemetry.

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

Performance monitoring, if enabled, should use aggregate spans and counters only. Acceptable examples include:

- Component solve duration buckets
- Connection attempt counts
- Sanitized error category counts
- NatNet connection state transitions without IP addresses or asset names

Do not attach frame payloads or per-marker/per-rigid-body values.

## Disable Completely

Telemetry must be disabled when `SENTRY_DSN` is missing, empty, or explicitly disabled by a local setting. Users should also be able to remove any local telemetry config file or clear related environment variables to disable reporting completely.
