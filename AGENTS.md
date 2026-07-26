# Repository guidelines

## Project purpose

This repository contains `Megaraz.ResultPattern`, a small, reusable .NET
library for representing successful operations, failures, and validation
errors without coupling consumers to web, persistence, or presentation
frameworks. It is a public OSS project and is published as a NuGet package, so
clarity, compatibility, documentation, and package quality are part of the
product.

## Package boundaries

- Keep this package framework-agnostic. ASP.NET Core, HTTP, persistence,
  logging, and other integration concerns belong in extension packages.
- Keep public abstractions small and stable. Application-specific policies,
  localization, and domain-specific error catalogs belong in consuming
  applications.
- Extension packages may derive specialized errors from `Error` and classify
  them as `ErrorType.External`; do not move extension-specific details into
  the core library.
- Avoid new dependencies unless they provide substantial, broadly useful
  value that cannot reasonably be implemented with the BCL.

## Public contracts and invariants

- Preserve the invariants enforced by `Result`, `Result<TValue>`, `Error`,
  `ValidationError`, `ErrorCode`, and `ErrorContext`.
- A successful typed result has a non-null value and no error. A failed result
  has a non-`None` primary error. Validation failures contain at least one
  `ValidationError`, including the primary error.
- Error codes are stable, machine-readable contracts. Do not require
  application-owned codes to use the optional `Operation.Entity.Reason`
  convention, and do not change the meaning of an existing code casually.
- Keep technical diagnostic detail in `Error.Description` and `Exception`.
  `UserMessage` and `Result.Message` must not implicitly expose exception or
  infrastructure details.
- Prefer explicit result-based control flow for expected failures. Do not
  silently catch programming errors or turn every exception into a result.

## Technical constraints

- The library and tests currently target `net8.0`, `net9.0`, and `net10.0`.
  Do not change target frameworks without explicitly reporting the
  compatibility and packaging impact.
- Keep nullable reference types and implicit usings enabled. Treat compiler
  and analyzer warnings as issues to understand, not noise to suppress.
- Preserve semantic versioning. Report any source, binary, or behavioral
  compatibility risk before completing a public API change.
- Add XML documentation for public APIs and update the README when public
  behavior, defaults, or examples change.
- Keep NuGet metadata, release notes, Source Link settings, and the packaged
  README accurate when preparing a release.

## Testing and packaging

Add or update tests for new public behavior, regressions, invariants, and edge
cases. Tests should describe observable behavior rather than implementation
details and should cover every supported target framework.

Before completing implementation work, run:

1. `dotnet restore Megaraz.ResultPattern.slnx`
2. `dotnet build Megaraz.ResultPattern.slnx --configuration Release --no-restore`
3. `dotnet test Megaraz.ResultPattern.slnx --configuration Release --no-build`
4. `dotnet pack Megaraz.ResultPattern/Megaraz.ResultPattern.csproj --configuration Release --no-build --no-restore`

Report failures, warnings, skipped validation, and public API or package
changes.

## Working style

- Start non-trivial tasks by reading the relevant implementation, tests,
  README, and project metadata, then present a short implementation plan.
- Make focused changes and preserve the repository's existing style.
- Do not reformat unrelated files or overwrite unrelated work in a dirty
  worktree.
- Prefer the simplest design that preserves the documented contracts. Prefer
  composition over inheritance unless inheritance clearly models the public
  contract.
- Summarize changed files, design decisions, verification performed, and any
  compatibility considerations.
