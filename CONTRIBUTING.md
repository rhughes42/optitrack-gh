# Contributing

Thanks for contributing to Tracker.

## Before You Start

1. Review [docs/README.md](docs/README.md).
2. Open an issue for substantial changes before implementation.
3. Keep pull requests focused and incremental.

## Development Expectations

- Preserve live capture behavior unless a change is intentional and documented.
- Keep NatNet-specific code inside `OptiTrack.NatNet`.
- Keep telemetry optional and privacy-safe.
- Add or update docs/examples with feature changes.

## Pull Request Checklist

- [ ] Code compiles in the intended Rhino/Grasshopper environment.
- [ ] `CHANGELOG.md` updated.
- [ ] Docs and examples updated.
- [ ] No secrets/DSNs/tokens committed.
- [ ] Telemetry payloads remain aggregate and sanitized.

## Reporting Security or Privacy Concerns

Use the telemetry/privacy issue template and include reproduction steps without sharing sensitive data.
