# Licensing

## Project License

This repository currently includes a restrictive top-level `LICENSE` file (`all rights reserved`).

If maintainers want open-source distribution, replace `LICENSE` with an approved open-source license and review all bundled third-party assets first.

## Third-Party SDK Files

Tracker integrates with OptiTrack NatNet and may rely on these files:

- `NatNetML.dll`
- `NatNetLib.dll`
- `NatNetLib.lib`
- `NatNetML.xml`

The legal redistribution status for bundled NatNet binaries should be confirmed directly against OptiTrack/NaturalPoint terms before publishing release archives that include these files.

If redistribution rights are unclear, publish release artifacts without NatNet DLLs and require manual SDK installation.

## Sentry SDK

Tracker references the Sentry .NET SDK via NuGet package dependency.

- Runtime telemetry remains optional and disabled by default.
- Sentry DSN, auth token, org slug, project slug, and release credentials must remain outside source control.
- Release metadata (for example `SENTRY_RELEASE`) should come from environment variables or CI secrets only.
