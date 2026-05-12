# Example 03: Marker Cloud Conversion

Goal: convert marker lists into Rhino point clouds with explicit scaling/remapping.

1. Add `Markers To Points`.
2. Feed marker points from stream output.
3. Set:
   - `Scale Factor`: `1.0` for meters or `1000.0` for millimetres
   - `Axis Remap`: `None` or `ZUpToYUp`
4. Optionally provide `World Transform` for fixture/world mapping.
5. Preview output `Points` and pass to downstream analysis.

Telemetry:

- Optional and disabled by default.
- Reports marker counts and conversion duration only.
