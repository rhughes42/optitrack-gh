# Example 02: Rigid Body Plane Conversion

Goal: convert a rigid body pose into a Rhino plane for downstream geometry/robotics use.

1. Add `Rigid Body To Plane`.
2. Connect:
   - `Origin`: rigid body position point
   - `Quaternion WXYZ`: list of four values
3. Set `Scale Factor` to `1.0` for meters or `1000.0` for millimetres.
4. Toggle `Y Up` if your downstream coordinate convention expects it.
5. Use output `Plane` for toolpath alignment, fixture checks, or transform derivation.

Telemetry:

- Keep `Enable Telemetry` false by default.
- When enabled/configured, only aggregate operation duration and count metadata is reported.
