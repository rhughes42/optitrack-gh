# Troubleshooting

## Blocked `.gha` or `.dll` Files

Windows may block files downloaded from the internet. Right-click the `.zip`, `.gha`, or `.dll`, open **Properties**, select **Unblock** if shown, and apply the change. Restart Rhino after unblocking.

## Missing NatNet DLLs

`Tracker.gha` depends on NatNet runtime files. Keep these files next to the `.gha` in the Grasshopper Libraries folder:

- `NatNetML.dll`
- `NatNetLib.dll`

The repository stores the bundled copies in `lib/NatNet`. If redistribution rules are unclear for your deployment, install the official NatNet SDK 4.0 from OptiTrack and use the matching runtime files.

## Motive Is Not Broadcasting

Confirm Motive is running, the tracking project is active, and NatNet streaming is enabled. Tracker listens for a NatNet server; it does not start Motive or create a broadcast.

## Wrong Local or Server IP

`Local IP` should match the network adapter used by Rhino/Grasshopper. `Server IP` should match the Motive machine. The default `127.0.0.1` only works when Motive and Rhino are on the same machine and Motive is configured accordingly.

Tracker now validates IP address format before connecting. A valid-looking IP can still be the wrong network adapter, so compare the component values with Motive and Windows network settings.

## Wrong Command or Data Port

The defaults are NatNet command port `1510` and data port `1511`. If Motive is configured differently, update the component inputs. The command and data ports must be valid TCP/UDP port numbers and should not be the same value.

## Firewall or Network Issues

Windows Firewall or network security tools can block NatNet traffic. Allow Motive and Rhino through the firewall on the relevant private network. On managed networks, confirm multicast or UDP traffic is permitted.

## Multicast vs. Unicast

Tracker supports multicast and unicast through the current NatNet adapter. Multicast remains the default for backwards compatibility. If a network blocks multicast, switch `Connection Type` to `Unicast` and confirm Motive is configured to support it.

## Grasshopper Plugin Does Not Load

Check these items:

- Rhino version is compatible with the target Grasshopper package.
- `Tracker.gha` is in a Grasshopper Libraries folder.
- NatNet DLLs are next to `Tracker.gha`.
- Files are unblocked.
- Rhino was restarted after installation.
- Grasshopper loading errors do not mention missing .NET Framework 4.8 or missing NatNet assemblies.

## Telemetry Enabled or Disabled

Telemetry/error reporting is disabled by default. In v1.7.0 Sentry is available only when `Enable Telemetry` is `true` and `SENTRY_DSN` or `tracker.telemetry.local.json` is configured. Removing that configuration or setting `Enable Telemetry` to `false` disables reporting.

Maintainers using the Codex Sentry plugin for read-only issue review should configure `SENTRY_AUTH_TOKEN` only in their local environment. That token is not required to run Tracker and must not be committed.

If telemetry is enabled but not configured, the component reports telemetry as disabled and continues normally.
