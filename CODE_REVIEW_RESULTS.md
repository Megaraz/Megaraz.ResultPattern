# Code Review: Megaraz.ResultPattern

**Reviewed:** 2026-07-26  
**Scope:** Release-readiness review of `main` at `4e6cb0e` (`v0.2.0`)  
**Package version:** 0.2.0  
**Target frameworks:** `net8.0`, `net9.0`, `net10.0`  
**Reviewer:** Codex

## Decision

**Not ready to merge**  
**Not ready to publish**

The implementation builds, tests, packs, and installs successfully on every
supported target framework, and both extension packages remain compatible.
Publication should still be blocked because a validation factory copies
technical descriptions into the user-facing channel by default,
`ErrorContext` can be changed into states its constructor promises to reject,
and the publishing workflow does not prove that the Git tag matches the NuGet
package version. The README's primary validation example also throws at
runtime.

## Findings

### P0 — Release blockers

None.

### P1 — Must fix before release

#### [P1] Custom validation errors expose technical descriptions as user messages

- **Location:** `Megaraz.ResultPattern/ValidationError.cs:46`
- **Evidence:** Both `Custom` overloads pass `userMessage ?? description` to
  the base error at lines 55-56 and 70-71. Therefore a caller that supplies
  only a diagnostic description gets the same text in `UserMessage`. This
  differs from the other error factories and directly contradicts
  `README.md:59-62`, which says `UserMessage` is empty unless supplied.
- **Impact:** Consumers and extension packages are encouraged to treat
  `UserMessage` as presentation-safe. A custom validation description can
  contain internal rules, identifiers, or other diagnostic detail and may be
  returned to a client without an explicit disclosure decision.
- **Recommendation:** Default `UserMessage` to `string.Empty` in both
  `Custom` overloads and, transitively, `CustomWithReason`. Preserve a
  description as a user message only when the caller explicitly supplies it.
  Document the behavioral correction and add regression coverage.
- **Acceptance criteria:**
  - [ ] Every custom validation factory returns an empty `UserMessage` when
        `userMessage` is omitted or `null`.
  - [ ] Explicit user messages are preserved.
  - [ ] Tests cover descriptions containing obviously sensitive diagnostic
        text and prove it is absent from `UserMessage`.
  - [ ] README and XML documentation describe the safe default.

#### [P1] Public init accessors bypass `ErrorContext` invariants

- **Location:** `Megaraz.ResultPattern/ErrorContext.cs:25`
- **Evidence:** `Operation`, `EntityName`, and `FieldName` have unrestricted
  public `init` accessors. Record `with` expressions assign through these
  accessors without running the constructor validation. The test at
  `Megaraz.ResultPattern.Tests/Error/ErrorContextTests.cs:76` demonstrates
  creation of an instance whose blank `EntityName` would be rejected by the
  constructor. The same bypass permits undefined operations and blank field
  names.
- **Impact:** The public type can represent states that its constructor,
  README, and error-code factories define as invalid. Failures are deferred
  until selected downstream operations, while paths such as a custom
  validation error with a caller-owned `ErrorCode` can consume parts of the
  invalid context without revalidating the complete object.
- **Recommendation:** Keep record copy ergonomics but validate assignments,
  for example with validated `init` accessors backed by fields. Ensure all
  public construction and copy paths enforce the same rules.
- **Acceptance criteria:**
  - [ ] Invalid `Operation`, `EntityName`, and `FieldName` values are rejected
        during both direct construction and `with` expressions.
  - [ ] Valid `with` expressions, including setting `FieldName` to `null`,
        continue to work.
  - [ ] Tests cover every invalid copy path.
  - [ ] Source and binary compatibility implications of the implementation
        choice are documented.

#### [P1] Publishing does not verify the tag-to-package version contract

- **Location:** `.github/workflows/publish.yml:3`
- **Evidence:** Any tag matching `v*` starts publishing. The workflow packs
  the static `<Version>` from the project and pushes the resulting package at
  lines 34-44, but never compares the tag, project version, and generated
  `.nuspec` version.
