# NUnit Style for TrackerTests

You are adding or updating tests in the `TrackerTests` project.

The objective is not only coverage. The objective is to protect real behavior, catch regressions early, validate contracts clearly, and make failures fast to diagnose.

Prefer fewer high-value tests over large quantities of shallow coverage. Every test should answer: what behavior contract is being protected?

## Framework

Use NUnit.

Preferred attributes:

```csharp
[TestFixture]
[Test]
[TestCase]
[SetUp]
[TearDown]
```

Use `Assert.That(...)` consistently.

Preferred:

```csharp
Assert.That(result.Success, Is.True);
```

Avoid legacy assertion styles such as `Assert.AreEqual(...)` unless they are already dominant in the surrounding file.

## Names

Name test classes after the production type or subsystem:

```csharp
LatestFrameBufferTests
OptiTrackRecordingSessionTests
OptiTrackReplayClientTests
TelemetrySanitizerTests
UtilitiesTests
```

Use behavior-based test method names:

```csharp
MethodOrFeature_WhenCondition_ShouldExpectedBehavior()
```

Examples:

```csharp
TryConsumeLatest_WhenNoFrameWasWritten_ShouldReturnFalse()
BuildRecording_WhenUnitsAreBlank_ShouldUseMeters()
LoadRecording_WhenRecordingIsNull_ShouldThrowArgumentNullException()
SanitizeTagValue_WhenKeyIsSensitive_ShouldReturnRedactedValue()
```

Avoid vague names such as `BasicTest`, `ParserWorks`, or `TestConnection`.

## Layout

Use explicit Arrange, Act, Assert sections for non-trivial tests.

```csharp
[Test]
public void Method_WhenCondition_ShouldExpectedResult()
{
	// Arrange
	var sut = new LatestFrameBuffer();

	// Act
	var consumed = sut.TryConsumeLatest(out var frame, out _, out var skippedIntermediateFrames);

	// Assert
	Assert.That(consumed, Is.False);
	Assert.That(frame, Is.Null);
	Assert.That(skippedIntermediateFrames, Is.False);
}
```

Keep tests small and focused. One test may assert several directly related facts about the same behavior contract, but avoid mixing unrelated contracts.

## Repository Style

Match the existing repository conventions:

- Use tabs for C# indentation.
- Keep nullable warnings in mind; do not hide nullability issues with broad suppression.
- Use `var` when the type is obvious from the right-hand side.
- Use explicit types when they improve readability.
- Keep comments sparse and focused on intent or non-obvious assumptions.
- Avoid clever fixtures, deep inheritance, and hidden setup.

Tests should read like executable documentation for a production-critical SDK or infrastructure component.
