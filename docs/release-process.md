# Release Process

## Branching

1. Create a release branch:
   - `release/vX.Y.Z-short-name`
2. Keep changes scoped and reviewable.

## Version and Docs

Before release:

1. Update `CHANGELOG.md` with the new version section.
2. Update relevant docs and examples.
3. Update assembly version metadata when runtime code changes.

## Validation

1. Build locally with the Rhino/Grasshopper-compatible toolchain.
2. Smoke-test:
   - Live connect component
   - Replay workflow
   - At least one geometry conversion chain
3. Confirm telemetry remains disabled by default.

## Privacy Checklist

Verify no committed files include:

- Sentry DSN or auth tokens
- private org/project names
- real capture recordings with sensitive identifiers
- user paths, machine names, IP lists

## Publish

1. Push branch.
2. Open PR with:
   - scope summary
   - test notes
   - telemetry/privacy notes
3. Merge after review and CI/local validation.
