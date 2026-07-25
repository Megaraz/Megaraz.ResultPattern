# Test Plan

This document is the executable-specification backlog for
`Megaraz.ResultPattern.Tests`. Test names should describe observable behavior,
not private implementation. Add rows when a new public API, invariant, or
documented behavior is introduced. A test is complete when its status is
changed from `Planned` to `Implemented`.

## Conventions

- Use xUnit `[Fact]` for one behavior and `[Theory]` with `[InlineData]` or
  `[MemberData]` when the same contract has meaningful input variants.
- Assert all observable state relevant to the rule: result state, code, error
  type, descriptions, user messages, fields, and exception parameter names
  where they are API behavior.
- Test each factory independently. Do not rely only on a test of a shared
  private helper.
- Do not test private implementation, XML documentation, or compiler-generated
  record members beyond the value semantics exposed by the public API.
- Keep test input creation in `TestData`; do not hide the behavior under test
  behind general-purpose test builders.
- Each test runs for every target framework defined by the test project.

## Proposed Test Layout

```text
Megaraz.ResultPattern.Tests/
  TestData.cs
  Error/
    ErrorTests.cs
    ErrorCodeTests.cs
    ErrorContextTests.cs
    ErrorReasonCodeCompatibilityTests.cs
    ValidationErrorTests.cs
  Results/
    ResultTests.cs
    ResultOfTTests.cs
    ResultExtensionsTests.cs
  Validation/
    ValidatorExtensionsTests.cs
```

`TestData` should contain only reusable, deterministic values: a normal
`ErrorContext`, a context with a field, a `ValidationError`, and a normal
non-validation `Error`. Individual test classes should create values inline
when the value itself explains the scenario. No inheritance hierarchy is
needed: test classes remain independent and use `TestData` through static
members. Place fixture-only derived errors, such as a deliberately invalid
plain validation `Error`, as private nested types in the test class that uses
them.

## Test Catalog

