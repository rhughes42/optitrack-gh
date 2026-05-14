# Tracker Test Design Rules

Use tests to validate observable behavior, not implementation trivia.

Tests should be deterministic, repeatable, clear, and maintainable. Avoid shallow coverage that only proves code executed.

## Assertions

Assertions must be specific and meaningful.

Good:

```csharp
Assert.That(recording.Metadata.FrameCount, Is.EqualTo(2));
Assert.That(recording.Metadata.Units, Is.EqualTo("meters"));
Assert.That(buffer.SkippedCount, Is.EqualTo(1));
Assert.That(exception.ParamName, Is.EqualTo("recording"));
```

Weak:

```csharp
Assert.That(result, Is.Not.Null);
```

Only assert non-null when nullability itself is the contract.

## Exceptions

When behavior should not throw:

```csharp
Assert.DoesNotThrow(() => sut.Stop());
```

When behavior should throw:

```csharp
var exception = Assert.Throws<ArgumentNullException>(() => sut.LoadRecording(null!));

Assert.That(exception!.ParamName, Is.EqualTo("recording"));
```

Do not suppress exceptions silently. Do not weaken tests to accommodate production defects.

## Determinism

Avoid:

- Random values unless seeded.
- Real-time waiting.
- `Thread.Sleep`.
- Timing assumptions.
- Shared mutable state between tests.
- Environment-dependent behavior.
- Ordering assumptions that are not part of the contract.

If time is required, use fixed timestamps or an injectable clock where the production code allows it.

For async tests:

- Return `async Task`, never `async void`.
- Await all asynchronous operations.
- Prefer cancellation tokens and deterministic events over arbitrary delays.
- Keep timeout-based assertions rare and conservative.

## Test Data

Keep test data small, focused, intentional, and readable.

Prefer helper methods or builders when setup becomes repetitive:

```csharp
private static OptiTrackFrame CreateFrame(int frameNumber, double timestampSeconds)
{
	return new OptiTrackFrame(
		frameNumber,
		timestampSeconds,
		0,
		false,
		false,
		Array.Empty<OptiTrackMarker>(),
		Array.Empty<OptiTrackRigidBody>(),
		Array.Empty<string>());
}
```

Use synthetic or anonymized payloads. Do not commit real capture sessions as test fixtures.

For serializer tests, prefer temporary files created by the test and cleaned up afterward. Do not write into source-controlled fixture files unless the user asks for a fixture update.

## Mocking

Mock only external dependencies or boundaries:

- Telemetry services.
- File or clock abstractions if introduced.
- Client interfaces at integration boundaries.

Do not mock:

- Pure value objects.
- Simple DTOs.
- Internal implementation details.

Prefer realistic object construction when practical.

## Regression Tests

When fixing a bug:

1. Reproduce the bug with a failing test first when practical.
2. Fix the implementation.
3. Keep the regression test strict enough to catch the old behavior.

Regression tests should clearly communicate what previously failed and what behavior is now protected.
