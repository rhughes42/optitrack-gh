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
6. For live streams, prefer latest-frame buffering over queued per-frame processing.
7. Do not call `ExpireSolution(true)` per solve in streaming loops; use controlled scheduling cadence.

## Telemetry Development Rules

- Telemetry is opt-in and disabled by default.
- Never emit raw motion payloads.
- Only emit aggregate counts, durations, and high-level error categories.
- Sanitize tags/context with `TelemetrySanitizer`.

## Build and Validation

- Build notes: [build.md](build.md)
- Runtime validation should cover:
  - Live connection
  - Buffered capture vs. solve cadence behavior
  - Replay workflow
  - Geometry conversion chain
  - Telemetry-off default behavior
  - Safe component deletion/disposal behavior

## v1.10.0 Manual Validation Checklist

The repository currently does not include a standalone unit test project for Tracker internals. For v1.10.0, run this manual validation:

1. Live stream, `Target Update Interval (ms)=100`, `Redraw Every Frame=false`; verify Grasshopper stays responsive while `frames_received` grows faster than `frames_consumed`.
2. Toggle `Redraw Every Frame=true`; verify higher update frequency and expected CPU increase.
3. Delete the component from canvas while connected; verify no orphan updates continue and no repeated connection events appear.
4. Replay recording; verify timing progression and `Late Frames` behavior.
5. Enable telemetry without DSN; verify `Telemetry Status` remains disabled and no failures block use.
6. Enable telemetry with DSN and low sample rate; verify only aggregate spans/metrics are emitted.

## Release and Contribution

- Release workflow: [release-process.md](release-process.md)
- Contribution guide: `../CONTRIBUTING.md`

