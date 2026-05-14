# SDK Upgrade Notes (v1.9.0)

## NatNet 4.0 Current Adapter

- Namespace/folder: `OptiTrack.NatNet4Adapter`
- Runtime class: `NatNet4OptiTrackClient`
- Delegates to: `OptiTrack.NatNet.NatNetOptiTrackClient`
- Status: active and supported baseline

## Latest SDK Candidate Adapter

- Namespace/folder placeholder: `OptiTrack.NatNetLatestAdapter`
- Current status: placeholder only; not enabled in runtime path
- Rule: do not claim support until verification checklist is completed

## Breaking API Changes to Check

When evaluating a newer NatNet SDK, validate:

- `NatNetClientML` constructor and lifecycle behavior
- `ConnectParams` shape and required fields
- Frame callback signature (`OnFrameReady`) and event semantics
- `FrameOfMocapData` fields consumed by converter
- Data descriptor APIs and descriptor subtype casts
- Connection/disconnection behavior and error codes

## Connection Behavior Checks

- Connect/disconnect success and failure paths
- Multicast vs unicast behavior
- Reconnect stability and cleanup correctness
- No orphaned callbacks/timers/tasks after disconnect

## Frame Data Model Checks

- Marker count and marker access APIs
- Rigid body payload fields and tracking flags
- Timestamp/frame numbering continuity
- Descriptor refresh when tracking models change

## Licensing and Redistribution

Before bundling a new SDK version, verify:

- redistribution rights for managed and native binaries
- any new required runtime files
- documentation and attribution requirements

Do not commit updated SDK binaries without confirming legal redistribution terms.
