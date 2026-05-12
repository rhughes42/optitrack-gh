# Example 03: Marker Cloud Conversion

## Purpose

Convert marker payloads into Rhino `Point3d` data for debugging, proximity analysis, or point-driven logic.

## Required Hardware/Software

- Rhino 8 + Grasshopper
- Tracker plugin
- Live or replay frame source

## Component List

- `OptiTrack Stream` or `Replay OptiTrack Recording`
- `Markers To Points`

## Step-by-Step Setup

1. Add `Markers To Points`.
2. Feed marker positions from your frame pipeline.
3. Configure:
   - `Scale Factor` (`1.0` for meters, `1000.0` for millimeters)
   - `Axis Remap` (`None` or `ZUpToYUp`)
4. Optionally apply a world transform for fixture/world context.
5. Preview points in Rhino.

## Expected Outputs

- Point list count matches marker count
- Geometry updates at frame rate/replay rate

## Troubleshooting Notes

- Empty output: verify source frame has marker data.
- Mirrored/flipped cloud: check axis remap and Y-up assumptions.
- Incorrect scale: apply a single consistent scale strategy.

## Telemetry Notes

If enabled, telemetry may report marker counts and conversion timings only. Marker coordinates must never be included.
