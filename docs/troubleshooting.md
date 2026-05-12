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

## Firewall or Network Issues

Windows Firewall or network security tools can block NatNet traffic. Allow Motive and Rhino through the firewall on the relevant private network. On managed networks, confirm multicast or UDP traffic is permitted.

## Multicast vs. Unicast

Tracker currently uses multicast in code. If a network blocks multicast, the component may fail even when IP addresses are correct. Document the deployment network behavior before changing connection mode.

## Grasshopper Plugin Does Not Load

Check these items:

- Rhino version is compatible with the target Grasshopper package.
- `Tracker.gha` is in a Grasshopper Libraries folder.
- NatNet DLLs are next to `Tracker.gha`.
- Files are unblocked.
- Rhino was restarted after installation.
- Grasshopper loading errors do not mention missing .NET Framework 4.8 or missing NatNet assemblies.

## Telemetry Enabled or Disabled

Telemetry/error reporting is disabled by default. In this version there is no runtime Sentry integration. If future builds add it, check for an explicit local setting, config file, or environment variable such as `SENTRY_DSN`. Removing that configuration must fully disable telemetry.

Maintainers using the Codex Sentry plugin for read-only issue review should configure `SENTRY_AUTH_TOKEN` only in their local environment. That token is not required to run Tracker and must not be committed.
