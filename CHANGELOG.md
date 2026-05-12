# Changelog

## v1.4.0

- Expanded the OptiTrack Stream component inputs for connection type, NatNet command/data ports, scale factor, Y-up transform, redraw throttle, debug logging, and optional telemetry enablement.
- Added outputs for rigid body transforms, frame number, timestamp, latency, warnings, and telemetry status while preserving the original output order.
- Added validation and clearer Grasshopper runtime messages for invalid IPs, invalid ports, missing NatNet DLLs, no-frame-after-connect cases, and telemetry configuration state.
- Added optional Sentry error reporting through `ITelemetryService`, disabled by default and activated only when explicitly enabled and configured.
- Added sanitized Sentry configuration support for `SENTRY_DSN`, `SENTRY_ENVIRONMENT`, `SENTRY_RELEASE`, `SENTRY_TRACES_SAMPLE_RATE`, and local `tracker.telemetry.local.json`.
- Updated setup, troubleshooting, telemetry, and Grasshopper component documentation.

## v1.3.0

- Added SDK-independent OptiTrack core models and `IOptiTrackClient`.
- Added a NatNet-backed adapter that isolates direct `NatNetML` usage under `OptiTrack.NatNet`.
- Updated the Grasshopper component to consume internal domain models instead of raw NatNet SDK frame types.
- Added a default no-op telemetry abstraction and sanitizer for future privacy-aware error/performance reporting.
- Added developer and architecture documentation, including a Mermaid diagram of the Motive-to-Grasshopper flow.
- Updated assembly metadata and documentation version references to `1.3.0`.

## v1.2.0

- Reorganized the repository around `src/`, `lib/`, `docs/`, and `examples/`.
- Moved bundled NatNet runtime/reference files into `lib/NatNet`.
- Updated project and solution references for the new layout.
- Updated assembly metadata and version references to `1.2.0`.
- Added baseline setup, compatibility, troubleshooting, build, and telemetry documentation.
- Added an initial privacy-aware telemetry/error-reporting policy for future optional Sentry integration.
- Added repository hygiene files including `.editorconfig` updates and CI build validation scaffolding.

## v1.1.0

- Previous public project version before the v1.2.0 repository modernization step.
