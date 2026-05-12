# OptiTrack Networking

Use this guide to configure Motive/NatNet networking for reliable Tracker connections.

## Required Settings

- Motive must be running.
- NatNet broadcast must be enabled in Motive.
- Tracker `Server IP` must point to the Motive machine.
- Tracker `Local IP` must match the NIC Rhino uses for that route.

## Ports

Default NatNet ports in Tracker:

- `Command Port`: `1510`
- `Data Port`: `1511`

Keep command and data ports different.

## Multicast vs Unicast

- Start with `Multicast` for standard lab setups.
- Use `Unicast` only when your network policy requires it.
- If unclear, confirm with Motive settings and network admin.

## Common Topologies

1. Single machine (Motive + Rhino):
   - `Local IP = 127.0.0.1`
   - `Server IP = 127.0.0.1`
2. Two machines on same LAN:
   - `Local IP = Rhino machine NIC IP`
   - `Server IP = Motive machine NIC IP`

## Firewall Checklist

- Allow Motive and Rhino through Windows Firewall.
- Allow UDP traffic on configured NatNet ports.
- Confirm the selected NIC is not blocked by enterprise policy.

## Quick Diagnostics

- Connection succeeds but no frames:
  - Check Motive broadcast enabled.
  - Confirm IP/port values.
  - Validate firewall rules.
- Intermittent updates:
  - Reduce network load.
  - Lower replay/update rates.
  - Use stable wired networking for live tracking.
