OptiTrack-GH Release Package
===========================

Version: v1.10.0

Install
-------
1. Unzip this package.
2. Copy files from `plugin/` to:
   %APPDATA%\Grasshopper\Libraries\Tracker
3. If NatNet DLLs are not included, install NatNet SDK 4.0 and copy:
   - NatNetML.dll
   - NatNetLib.dll
   next to Tracker.gha.
4. Right-click copied `.gha`/`.dll` files, open Properties, and select Unblock if shown.
5. Restart Rhino and open Grasshopper.

Documentation
-------------
- docs/getting-started.md
- docs/setup.md
- docs/compatibility.md
- docs/sdk-compatibility.md

Telemetry
---------
- Disabled by default.
- Optional Sentry configuration is documented in docs/sentry.md.
- Do not commit Sentry secrets.
