# Release Closure Plan

This is a finite pre-release closure list, not a commitment to continuous API
growth. The library intentionally covers its documented Result, Option, error,
effect, diagnostic, and application-boundary workflows. Any addition must close
a demonstrated gap, have explicit semantics, and include pure benchmarks and
NativeAOT size evidence.

## Near Term

- Complete the remaining intentional `Option`/nullable conversions without
  introducing ambiguous implicit operators.
- Add caller-state async composition only where profiling shows delegate capture;
  do not create a combinatorial overload matrix speculatively.
- Maintain generic validation-source mappers so FluentValidation compatibility
  needs no runtime dependency. Compatibility tests pin the exact third-party
  version and upgrades are explicit maintenance work.
- Define intentional equality, hashing, and formatting instead of inheriting
  record-generated behavior accidentally.

## Deferred Candidates

- Expand Task and ValueTask side-effect composition after its API and allocation
  behavior have dedicated tests and benchmarks.
- Extend ASP.NET Core OpenAPI metadata only where generated documents have a
  verified compatibility gap.
- Optional `ActivitySource` and `Meter` diagnostics. This adapter remains
  provisional until production audits confirm that its event model is useful;
  core error values remain independently projectable into any telemetry stack.
- Applicative validation only after an accumulating production use case exists.

Deferred candidates are not part of the release commitment. They should remain
outside the core package and may be rejected when application-local composition
is smaller or clearer.

## Non-Goals

- Reflection, runtime code generation, mandatory DI, or service location.
- Mutable pointer-backed Result state.
- Pulling third-party validation, telemetry, or documentation stacks into core.
- API parity for its own sake when an operation has no clear semantics or use case.
- General-purpose collection, LINQ, or functional helpers unrelated to the
  documented Result and Option workflows.
