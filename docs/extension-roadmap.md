# Extension Roadmap

The current CSFM API is the compatibility floor. Extensions are accepted only
with explicit semantics, pure benchmarks, and NativeAOT size measurements.

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

## Optional Layers

- Expand Task and ValueTask side-effect composition after its API and allocation
  behavior have dedicated tests and benchmarks.
- Extend ASP.NET Core OpenAPI metadata only where generated documents have a
  verified compatibility gap.
- Optional `ActivitySource` and `Meter` diagnostics. This adapter remains
  provisional until production audits confirm that its event model is useful;
  core error values remain independently projectable into any telemetry stack.
- Applicative validation only after an accumulating production use case exists.

## Non-Goals

- Reflection, runtime code generation, mandatory DI, or service location.
- Mutable pointer-backed Result state.
- Pulling third-party validation, telemetry, or documentation stacks into core.
- API parity for its own sake when an operation has no clear semantics or use case.
