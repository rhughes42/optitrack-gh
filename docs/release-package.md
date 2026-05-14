# Release Package Layout (v1.11.0)

Example package output:

```text
OptiTrack-GH-v1.11.0/
  plugin/
    Tracker.gha
    NatNetML.dll                 (optional; include only when redistribution is confirmed)
    NatNetLib.dll                (optional; include only when redistribution is confirmed)
    sdk-manual-install/README.txt (present when NatNet DLLs are not bundled)
  README.txt
  LICENSE
  docs/
  examples/
```

Notes:

- Keep layout simple for Grasshopper users: plugin files in one folder.
- Include docs and examples in every public release zip.
- Do not include Sentry DSNs, auth tokens, org/project slugs, or private release config.

