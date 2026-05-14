# Performance and Update Cadence (v1.10.0)

## Live Frame Processing Model

Tracker now decouples NatNet capture from Grasshopper solve frequency:

1. NatNet callback writes each valid frame into a single-slot latest-frame buffer.
2. Grasshopper solves consume the latest available frame.
3. Intermediate frames may be skipped when inbound FPS exceeds solve cadence.

This avoids unbounded queue growth and prevents forcing a full Grasshopper recompute for every incoming frame.

## Controls

- `Target Update Interval (ms)`: cadence target for scheduled solves. Lower values increase update frequency and CPU usage.
- `Redraw Every Frame`: advanced mode that schedules solves per frame. Use only for controlled scenarios.

Recommended starting values:

- Development: `50-100 ms`
- Production/shared definitions: `100-200 ms`
- `Redraw Every Frame`: off

## Diagnostics Output

`Diagnostics` output reports:

- `status`
- `telemetry_status`
- `frames_received`
- `frames_consumed`
- `skipped_frame_count`
- `dropped_frame_count`
- `last_frame_timestamp_utc`
- `buffer_age_ms`
- `uptime_seconds`
- `reconnect_count`
- `last_solve_timestamp_utc`

## Threading Notes

- NatNet callback path does minimal work and stores latest frame only.
- Conversion to Rhino/Grasshopper geometry happens on solve.
- Update scheduling uses Grasshopper document scheduling to avoid UI thread blocking.
- Component deletion triggers disconnect and timer disposal.

