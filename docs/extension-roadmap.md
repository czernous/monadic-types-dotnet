# Extension Roadmap

The current CSFM API is the compatibility floor. Extensions are accepted only
with explicit semantics, pure benchmarks, and NativeAOT size measurements.

## Near Term

- Complete `Option`/nullable and `Option`/`Result` conversions.
- Add `Flatten` and `Transpose` where signatures remain unambiguous.
- Add explicit exception-to-error `Try` transformations from the older result
  experiments without adopting pointer ownership or consumable value semantics.
- Complete type-changing and caller-state async composition without a combinatorial
  overload explosion.
- Maintain generic validation-source mappers so FluentValidation compatibility
  needs no runtime dependency. Compatibility tests pin the exact third-party
  version and upgrades are explicit maintenance work.
- Define intentional equality, hashing, and formatting instead of inheriting
  record-generated behavior accidentally.

## Optional Layers

- Task and ValueTask composition helpers.
- Effects helpers for explicit exception capture.
- ASP.NET Core RFC 9457 and OpenAPI integration.
- Optional `ActivitySource` and `Meter` diagnostics. This adapter remains
  provisional until production audits confirm that its event model is useful;
  core error values remain independently projectable into any telemetry stack.
- Applicative validation only after an accumulating production use case exists.

## Non-Goals

- Reflection, runtime code generation, mandatory DI, or service location.
- Mutable pointer-backed Result state.
- Pulling third-party validation, telemetry, or documentation stacks into core.
- API parity for its own sake when an operation has no clear semantics or use case.
