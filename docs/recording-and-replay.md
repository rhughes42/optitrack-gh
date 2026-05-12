# Recording and Replay

Tracker v1.7.0 documents offline recording and replay so geometry and calibration workflows can be developed without a live Motive session.

## Components

- `Record OptiTrack Stream`: capture `OptiTrackFrame` objects and save JSON recordings.
- `Load OptiTrack Recording`: load a JSON recording file.
- `Replay OptiTrack Recording`: play/pause/loop/scrub loaded recordings with speed control.
- `Inspect Recording Metadata`: inspect format version, units, source, frame count, and duration.

## File Format (JSON)

Recording files use a structured JSON container:

- `FormatVersion` (string): currently `1.0`
- `Metadata`:
  - `PluginVersion`
  - `RecordedUtc`
  - `Units`
  - `Source`
  - `FrameCount`
  - `DurationSeconds`
- `Frames` (array of `OptiTrackFrame` domain-model payloads)

See [sample-rigid-body-recording.json](/d:/Repos/Archive/optitrack-gh/examples/data/sample-rigid-body-recording.json).

JSON is the primary supported format for full-fidelity replay. CSV export/import may be added later for simplified marker/rigid-body tabular workflows.

## Development Workflow Without Motive

1. Use `Load OptiTrack Recording` with a sample or previously captured file.
2. Connect it to `Replay OptiTrack Recording`.
3. Use replay `Frame` output as input to recording, geometry conversion, filtering, smoothing, and calibration components.
4. Iterate on component chains without requiring NatNet traffic.

## Privacy and Data Awareness

Recording files may contain motion-capture frame payloads (markers, rigid body transforms, and status messages). Treat recordings as potentially sensitive lab data.

- Do not commit real capture sessions to public repositories.
- Use synthetic or anonymized data for examples.
- Prefer lab-scoped storage and access controls for recorded sessions.

## Telemetry Notes

Recording and replay telemetry must remain aggregate-only:

- allowed: frame counts, duration/timing, format version, high-level error category
- prohibited: marker coordinates, rigid body names, file paths, file names, raw frame payloads

Telemetry remains optional and disabled unless explicitly enabled and configured.
