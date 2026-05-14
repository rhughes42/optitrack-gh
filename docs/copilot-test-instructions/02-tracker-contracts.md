# Tracker Behavior Contracts

Generated tests must preserve the current Tracker architecture and implementation contracts.

## Project Boundaries

Respect these boundaries from the project docs:

- `OptiTrack.Core` contains transport-neutral models, connection contracts, and buffering.
- `OptiTrack.NatNet` and adapter namespaces contain direct NatNet SDK interaction.
- `OptiTrack.Recording` owns JSON recording, frame snapshots, serialization, and replay.
- `OptiTrack.Geometry` owns Rhino geometry and coordinate conversion logic.
- `OptiTrack.Telemetry` owns optional telemetry, sanitization, and error-reporting boundaries.
- `Tracker.Components` and `TrackerComponent` own Grasshopper/Rhino UI behavior.

Tests should not move responsibilities across these boundaries or require implementation changes that violate them.

## Priority Areas

Prioritize tests for:

1. Public APIs and user-facing behavior.
2. Transport-neutral core models and interfaces.
3. Latest-frame buffering and receive/consume counters.
4. Recording session behavior, cloning, metadata normalization, and duration calculation.
5. Recording serializer load/save behavior and invalid input handling.
6. Replay client lifecycle, frame index clamping, looping, pause/resume, and disconnect behavior.
7. Geometry conversion, axis remapping, calibration transforms, and unit scaling.
8. Telemetry privacy and sanitization rules.
9. Grasshopper component failure messages and non-live workflows where they can be tested without Rhino UI.
10. Regression coverage for fixed bugs.

For each implementation change, aim for one happy-path test, one edge-case test, and one failure-path test when applicable.

## Live OptiTrack and Grasshopper Rules

Unit tests must not require:

- Live OptiTrack hardware.
- Motive.
- Active NatNet network discovery.
- Rhino or Grasshopper runtime interaction.
- External services.
- Internet access.

If a scenario truly requires live infrastructure, mark it explicitly:

```csharp
[Category("Integration")]
[Explicit("Requires live OptiTrack/Motive/Rhino/Grasshopper environment.")]
```

Integration tests must not run during normal unit test execution.

## Telemetry and Privacy Contracts

Telemetry is opt-in and disabled by default.

Tests must protect that telemetry never sends sensitive capture or environment data. Sensitive data includes:

- Raw OptiTrack or NatNet frame payloads.
- Marker coordinates or point arrays.
- Rigid body names.
- Skeleton or asset names.
- IP addresses.
- File paths or file names.
- Machine names.
- Usernames.
- Project, model, Rhino document, or Grasshopper document identifiers.
- Secrets, tokens, or API keys.

Allowed telemetry assertions should focus on aggregate, low-cardinality data such as counts, durations, format versions, adapter names, SDK load results, and high-level error categories.

## Compatibility and Backward Behavior

Preserve existing behavior unless the user explicitly asks to change it.

If behavior is ambiguous:

- Inspect surrounding production code and docs first.
- Infer the safest existing contract.
- Preserve backward compatibility where reasonable.
- Add one concise test comment only if the assumption would otherwise be unclear.

Do not invent new behavior just to make a test convenient.
