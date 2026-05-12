# Developer Guide

## Repository Areas

- `src/Tracker`: plugin code and `.csproj`
- `src/Tracker/OptiTrack/Core`: transport-neutral models and interfaces
- `src/Tracker/OptiTrack/NatNet`: NatNet adapter layer
- `src/Tracker/OptiTrack/Recording`: JSON recording and replay adapter
- `src/Tracker/OptiTrack/Telemetry`: optional telemetry boundary
- `src/Tracker/Components`: composable Grasshopper components

## Coding Boundaries

- Keep direct `NatNetML` references inside `OptiTrack.NatNet`.
- Keep replay/file format logic inside `OptiTrack.Recording`.
- Keep RhinoCommon/Grasshopper-specific logic in component or tracker UI layers.
- Keep telemetry behind `ITelemetryService`.

## Extension Guidelines

1. Prefer adding small components over expanding one large component.
2. Reuse `OptiTrackFrame` and related models across live/replay paths.
3. Keep coordinate conversion logic centralized in `OptiTrackGeometryConverter`.
4. Add docs/examples with feature changes.
5. Preserve backward-compatible input/output ordering when extending existing components.

## Telemetry Development Rules

- Telemetry is opt-in and disabled by default.
- Never emit raw motion payloads.
- Only emit aggregate counts, durations, and high-level error categories.
- Sanitize tags/context with `TelemetrySanitizer`.

## Build and Validation

- Build notes: [build.md](build.md)
- Runtime validation should cover:
  - Live connection
  - Replay workflow
  - Geometry conversion chain
  - Telemetry-off default behavior

## Release and Contribution

- Release workflow: [release-process.md](release-process.md)
- Contribution guide: `../CONTRIBUTING.md`
