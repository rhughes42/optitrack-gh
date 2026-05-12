# Changelog

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