- **Impact:** A mistyped or stale release tag can publish a package whose
  version and source tag disagree, or fail only at the final push because it
  accidentally rebuilt an already-published version. This weakens release
  provenance and makes the tag an unreliable package baseline.
- **Recommendation:** Parse the expected version from the tag, fail before
  authentication when it differs from the project/package version, and
  inspect the packed `.nuspec` before publishing.
- **Acceptance criteria:**
  - [ ] A tag such as `v0.3.0` can publish only a package whose NuGet version is
        exactly `0.3.0`.
  - [ ] Mismatched or malformed tags fail before the NuGet login step.
  - [ ] CI tests both matching and mismatching version cases.

### P2 — Should fix

#### [P2] The documented validation flow calls the wrong result factory

- **Location:** `README.md:64`
- **Evidence:** The example obtains a `ValidationError` from
  `ValidateRequired` and then calls `Result.Failure(error)` at line 70.
  `Result.Failure` supplies an empty validation-error collection, after which
  the invariant check in `Result.cs:51-56` throws. Existing tests explicitly
  assert this rejection at
  `Megaraz.ResultPattern.Tests/Results/ResultTests.cs:28`.
  An isolated package consumer reproduced the `ArgumentException`.
- **Impact:** The main validation example compiles but fails at runtime,
  leading adopters away from the required `Result.ValidationFailure` API.
- **Recommendation:** Change the example to
  `Result.ValidationFailure([error])` and add a documentation smoke test that
  compiles and executes representative README examples.
- **Acceptance criteria:**
  - [ ] The README validation example runs without throwing and returns a
        validation failure containing the error.
  - [ ] An automated documentation/consumer smoke test covers the example.

#### [P2] Compatibility validation methods have incorrect nullability contracts

- **Location:** `Megaraz.ResultPattern/ValidatorExtensions.cs:115`
- **Evidence:** Both `IsNullOrWhiteSpace` overloads and `DoesNotMatch` declare
  non-nullable `out ValidationError` values, but assign `null` whenever the
  method returns `false` (lines 121-122, 135-136, and 155-157). The tests at
  `Megaraz.ResultPattern.Tests/Validation/ValidatorExtensionsTests.cs:38` and
  line 68 confirm the null runtime value.
- **Impact:** Nullable-aware consumers receive a false promise from the
  compiler and can dereference a null result without a warning.
- **Recommendation:** Model the contract as nullable with
  `[NotNullWhen(true)] out ValidationError?`, while retaining the existing
  method behavior and compatibility surface.
- **Acceptance criteria:**
  - [ ] Nullable analysis knows the error is non-null on the `true` branch and
        possibly null otherwise.
  - [ ] Runtime behavior and method names remain unchanged.
  - [ ] A nullable-enabled consumer compile test verifies both branches.

#### [P2] Source Link is configured but no symbol package is produced or published

- **Location:** `Megaraz.ResultPattern/Megaraz.ResultPattern.csproj:21`
- **Evidence:** The project references `Microsoft.SourceLink.GitHub`, but does
  not enable symbol package generation. The required pack command produced
  only `Megaraz.ResultPattern.0.2.0.nupkg`; its contents include assemblies
  and XML documentation but no PDB, and no `.snupkg` was produced. The
  publishing workflow pushes only `artifacts/*.nupkg`.
- **Impact:** Repository provenance appears in the `.nuspec`, but NuGet
  consumers do not receive portable symbols needed to use Source Link for
  source-level debugging.
- **Recommendation:** Define the package's symbol policy. If Source Link is
  intended, enable portable `.snupkg` generation, validate it, and publish it
  with the primary package.
- **Acceptance criteria:**
  - [ ] Pack produces a `.snupkg` containing portable PDBs for all TFMs.
  - [ ] Source Link verification resolves source files to commit `4e6cb0e` or
        the applicable release commit.
  - [ ] The release workflow publishes the verified symbol package.

