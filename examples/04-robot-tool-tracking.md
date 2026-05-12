# Example 04: Robot Tool Tracking

Goal: align tracked tool poses to robot/world coordinates with calibration and filtering.

1. Use `OptiTrack Stream` with rigid body output enabled.
2. Use `Filter Rigid Bodies` to select the tool tracker.
3. Use `Calibrate OptiTrack Frame` to compute source-to-target transform.
4. Use `Apply OptiTrack Transform` to map planes/transforms into robot space.
5. Optionally add `Smooth Pose Stream` for display stability.
6. Use `Velocity / Acceleration Estimate` on tool-tip points for dynamic checks.

Notes:

- Keep calibration transforms explicit and versioned in your GH definition.
- Validate against known robot fixture points after each calibration update.
