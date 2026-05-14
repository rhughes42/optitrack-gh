# SDK Migration Notes (v1.11.0)

## From v1.10.0 to v1.11.0

Changes:

- Added concrete `OptiTrack.NatNetLatestAdapter` implementation.
- Kept `OptiTrack.NatNet4Adapter` intact.
- Added runtime adapter selection with `TRACKER_NATNET_ADAPTER`.
- Expanded compatibility diagnostics and safe telemetry tags.

Default behavior remains legacy adapter mode unless `TRACKER_NATNET_ADAPTER=latest` is set.

## From NatNet 4.0 Adapter to Latest Adapter

1. Keep existing deployment unchanged first.
2. Set `TRACKER_NATNET_ADAPTER=latest` in a test environment.
3. Validate:
   - live connection,
   - rigid body stream,
   - marker stream,
   - reconnect behavior,
   - replay mode compatibility.
4. Review `Inspect SDK Compatibility` output and ensure expected adapter/assembly values.
5. Keep telemetry disabled unless validating optional Sentry diagnostics.

## Rollback

- Remove/unset `TRACKER_NATNET_ADAPTER` to return to `NatNet4Adapter`.
- No model/schema migration is required for replay files in this step.
