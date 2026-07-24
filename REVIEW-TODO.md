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

- [ ] **P0 — Define and test the error invariants**
  - Prevent `Error.Custom(..., ErrorType.Validation)` from creating an invalid
    plain `Error`, or provide a valid custom-validation path.
  - Reject undefined enum values where appropriate.
  - Test all success/failure/validation combinations and invalid construction.

- [ ] **P0 — Add comprehensive automated tests**
  - Cover factories, null handling, validation aggregation, primary errors,
    `Map`, exception propagation, equality, immutability, and edge cases.
  - Add tests for every supported target framework.

- [ ] **P0 — Choose supported target frameworks**
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

- [ ] **P1 — Validate and simplify `ErrorContext`**
  - Validate required entity/resource names and decide how field names behave.
  - Prevent malformed codes such as `Create..NotFound`.
  - Reconsider `NameOfEntity`; prefer clearer terminology such as `Entity`,
    `Resource`, or `ResourceName`.
  - Separate machine metadata from display/presentation labels where needed.

- [ ] **P1 — Remove application-specific message assumptions**
  - Keep the core package framework-neutral and avoid requiring English,
    UI-oriented wording.
  - Allow caller-supplied messages or message factories for all built-in
    error kinds.
  - Define how localization and API-specific presentation belong in extensions.

- [ ] **P1 — Remove or replace `FormatDescription`**
  - It is currently a non-overridable no-op and provides no real extension point.
  - Delete it or replace it with an intentional, documented formatting strategy.

- [ ] **P1 — Reconsider `Error.ToString()`**
  - Do not impose a logging/serialization format from the core package.
  - Either provide a concise diagnostic representation or leave formatting to
    logging and integration adapters.

- [ ] **P1 — Improve validation API ergonomics**
  - Reassess methods that return `true` when validation fails despite names that
    look like ordinary predicates.
  - Consider clearer `TryValidate...`/`Validate...` APIs.
  - Avoid fallback field names such as `"value"`.
  - Decide whether `DoesNotMatch` should also perform required checks or whether
    those should be separate validations.
  - Validate arguments such as `expectedFormat` and `range`.

- [ ] **P2 — Make collection contracts express ordering**
  - Consider exposing `ValidationErrors` as `IReadOnlyList<ValidationError>` since
    the first item is defined as the primary validation error.

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
