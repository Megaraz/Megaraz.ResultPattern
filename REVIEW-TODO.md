# ResultPattern Review TODO

AI-maintained backlog for making `Megaraz.ResultPattern` suitable as a reusable,
public NuGet package. Update checkboxes, notes, and acceptance criteria as work
is completed. Keep scope focused on the core package; framework integrations
belong in extension packages such as `Megaraz.ResultPattern.AspNetCore`.

## Priority legend

- **P0**: Blocks a safe public release
- **P1**: Important for a robust, maintainable library
- **P2**: API/documentation improvement

## Backlog

- [x] **P0 — Define and test the error invariants**
  - Prevent `Error.Custom(..., ErrorType.Validation)` from creating an invalid
    plain `Error`, or provide a valid custom-validation path.
  - Reject undefined enum values where appropriate.
  - Test all success/failure/validation combinations and invalid construction.
  - Completed: plain validation errors are rejected in favor of `ValidationError`;
    error-operation and legacy-reason enum values are validated; result-state
    invariants are covered by multi-targeted tests.

- [x] **P0 — Add comprehensive automated tests**
  - Cover factories, null handling, validation aggregation, primary errors,
    `Map`, exception propagation, equality, immutability, and edge cases.
  - Add tests for every supported target framework.
  - Completed: the executable test plan covers all public API surfaces across
    net8.0, net9.0, and net10.0.

- [x] **P0 — Choose supported target frameworks**
  - Do not remain `net10.0`-only unless this is an explicit product decision.
  - Prefer supported .NET targets such as `net8.0;net9.0;net10.0`, or assess
    `netstandard2.1` if broader compatibility is required.

- [ ] **P0 — Add open-source licensing and repository essentials**
  - Add `LICENSE` and `PackageLicenseExpression`.
  - Add CI, contribution guidance, code-of-conduct guidance, and issue/PR
    templates as appropriate for the public repository.

- [x] **P1 — Redesign error-code extensibility**
  - Preserve convenience codes but avoid forcing every consumer into the
    `Operation.Entity.Reason` format.
  - Support stable caller-defined codes and custom reasons without all custom
    errors collapsing to `*.Custom`.
  - Avoid making enum member names an accidental long-term code contract.
  - Completed: `ErrorCode.From` accepts caller-owned stable code values, while
    `ErrorCode.For` and `ValidationError.CustomWithReason` provide the optional
    conventional shape. Built-in reason strings are explicit constants.

- [x] **P1 — Validate and simplify `ErrorContext`**
  - Validate required entity/resource names and decide how field names behave.
  - Prevent malformed codes such as `Create..NotFound`.
  - Reconsider `NameOfEntity`; prefer clearer terminology such as `Entity`,
    `Resource`, or `ResourceName`.
  - Separate machine metadata from display/presentation labels where needed.
  - Completed: `ErrorContext` validates operation, entity, and optional field
    values; entity names cannot create malformed conventional codes, while
    field names remain optional metadata and are excluded from code generation.
    `EntityName` is retained as the established public name and documented as
    covering logical resources.

- [x] **P1 — Remove application-specific message assumptions**
  - Keep the core package framework-neutral and avoid requiring English,
    UI-oriented wording.
  - Allow caller-supplied messages or message factories for all built-in
    error kinds.
  - Define how localization and API-specific presentation belong in extensions.
  - Completed: built-in factories accept caller-supplied user messages and
    context-aware message factories; omitted user messages remain empty, and
    result validation failures no longer impose an English presentation message.

- [x] **P1 — Remove or replace `FormatDescription`**
  - It is currently a non-overridable no-op and provides no real extension point.
  - Delete it or replace it with an intentional, documented formatting strategy.
  - Completed: removed the no-op helper and pass validated descriptions directly to
    the error constructors.

- [x] **P1 — Reconsider `Error.ToString()`**
  - Do not impose a logging/serialization format from the core package.
  - Either provide a concise diagnostic representation or leave formatting to
    logging and integration adapters.
  - Completed: `ToString()` now returns a concise `Code: Description` diagnostic
    representation without the previous labeled multi-line format.

- [x] **P1 — Improve validation API ergonomics**
  - Reassess methods that return `true` when validation fails despite names that
    look like ordinary predicates.
  - Consider clearer `TryValidate...`/`Validate...` APIs.
  - Avoid fallback field names such as `"value"`.
  - Decide whether `DoesNotMatch` should also perform required checks or whether
    those should be separate validations.
  - Validate arguments such as `expectedFormat` and `range`.
  - Completed: added null-returning `ValidateRequired`, `ValidateRequiredFields`,
    and `ValidateDoesNotMatch` APIs while retaining the legacy boolean methods;
    required checks remain first-class behavior for matching validation, field
    fallback no longer uses `"value"`, and blank format/range descriptors are rejected.

- [x] **P2 — Make collection contracts express ordering**
  - `ValidationErrors` is exposed as `IReadOnlyList<ValidationError>` since the
    first item is defined as the primary validation error. The read-only contract
    preserves result immutability while providing indexed access.

- [ ] **P1 — Define the intended Result API surface**
  - Decide whether the core needs `Bind`/`FlatMap`, `Match`, `MapError`, `Tap`,
    `Ensure`, async variants, result combination, and conversions between
    `Result` and `Result<T>`.
  - Avoid publishing a stable 1.0 API before this scope is intentional.

- [ ] **P2 — Improve package metadata and packaging**
  - Add copyright, project URL, explicit versioning, symbols/source link, and
    package validation where appropriate.
  - Verify the package contains the expected README, XML docs, symbols, and
    framework assets.

- [ ] **P1 — Expand public documentation**
  - Document result semantics, failure handling, validation behavior, null/value
    rules, error-code conventions, mapping behavior, and recommended usage.
  - Include examples for both domain/core usage and extension-package usage.

- [ ] **P0 — Establish release criteria**
  - Do not publish `1.0.0` until the P0 items are complete and the public API has
    been reviewed for compatibility, naming, extensibility, and maintainability.
  - Record deliberate non-goals and breaking-change policy.

## Review notes

- Current implementation is compact and readable, but should be treated as a
  prototype until the error model and public API are deliberately stabilized.
- Keep ASP.NET Core, serialization, HTTP status mapping, and other framework
  concerns outside the core package.