#### [P2] Release dependencies use mutable action tags

- **Location:** `.github/workflows/publish.yml:17`
- **Evidence:** The credential-bearing publishing job executes
  `actions/checkout@v4`, `actions/setup-dotnet@v4`, and `NuGet/login@v1`.
  Major-version tags can move to different commits after review.
- **Impact:** A compromised or unexpectedly changed upstream tag can alter
  code running in a job with `id-token: write` and NuGet publishing access.
- **Recommendation:** Pin third-party actions in publishing and CI workflows
  to reviewed full commit SHAs, with comments retaining the human-readable
  release versions and an update process such as Dependabot.
- **Acceptance criteria:**
  - [ ] Every third-party action is pinned to a reviewed full commit SHA.
  - [ ] Automated dependency updates keep action pins maintainable.
  - [ ] Workflow permissions remain `contents: read` plus `id-token: write`
        only where publishing requires it.

#### [P2] Public security and support guidance is incomplete

- **Location:** `README.md:1`
- **Evidence:** The repository has no `SECURITY.md`, and the README does not
  state the supported frameworks, versioning policy, package-family choices,
  or a private vulnerability-reporting path. The supported TFMs appear only
  in the project file and contributor guide.
- **Impact:** Package consumers cannot readily determine support and
  compatibility expectations, and security researchers have no documented
  private reporting route.
- **Recommendation:** Add a security policy and concise README sections for
  supported frameworks, semantic versioning, package boundaries, security
  reporting, and license.
- **Acceptance criteria:**
  - [ ] `SECURITY.md` documents a private reporting method and supported
        versions.
  - [ ] README lists .NET 8, 9, and 10 and explains when to use the core versus
        extension packages.
  - [ ] README states compatibility/versioning expectations and links the MIT
        license.

### P3 — Improvements

#### [P3] CI does not retain or inspect the packed artifact

- **Location:** `.github/workflows/ci.yml:40`
- **Evidence:** CI runs `dotnet pack` but performs no package-content check and
  does not upload the `.nupkg` for inspection. The release workflow separately
  rebuilds a new artifact.
- **Impact:** Accidental package-content or metadata regressions are discovered
  only manually or after publication, and reviewers cannot inspect the CI
  artifact.
- **Recommendation:** Add automated `.nupkg` metadata/content assertions and
  upload the verified package as a CI artifact. Where practical, promote that
  artifact through the release process rather than rebuilding it.
- **Acceptance criteria:**
  - [ ] CI verifies expected TFMs, assemblies, XML docs, README, license
        metadata, repository commit, and absence of unintended files.
  - [ ] The checked package is retained as a workflow artifact.

## Compatibility assessment

- **Source compatibility:** Compatible
- **Binary compatibility:** Not assessed
- **Behavioral compatibility:** Changed
- **Wire compatibility:** Not applicable
- **Dependency compatibility:** Compatible
- **Target-framework compatibility:** Compatible
- **Serialized or persisted formats:** Not applicable
- **Required version impact:** Minor

The `v0.1.0..v0.2.0` source diff adds `ErrorType.External = 8` without removing
or renumbering existing enum values, so source compatibility is preserved and
the minor prerelease version increment is appropriate. Behavior changes
because extension errors may now return the new enum value. Binary
compatibility was inspected at source level but not exercised with a consumer
binary compiled against 0.1.0, so it remains unassessed. No serializer or
persistence format is part of the core package contract. Correcting the
validation-message default is itself a behavioral change and should be called
out in the next release notes.

## Cross-repository consistency

- Inspected `Megaraz.ResultPattern.AspNetCore` and
  `Megaraz.ResultPattern.Infrastructure`.
- Both target .NET 8, 9, and 10 and reference core package 0.2.0.
- Both derive specialized errors from `Error`, classify them as
  `ErrorType.External`, retain diagnostic exceptions, and default
  `UserMessage` to empty.
