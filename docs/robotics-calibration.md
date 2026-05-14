# Robotics Calibration Workflow

This workflow is intended for robotics/fabrication experiments where OptiTrack data must be aligned to a robot or tool frame.

## Baseline Workflow

1. Stream live data with `OptiTrack Stream`.
2. Convert rigid body outputs to geometry:
   - `RB Plane` output directly from stream, or
   - `Rigid Body To Plane` from explicit pose values.
3. Capture a known source frame in OptiTrack space and a matching target frame in robot/world space.
4. Build calibration transform with `Calibrate OptiTrack Frame`.
5. Apply the transform with `Apply OptiTrack Transform`.
6. Validate by moving a tracked tool to known checkpoints and measuring residual offsets.

## Recommended Components

- `Filter Rigid Bodies` for stable selection of relevant trackers.
- `Smooth Pose Stream` to reduce jitter in real-time display/control branches.
- `Velocity / Acceleration Estimate` for feed-rate checks and dynamic constraints.

## Practical Guidance

- Keep calibration snapshots reproducible: save source and target planes.
- Re-run calibration after camera layout changes, tracker remounting, or coordinate system edits.
- Use explicit scale values and keep them near the conversion/calibration components.
- Keep control and visualization branches separate so smoothing choices are intentional.

## Telemetry and Privacy

Telemetry instrumentation for calibration components reports aggregate timings and counts only. It does not send coordinates, rigid body names, or geometry payloads.
