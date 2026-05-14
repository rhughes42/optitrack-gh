# Copilot NUnit Test Generation Instructions

Use these files as workspace references when asking GitHub Copilot to generate or update tests for `Tracker.Tests`.

## Primary References

Reference these files in the C# Dev Kit or C# Dev Tools test-generation screen:

- `docs/copilot-test-instructions/01-nunit-style.md`
- `docs/copilot-test-instructions/02-tracker-contracts.md`
- `docs/copilot-test-instructions/03-test-design-rules.md`
- `docs/copilot-test-instructions/04-validation-workflow.md`
- `tests/Tracker.Tests/Tracker.Tests.csproj`
- `.editorconfig`

Also reference the production file or subsystem being tested. Add these project docs when relevant:

- `docs/developer-guide.md`
- `docs/architecture.md`
- `docs/recording-and-replay.md`
- `docs/telemetry.md`
- `docs/optitrack-networking.md`
- `docs/coordinate-systems.md`

## Prompt Template

Paste this into the Copilot test-generation instructions field, then add the file references listed above:

```text
Generate NUnit tests for the Tracker.Tests project.

Follow the referenced Tracker test instruction files exactly. Inspect the related production code, nearby project docs, .editorconfig, and Tracker.Tests.csproj before writing tests.

Prioritize observable behavior, regression protection, deterministic execution, and clear failure diagnostics. Do not require live OptiTrack hardware, Motive, Rhino, Grasshopper, network discovery, internet access, or real timing dependencies for unit tests.

Use NUnit with Assert.That assertions. Match the repository's C# style, naming, tabs, nullable context, and local architecture boundaries. Prefer a small set of high-value tests over broad shallow coverage.

After generating tests, run Tracker.Tests first, then broader tests if practical. Do not weaken assertions or delete existing tests to make the run pass. If behavior is ambiguous, state the assumption and preserve the safest existing contract.
```

## Typical Reference Set

For a pure core-model, recording, replay, telemetry, or utility test request, use:

```text
docs/copilot-test-instructions/01-nunit-style.md
docs/copilot-test-instructions/02-tracker-contracts.md
docs/copilot-test-instructions/03-test-design-rules.md
docs/copilot-test-instructions/04-validation-workflow.md
tests/Tracker.Tests/Tracker.Tests.csproj
.editorconfig
docs/developer-guide.md
docs/architecture.md
<production-file-under-test>
```

For telemetry-sensitive changes, also add:

```text
docs/telemetry.md
src/Tracker.Telemetry/Telemetry/TelemetrySanitizer.cs
```

For recording or replay changes, also add:

```text
docs/recording-and-replay.md
examples/data/sample-rigid-body-recording.json
```