- ASP.NET Core restored, built with zero warnings, and passed 68 tests on each
  TFM.
- Infrastructure restored, built with zero warnings, and passed 11 tests on
  each TFM.
- The validation-message leak is inconsistent with the safer defaults used by
  both sibling packages.
- No sibling source changes were made.

## Security assessment

Normal `Error` factories and both extension packages keep exception and
technical details out of `UserMessage` by default. `ValidationError.Custom` is
the material exception and is release-blocking because it silently crosses
the diagnostic/presentation boundary. The core package performs no I/O,
logging, deserialization, or resource ownership, so cancellation, input
bounds, and disposal risks are limited. The release job uses least-privilege
declared permissions, but mutable action tags and missing public vulnerability
reporting remain residual supply-chain and disclosure-process risks.

## Package and open-source readiness

The generated package has accurate ID/version/repository metadata, dependency
groups for all three TFMs, the expected assemblies and XML documentation, the
NuGet README, MIT license expression, and no runtime dependencies. The
repository commit is present in the `.nuspec`. The package does not include
unexpected repository files. A local clean consumer project restored, built,
and ran against the generated package. Symbol packaging, package-content CI,
security guidance, and complete consumer-facing support/versioning
documentation remain incomplete.

## Verification

| Check | Result | Notes |
|---|---|---|
| Restore | Pass | `dotnet restore Megaraz.ResultPattern.slnx` |
| Release build | Pass | All three TFMs; 0 warnings, 0 errors |
| Tests | Pass | 118 passed, 0 failed, 0 skipped per TFM; 354 total executions |
| Pack | Pass | Produced `Megaraz.ResultPattern.0.2.0.nupkg` |
| Package inspection | Pass | Correct TFMs, DLLs, XML docs, README, metadata; no symbol package |
| Consumer smoke test | Pass | Local package installed and ran on `net8.0`; reproduced README exception |
| Public API comparison | Pass | Inspected `v0.1.0..v0.2.0`; additive enum member only |
| Sibling compatibility | Pass | ASP.NET Core 68/TFM; Infrastructure 11/TFM |
| Dependency vulnerability review | Pass | No vulnerable direct or transitive packages reported |

No compiler warnings or skipped tests were reported. The initial sandboxed
restore could not read the user's NuGet configuration; the same required
restore passed with approved access. The first consumer restore attempt used
only the local feed and could not resolve the .NET host package; restoring with
normal NuGet sources plus the local feed succeeded. A binary consumer compiled
against 0.1.0 and Source Link resolution were not run.

Informationally, NuGet reported newer versions of `coverlet.collector`,
`Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, and
`Microsoft.SourceLink.GitHub`; no upgrade is required solely because a newer
version exists.

## Positive observations

- Result constructors centralize and defend success, failure, and validation
  invariants.
- Typed success rejects null at both compile-time intent and runtime.
- Validation collections are materialized and copied, avoiding deferred
  execution and caller mutation.
- Error codes preserve existing numeric enum values and offer caller-owned
  codes without imposing the conventional format.
- Technical descriptions, exceptions, and explicit user messages are
  separated correctly in the normal and extension error paths.
- Core remains framework-agnostic and dependency-light.
- Tests exercise every supported TFM and cover the key result/error
  invariants.
- CI and publishing workflows declare narrow permissions.

## Recommended next steps

1. [ ] Fix the `ValidationError.Custom` safe-message default and document its
       compatibility impact.
2. [ ] Enforce `ErrorContext` invariants through record copy/init paths.
3. [ ] Gate publishing on exact tag, project, and packed-package version
       agreement.
4. [ ] Correct and execute-test the README validation example.
5. [ ] Correct nullable annotations on compatibility validation methods.
6. [ ] Define and implement symbol, package-inspection, and action-pinning
       policies.
7. [ ] Add security, framework-support, package-family, versioning, and license
       guidance.
