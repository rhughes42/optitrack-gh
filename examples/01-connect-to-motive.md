# Example: Connect to Motive

1. Start Motive and confirm NatNet streaming is enabled.
2. In Grasshopper, place `Tracker > OptiTrack > OptiTrack Stream`.
3. For a local Motive instance, keep:

   - `Local IP`: `127.0.0.1`
   - `Server IP`: `127.0.0.1`
   - `Connection Type`: `Multicast`
   - `Command Port`: `1510`
   - `Data Port`: `1511`
   - `Redraw Throttle`: `4`

4. Set `Connect` to `true`.
5. Enable `Rigid Body` from the component context menu if rigid body planes/transforms are needed.
6. Watch `Status`, `Warnings`, `Frame Number`, and `Latency`.

Telemetry is not required. Leave `Enable Telemetry` as `false` unless a deployment owner has configured Sentry.
