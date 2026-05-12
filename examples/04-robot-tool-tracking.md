# Example 04: Robot Tool Tracking

## Purpose

Map OptiTrack rigid body poses into robot/workcell coordinates for experimental tool tracking.

## Required Hardware/Software

- Rhino 8 + Grasshopper
- Tracker plugin
- Live Motive stream or replayed capture
- Robot/world reference frame definition

## Component List

- `OptiTrack Stream` or `Replay OptiTrack Recording`
- `Filter Rigid Bodies`
- `Rigid Body To Plane`
- `Calibrate OptiTrack Frame`
- `Apply OptiTrack Transform`
- Optional: `Smooth Pose Stream`, `Velocity / Acceleration Estimate`

## Step-by-Step Setup

1. Stream or replay rigid body data.
2. Filter to the tool rigid body branch.
3. Convert to plane or transform representation.
4. Build calibration transform from known source and target frames.
5. Apply calibration transform to tool pose output.
6. Optionally smooth output for visualization stability.
7. Optionally compute velocity/acceleration from tool-tip points.

## Expected Outputs

- Tool pose aligned with workcell/world reference
- Stable transformed planes/transforms for downstream robot logic

## Troubleshooting Notes

- Pose drift: re-check calibration reference pairs.
- Inverted orientation: verify coordinate convention and remap choices.
- No rigid body output: confirm rigid body stream toggle and tracking quality.

## Telemetry Notes

If enabled, telemetry may report operation durations and aggregate rigid-body counts. It must not include rigid body names, coordinates, or calibration payloads.
