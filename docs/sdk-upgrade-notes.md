# SDK Upgrade Notes (v1.11.0)

## Summary

v1.11.0 adds a concrete `NatNetLatestAdapter` path while preserving `NatNet4Adapter`.

Because only one NatNet SDK binary set is available in this repository (`NatNetML.dll`/`NatNetLib.dll` file version `3.0.0.0`), both adapters currently map to the same underlying transport implementation.

## NatNet 4.0 Compatibility Adapter

- Namespace: `OptiTrack.NatNet4Adapter`
- Status: retained and selectable
- Purpose: preserve legacy adapter contract for existing deployments

## Latest Local SDK Adapter

- Namespace: `OptiTrack.NatNetLatestAdapter`
- Status: implemented and selectable
- Purpose: explicit adapter target for the newest SDK artifacts currently available in local environment

## Key Comparison Areas

No API deltas were observed in current local SDK artifacts for the code paths used here:

- connection setup
- local/server IP handling
- multicast/unicast mapping
- frame callback signature
- rigid body/marker/skeleton frame parsing
- data descriptor handling
- frame number/timestamp handling

## Selection Model

Runtime selection via environment variable:

- `TRACKER_NATNET_ADAPTER=latest` -> latest adapter
- otherwise -> natnet4 adapter

## Redistribution and Licensing

Do not remove old SDK files unless licensing or compatibility requires it.
If future replacement/removal is required, document the reason and migration path in release notes and compatibility docs.
