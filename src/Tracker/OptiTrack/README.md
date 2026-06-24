# OptiTrack Modules

This folder separates SDK-independent contracts/models from NatNet adapter code and telemetry services.

## Navigation

- [Tracker Project](../README.md)
- [Documentation Index](../../../docs/README.md)
- [Architecture Notes](../../../docs/architecture.md)
- [Developer Guide](../../../docs/developer-guide.md)

## Module Areas

- `Core` - domain models, event args, and `IOptiTrackClient` contract.
- `NatNet` - adapter that integrates NatNetML and converts SDK data to `Core` models.
- `Telemetry` - telemetry abstractions and optional Sentry implementation.
