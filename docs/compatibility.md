# Compatibility

Tracker is a Rhino/Grasshopper plugin for OptiTrack Motive through NatNet. This matrix records the expected and tested combinations. Rows marked "TBD" need validation on real capture systems.

| Area | Current / Expected | Tested | Notes |
| --- | --- | --- | --- |
| Rhino version | Rhino 8 | TBD | Project references the Grasshopper NuGet package `8.2.23346.13001`. |
| Grasshopper version | Rhino 8 bundled Grasshopper | TBD | Plugin loads as a `.gha`. |
| Windows version | Windows 10/11 | TBD | NatNet and Rhino plugin workflow are Windows-oriented. |
| Motive version | Compatible with NatNet SDK 4.0 | TBD | Confirm against the Motive version used in deployment. |
| NatNet SDK version | 4.0 | TBD | `NatNetML.dll` is referenced from `lib/NatNet`. |
| .NET target framework | .NET Framework 4.8, x64 build target | Build validation pending | Required by `src/Tracker/Tracker.csproj`; x64 matches Rhino 8 and bundled NatNet runtime files. |
| Sentry SDK version | Not currently included | N/A | Future optional integration only. |

## Notes

- Do not assume a Motive/NatNet upgrade is compatible without testing a live stream.
- Keep the NatNet managed DLL and native DLL together with the built `Tracker.gha`.
- If Sentry is added later, document the exact SDK version and default disabled behavior here.
