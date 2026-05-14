# Release Process

## Branching

1. Create a release branch:
   - `release/vX.Y.Z-short-name`
2. Keep changes scoped and reviewable.

## Version and Docs

Before release:

1. Update `CHANGELOG.md` with the new version section.
2. Update `src/Tracker/Properties/AssemblyInfo.cs` version fields.
3. Update README/docs version references where needed.
4. Verify SDK compatibility docs remain accurate.

## Build and Package

1. Build locally with Rhino/Grasshopper-compatible toolchain.
2. Create package zip with:
   - `pwsh ./build/package.ps1 -Version 1.11.0 -Configuration Release`
3. If NatNet redistribution status is confirmed, optionally include SDK DLLs:
   - `pwsh ./build/package.ps1 -Version 1.11.0 -Configuration Release -IncludeBundledNatNet`
4. Ensure package includes docs and examples.

## Stable v1.x Release Checklist

1. Clean clone.
2. Build.
3. Install into Grasshopper Libraries.
4. Unblock copied files in Windows Properties when required.
5. Launch Rhino.
6. Load Grasshopper.
7. Test replay mode with sample recording.
8. Test live Motive connection if hardware is available.
9. Verify telemetry disabled by default.
10. Verify optional Sentry test event only when configured.
11. Verify docs links in README and docs index.
12. Create GitHub release and upload zip artifact.

## Final Manual QA Checklist

1. Component loads.
2. No missing DLL errors.
3. Replay sample works.
4. Diagnostics visible.
5. Telemetry disabled by default.
6. Sentry enabled only when configured.
7. Documentation opens from release package.

## Privacy and Secrets Checklist

Verify no committed files include:

- Sentry auth tokens, DSNs, org slugs, or project slugs
- private release configuration
- real capture recordings with sensitive identifiers
- user paths, machine names, or IP lists

Sentry release metadata must come only from environment variables or CI secrets.

