# Developer Guide

## Project Layout

- `src/Tracker` contains the Grasshopper plugin project.
- `src/Tracker/OptiTrack/Core` contains SDK-independent domain models and client interfaces.
- `src/Tracker/OptiTrack/NatNet` contains the NatNetML-backed adapter and conversion helpers.
- `src/Tracker/OptiTrack/Telemetry` contains the default-disabled telemetry abstraction.
- `lib/NatNet` contains the current bundled NatNet SDK 4.0 files.

## NatNet Boundary

Grasshopper components should depend on `OptiTrack.Core` models and interfaces instead of `NatNetML` types. Keep direct usage of `NatNetClientML`, `FrameOfMocapData`, `DataDescriptor`, and related SDK classes inside `OptiTrack.NatNet`.

The current adapter is `NatNetOptiTrackClient`, which implements `IOptiTrackClient`.

## Domain Models

Use these models when passing capture data across internal boundaries:

- `OptiTrackFrame`
- `OptiTrackMarker`
- `OptiTrackRigidBody`
- `OptiTrackSkeleton`
- `OptiTrackConnectionOptions`
- `OptiTrackConnectionInfo`
- `OptiTrackConnectionStatus`
- `OptiTrackFrameEventArgs`
- `OptiTrackConnectionEventArgs`

These models may contain motion-capture values for local Grasshopper output. They must not be serialized into telemetry payloads.

## Telemetry

Telemetry is represented by `ITelemetryService` and defaults to `NoOpTelemetryService`. `SentryTelemetryService` is available for optional sanitized reporting, but it must remain disabled unless the component enables telemetry and a valid DSN is configured.

Only send coarse operational fields to telemetry in future integrations. Never send marker coordinates, rigid body names, raw frame payloads, IP addresses, file paths, usernames, machine names, or Rhino document names.

## Conversion Helpers

`NatNetFrameConverter` performs the SDK-to-domain conversion. Keep conversion behavior small and testable. If a test project is added later, cover:

- NatNet frame metadata maps to `OptiTrackFrame`.
- Marker IDs map to marker labels without sending coordinates to telemetry.
- Rigid body position/quaternion values map to `OptiTrackRigidBody`.
- Missing or untracked rigid bodies do not create Grasshopper planes.

## Build

Use the local build command in [build.md](build.md). Rhino and Motive integration still require manual validation on a machine with the relevant runtime dependencies.
