# Grasshopper Components

## OptiTrack Stream

Category: `Tracker`  
Subcategory: `OptiTrack`

Connects to Motive over NatNet and streams OptiTrack data into Grasshopper.

### Inputs

| Input | Default | Description |
| --- | --- | --- |
| `Connect` | `false` | Connect to the Motive NatNet stream. Previously named Activate; existing definitions should retain the same input position. |
| `Reset` | `false` | Disconnect, reset cached frame data, and clear outputs. |
| `Local IP` | `127.0.0.1` | Receiver network adapter IP address. |
| `Server IP` | `127.0.0.1` | Motive server IP address. |
| `Connection Type` | `Multicast` | `Multicast` or `Unicast`. Multicast matches the historical component behavior. |
| `Command Port` | `1510` | NatNet server command port. |
| `Data Port` | `1511` | NatNet server data port. |
| `Scale Factor` | `1.0` | Additional scale applied to marker points and rigid body planes. |
| `Y Up` | `false` | Applies the existing Y-up coordinate adjustment. |
| `Redraw Throttle` | `4` | Processes every Nth NatNet frame. Use `1` for every frame. |
| `Debug Logging` | `false` | Shows extra runtime remarks, including telemetry status. |
| `Enable Telemetry` | `false` | Enables optional sanitized Sentry reporting only when configured. |

### Outputs

| Output | Description |
| --- | --- |
| `Status` | Connection and NatNet status messages. |
| `Markers` | Marker points generated from the latest processed frame. |
| `Labels` | Marker labels. |
| `RB Name` | Rigid body names from Motive. These are local Grasshopper outputs and are never sent to telemetry. |
| `BR Plane` | Rigid body planes. Name preserved for compatibility. |
| `RB Transform` | Rigid body transforms derived from output planes. |
| `Frame Number` | Latest processed NatNet frame number. |
| `Timestamp` | Latest NatNet software timestamp in seconds. |
| `Latency` | Approximate seconds since the NatNet host transmit timestamp. |
| `Warnings` | Validation and runtime warnings. |
| `Telemetry Status` | `disabled`, `active`, or `failed` state. |

### Runtime Messages

The component reports Grasshopper warnings/errors for invalid IP addresses, invalid ports, missing NatNet DLLs, unconfigured telemetry, and successful connection without frame data after several seconds.

### Backwards Compatibility

The first four inputs and first five outputs keep their original order. New inputs and outputs are appended so existing definitions should be migration-friendly.
