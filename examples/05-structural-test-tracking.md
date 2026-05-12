# Example 05: Structural Test Tracking

## Purpose

Track marker/rigid-body motion during a structural or vibration test and generate stable geometry streams for analysis.

## Required Hardware/Software

- Rhino 8 + Grasshopper
- Tracker plugin
- OptiTrack Motive with calibrated cameras (live mode), or replay recording

## Component List

- `OptiTrack Stream` or `Replay OptiTrack Recording`
- `Markers To Points`
- `Rigid Body To Plane`
- Optional: `Smooth Pose Stream`, `Velocity / Acceleration Estimate`

## Step-by-Step Setup

1. Connect to live stream (or load replay data).
2. Route marker data into `Markers To Points`.
3. Route selected rigid body pose into `Rigid Body To Plane`.
4. Use consistent scale and axis settings.
5. Add smoothing for high-frequency noise if needed.
6. Add velocity/acceleration estimation for dynamic trend checks.

## Expected Outputs

- Continuous point cloud updates for marker sets
- Plane-based rigid-body orientation stream
- Optional velocity and acceleration traces

## Troubleshooting Notes

- Noisy output: use smoothing and verify camera calibration quality.
- Missing markers: inspect occlusion, marker visibility, and labeling stability.
- Unrealistic dynamics: verify timestamp consistency and scale factor.

## Telemetry Notes

If enabled, telemetry may report operation durations and aggregate marker/rigid-body counts. It must not include coordinates, labels, or raw motion payloads.
