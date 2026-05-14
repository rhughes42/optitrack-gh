# Test Generation Workflow

Follow this workflow before and after generating tests.

## Before Writing Tests

1. Inspect the production code being tested.
2. Inspect `tests/Tracker.Tests/Tracker.Tests.csproj`.
3. Inspect `.editorconfig`.
4. Inspect related project docs, especially `docs/developer-guide.md` and `docs/architecture.md`.
5. Identify the real behavior contract.
6. Generate the minimum high-value set of tests.

Do not generate tests from guesses alone when the relevant source file is available.

## After Writing Tests

Run focused tests first:

```powershell
dotnet test tests/Tracker.Tests/Tracker.Tests.csproj
```

Then run broader solution tests when practical:

```powershell
dotnet test
```

If this repository cannot run through `dotnet test` because of .NET Framework, Rhino/Grasshopper, NatNet, or SDK limitations, report the exact failure and run the most specific viable validation command.

## Failure Triage

Every failing test must be analyzed explicitly:

- What failed.
- Why it failed.
- Whether it is a production defect, test defect, missing dependency, environmental limitation, or ambiguous behavior.
- What changed to resolve it.

Do not:

- Delete existing tests to make the run pass.
- Weaken assertions without explaining why the asserted contract was wrong.
- Hide failures behind broad `try/catch`.
- Add sleeps or timing tolerances without proving they are necessary.

## Production Defects Found While Testing

If a legitimate implementation defect is discovered:

- Fix the production code when in scope.
- Keep the strict regression test.
- Preserve architecture boundaries.
- Avoid unrelated refactors.
- Document any ambiguity in the final response.

## Final Output Expectations

Summarize:

- What tests were added or changed.
- Which behavior contracts are now protected.
- Which validation commands were run.
- Any remaining test gaps or environmental limits.

The final test suite should increase confidence during refactors, SDK upgrades, parser changes, networking changes, recording/replay changes, telemetry changes, and future performance work.
