# Example 01: Connect to Motive

## Purpose

Verify live NatNet connectivity and frame flow from Motive into Grasshopper.

## Required Hardware/Software

- Rhino 8 + Grasshopper
- Tracker plugin installed
- OptiTrack Motive with NatNet broadcast enabled

## Component List

- `OptiTrack Stream`

## Step-by-Step Setup

1. Start Motive and confirm tracking is active.
2. Enable NatNet broadcast in Motive.
3. In Grasshopper, add `Tracker > OptiTrack > OptiTrack Stream`.
4. Set:
   - `Connect = true`
   - `Local IP = 127.0.0.1` (single machine) or local NIC IP
   - `Server IP = Motive host IP`
   - `Connection Type = Multicast`
   - `Command Port = 1510`
   - `Data Port = 1511`
5. Enable `Rigid Body` in the component menu if needed.

## Expected Outputs

- Increasing `Frame Number`
- `Status` includes connection success messages
- `Markers` and optional rigid body outputs populate

## Troubleshooting Notes

- No frames: confirm broadcast enabled, IPs/ports, and firewall rules.
- Invalid config warnings: correct IP/port values.
- Missing DLL warnings: confirm `NatNetML.dll` and `NatNetLib.dll` are in plugin folder.

## Telemetry Notes

If enabled, telemetry may report component operation categories and aggregate timing/count values. It must not report marker coordinates, rigid body names, or network identifiers.
