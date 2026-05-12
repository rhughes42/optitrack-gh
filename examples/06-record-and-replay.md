# Example 06: Record and Replay

## Purpose

Capture a short stream session and replay it offline to test geometry and calibration graphs without Motive.

## Required Hardware/Software

- Rhino 8 + Grasshopper
- Tracker plugin
- Optional live Motive stream for recording phase

## Component List

- `OptiTrack Stream` (record phase) or sample data
- `Record OptiTrack Stream`
- `Load OptiTrack Recording`
- `Replay OptiTrack Recording`
- `Inspect Recording Metadata`

## Step-by-Step Setup

1. Connect live stream or prepare sample frame source.
2. Add `Record OptiTrack Stream`.
3. Set `Record = true` and feed frame input.
4. After collecting frames, set `Record = false`.
5. Set output path and toggle `Save = true` once.
6. Load saved JSON with `Load OptiTrack Recording`.
7. Feed recording object to `Replay OptiTrack Recording`.
8. Set `Play = true`; adjust `Speed`, `Loop`, and `Scrub Index`.
9. Use `Inspect Recording Metadata` to verify file metadata.

## Expected Outputs

- JSON recording file with metadata and frame array
- Replay frame updates without live NatNet connection
- Metadata summary with frame count and duration

## Troubleshooting Notes

- Save failure: verify write permissions and valid output path.
- Load failure: confirm JSON structure matches Tracker format.
- Replay does not start: ensure recording has at least one frame.

## Telemetry Notes

If enabled, telemetry may report recording start/stop events, frame counts, format version, replay timing, and high-level error types. It must not include file path, filename, raw frame payloads, coordinates, or rigid body names.
