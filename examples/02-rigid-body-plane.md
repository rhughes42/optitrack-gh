# Example 02: Rigid Body Plane Conversion

## Purpose

Convert a rigid body pose into a Rhino `Plane` for fixture alignment, robot target definition, or transform chaining.

## Required Hardware/Software

- Rhino 8 + Grasshopper
- Tracker plugin
- Either live stream data or replay recording

## Component List

- `OptiTrack Stream` or `Replay OptiTrack Recording`
- `Rigid Body To Plane`

## Step-by-Step Setup

1. Obtain pose values (`origin`, `quaternion`) from live or replay workflows.
2. Add `Rigid Body To Plane`.
3. Connect `Origin` and `Quaternion WXYZ`.
4. Set `Scale Factor`:
   - `1.0` for meter workflow
   - `1000.0` for millimeter workflow
5. Toggle `Y Up` only when your downstream graph expects that remap.

## Expected Outputs

- Valid `Plane` aligned with rigid body orientation
- Stable orientation under consistent input quaternions

## Troubleshooting Notes

- Invalid quaternion length: ensure exactly four numbers in `W,X,Y,Z` order.
- Unexpected orientation: verify quaternion convention and Y-up expectations.
- Wrong scale: confirm unit convention near conversion stage.

## Telemetry Notes

If enabled, telemetry may report conversion duration and operation counts only. Pose coordinates and names must not be reported.
