# Build

Tracker is a .NET Framework Grasshopper plugin. Local builds are the source of truth because runtime validation requires Rhino, Grasshopper, Motive, and NatNet.

## Local Requirements

- Windows
- Visual Studio 2022 or compatible MSBuild
- .NET Framework 4.8 Developer Pack / targeting pack
- NuGet package restore
- Bundled NatNet files in `lib/NatNet`
- x64 runtime target for Rhino 8 and the bundled NatNet assemblies

## Build Command

From the repository root:

```powershell
msbuild Tracker.sln /restore /p:Configuration=Release /p:Platform="Any CPU"
```

The post-build step copies `Tracker.dll` to `Tracker.gha` and removes the intermediate `.dll`/`.pdb` from the output folder.

## CI Limitations

The GitHub Actions workflow performs a basic restore/build validation on Windows. It does not validate:

- Loading the `.gha` inside Rhino
- Live NatNet streaming
- Motive broadcast settings
- OptiTrack hardware behavior

Run manual integration tests with Rhino, Grasshopper, Motive, and a representative capture network before release.