| Planned test class | Source surface | Rules / test cases | Status |
| --- | --- | --- | --- |
| `ErrorTests` | `Error`, `ErrorType` | **Custom factories:** preserve supplied code, description, type, user message, and exception; replace a null user message with an empty string; accept caller-owned `ErrorCode`; generate the conventional code for context-and-reason input. **Invariants:** reject null/empty/whitespace code and description; reject `ErrorType.None`, `ErrorType.Validation`, and undefined error-type values; reject a null `ErrorCode` and null context. **Convenience factories:** `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`, and `Cancelled` each produce their documented type, conventional reason/code, technical description, and default user message. Verify field-aware unauthorized wording and field-free wording. Verify `Failure` uses its default description, preserves an explicit description, exception, and custom user message. **Sentinel and value behavior:** `Error.None` has the empty/default state and is the shared sentinel; equal records compare equal and a changed value does not. **Formatting:** `ToString()` returns a concise `Code: Description` diagnostic representation. | Implemented |
| `ErrorCodeTests` | `ErrorCode`, `ErrorCodeReasons`, `OperationType` | **Caller-owned codes:** `From` preserves arbitrary non-blank text exactly, including text that is not conventional. Reject null, empty, and whitespace codes. **Conventional codes:** `For` combines every defined operation with entity and reason using `Operation.Entity.Reason`. Cover all `OperationType` values, including `Custom` and `Login`. Reject null context; blank entity/reason; and entity/reason parts containing `.`. Reject undefined operation values. **Value behavior:** equal codes compare equal; differing codes do not. **Reason constants:** built-in factory tests must assert the corresponding `ErrorCodeReasons` value rather than duplicate string literals where practical. | Implemented |
| `ErrorContextTests` | `ErrorContext` | Record equality includes operation, entity, and field; `with` creates the expected changed copy; a null field remains supported. Do not add validation tests for entity or field until the library intentionally defines those rules. | Implemented |
| `ErrorReasonCodeCompatibilityTests` | Obsolete `ErrorReasonCode` overload and internal conversion | For every defined legacy reason, `ErrorCode.For(context, legacyReason)` produces the documented modern reason/code. Undefined legacy enum values throw `ArgumentOutOfRangeException` with parameter `reason`. Suppress the obsolete warning locally around only these tests. | Implemented |
| `ValidationErrorTests` | `ValidationError`, `ValidationErrorType` | **Custom factories:** the conventional custom overload creates `Custom` with the `Custom` conventional reason; the caller-owned-code overload preserves the supplied code; `CustomWithReason` uses the supplied conventional reason. Each preserves description and resolves user message to the supplied value or description. **Field precedence:** explicit field overrides context field; absent explicit field uses context field; both may be null. **Built-in factories:** `Required`, `InvalidFormat`, `OutOfRange`, `TooShort`, `TooLong`, `AlreadyExists`, and `NonMatchingValues` each expose `ErrorType.Validation`, the matching `ValidationErrorType`, the expected conventional reason/code, resolved field, technical description, and default/custom user message. Cover field-present and field-absent message branches. For `NonMatchingValues`, cover both supplied field labels, missing confirmation field, and explicit custom user message. **Argument propagation:** every factory rejects a null context; caller-owned custom rejects null code; `CustomWithReason` rejects an invalid reason through `ErrorCode.For`. **Value behavior:** equal validation errors compare equal and a changed validation-specific property does not. | Implemented |
| `ResultTests` | non-generic `Result` | **Success:** is successful/not failed, has empty message and validation errors, and uses `Error.None` as primary error. **Normal failures:** `Failure(error)` uses the error user message; overload with message preserves explicit message; both have failed state, empty validation errors, and the supplied primary error. Reject null error and null explicit message. Reject validation errors passed to normal failure. **Validation failures:** preserve error order; select the first error as primary; use default message for null, empty, and whitespace message and preserve a non-blank custom message; reject null collection, empty collection, and null members. **Isolation:** modifying the source error array after creation does not change exposed validation errors. | Implemented |
| `ResultOfTTests` | `Result<TValue>` | Repeat all state, message, primary-error, validation, invalid-input, and source-collection-isolation rules from `ResultTests` for generic failures. **Success values:** successful value is returned for reference and value types; null reference input is rejected. **Failure access:** accessing `Value` on normal and validation failures throws `InvalidOperationException`. Use representative `string`, `int`, and a nullable-suppressed null reference input to exercise the generic contract without duplicating every test for every type. | Implemented |
| `ResultExtensionsTests` | `ResultExtensions.Map` | **Success mapping:** invokes mapper once, returns mapped value, and creates a successful `Result<TOut>`. **Failure preservation:** does not invoke mapper and preserves message, primary-error identity/value, and validation errors for normal and validation failures. **Error behavior:** reject null result and mapper; propagate an exception thrown by mapper unchanged; reject a mapper result of null under the non-null output contract. | Implemented |
| `ValidatorExtensionsTests` | `ValidatorExtensions` | **`RequiredFieldsAreNullOrWhiteSpace`:** null source/context throws; empty and all-valid input return false with an empty list; null, empty, whitespace, and mixed input return true and one ordered required error per failing field; returned errors use the supplied tuple field name rather than the context field. **`IsNullOrWhiteSpace` with field:** null/empty/whitespace return true and produce a required error; non-whitespace returns false; supplied non-blank field wins; blank field falls back to context field, then `value`. **`IsNullOrWhiteSpace` with context:** same invalid/valid cases and uses the context field. **`DoesNotMatch`:** null context throws; missing first value returns its required error before inspecting later mismatch; missing second value returns its required error; distinct non-blank values return a non-matching error with both labels; identical values return false; comparison is ordinal and case-sensitive; blank labels follow the required-field fallback behavior. | Implemented |

## Current `UnitTest1.cs` Migration

Delete `UnitTest1.cs` only after moving its tests without changing their
behavior:

| Current class | Destination | Notes |
| --- | --- | --- |
| `ErrorInvariantTests` | `Error/ErrorTests.cs`, `Results/ResultTests.cs`, and `Results/ResultOfTTests.cs` | Move error-construction rules to `ErrorTests`; move result-state rules to their generic/non-generic class. Keep the private invalid-error fixture with `ErrorTests`. |
| `ErrorCodeTests` | `Error/ErrorCodeTests.cs` and `Error/ValidationErrorTests.cs` | Keep direct `ErrorCode` tests in `ErrorCodeTests`; move the `CustomWithReason` integration assertion to `ValidationErrorTests`; built-in-code assertion may remain in `ErrorCodeTests` as a cross-surface convention check. |

## Implementation Order

1. Create `TestData.cs` and the folders, then move the existing tests to their
   destinations without expanding behavior.
2. Complete the error model tests (`ErrorCode`, compatibility, context, error,
   then validation error) because result and validator tests depend on them.
3. Complete non-generic and generic result tests, then mapping tests.
4. Complete validator extension tests.
5. Run the complete test project across all configured target frameworks and
   update each catalog status to `Implemented`.
