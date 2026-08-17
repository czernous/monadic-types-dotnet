# Release Closure Plan

This is a finite pre-release closure list, not a commitment to continuous API
growth. The library intentionally covers its documented Result, Option, error,
effect, diagnostic, and application-boundary workflows. Any addition must close
a demonstrated gap, have explicit semantics, and include pure benchmarks and
NativeAOT size evidence.

## Completion Threshold

The initial library is complete when:

- Every retained public API supports a demonstrated application workflow and has
  exhaustive behavioral documentation.
- Unit, compatibility, generator, ASP.NET Core, and NativeAOT smoke tests pass.
- Accepted hot paths preserve their allocation requirements and benchmark
  baselines; new hot-path APIs have isolated measurements before acceptance.
- Shipping projects remain reflection-free, trim-safe, and NativeAOT-compatible.
- Package boundaries and optional integrations remain independently consumable.
- The library has been exercised in maintained applications and material API
  problems discovered there have been resolved.

After that threshold, maintenance is limited primarily to correctness, security,
runtime compatibility, and measured performance work. A new feature requires a
recurring concrete use case that cannot be handled clearly at the application
boundary; theoretical completeness alone is insufficient.

## Near Term

- Add caller-state async composition only where profiling shows delegate capture;
  do not create a combinatorial overload matrix speculatively.
- Maintain generic validation-source mappers so FluentValidation compatibility
  needs no runtime dependency. Compatibility tests pin the exact third-party
  version and upgrades are explicit maintenance work.

## Completed Application Gaps

- Explicit reference/value nullable bridges and positional deconstruction.
- Option traversal through fallible selectors with delegate, caller-state, and
  struct-callable dispatch.
- Fail-fast independent Result Map and Bind for two through six inputs.
- Count-known collection traversal in an optional package with explicit
  one-array ownership and no `IEnumerable<T>` surface.
- Opt-in LINQ extension members in a separate package. Fluent and query syntax
  are both supported; fluent syntax is the measured default.
- Positional Result and Option deconstruction for allocation-free switch and
  property patterns.
- Reflection-free ASP.NET Core error catalogs, deterministic problem examples,
  status-scoped OpenAPI transformation, NativeAOT package smoke coverage, and a
  targeted cross-IDE XML-projection diagnostic.
- Explicit semantic Error equality and hashing, retained-cause identity, and
  allocation-free diagnostic formatting contracts.
- Type-changing Result Map revalidated against an unchanged same-run control;
  the established implementation remains allocation-free and was retained.

## Decision Matrix

| Candidate | Value | Cost or risk | Decision |
| --- | --- | --- | --- |
| Count-known collection traversal | High | One explicit owned output array | Accepted in optional Collections package |
| Option traversal and nullable bridges | High | Small, allocation-free core surface | Accepted |
| Result combination through arity six | High | Finite overload surface and NativeAOT code size | Accepted; stop at six until a recurring higher-arity use case exists |
| Opt-in LINQ | Medium-high | Additional names and generated code when consumed | Accepted in separate package; fluent form is recommended |
| Positional pattern support | High | Two small deconstructors | Accepted; matchable case-wrapper hierarchy rejected as larger and less predictable |
| OpenAPI error catalogs | High | Cold metadata ownership and optional Microsoft runtime dependencies | Accepted in separate package with reflection-free default |
| XML-comment automation | Medium-high | Microsoft's complete behavior uses reflection during document generation | Compatibility only; direct third-party reference is explicit opt-in |
| Logging projection and severity policy | Medium | Logging dependency and application-specific severity/disclosure policy | Rejected from runtime packages; document application-owned extensions |
| General-purpose usage analyzer package | Medium | False positives, build cost, and policy-specific heuristics | Rejected; retain only precise diagnostics tied to package behavior |
| Framework-agnostic testing helpers | Low-medium | Very small application helper but permanent public/package surface | Deferred until repeated use demonstrates stable assertion semantics |
| Cached or interned widened errors | Low-medium | Lifetime, retained cause, identity, and domain-policy hazards | Rejected as a library policy; widen once at the boundary and cache only domain-safe singleton cases locally |
| More Task/ValueTask side-effect overloads | Medium | Large overload matrix and NativeAOT code-size cost | Deferred until profiling identifies a specific capture or composition gap |
| Accumulating validation | Medium-high | New error-combination semantics and ownership allocations | Deferred until a concrete accumulating workflow defines the contract |
| Option Tap/Zip and higher arities | Low | Convenience growth without a demonstrated missing workflow | Rejected for the initial completion boundary |

Value is not sufficient by itself. An accepted API must also have stable
semantics, remain reflection-free, avoid request/hot-path allocation, justify
its NativeAOT code-size cost, and be smaller than an application-local solution.

## Deferred Candidates

- Expand Task and ValueTask side-effect composition after its API and allocation
  behavior have dedicated tests and benchmarks.
- Extend ASP.NET Core OpenAPI metadata only where generated documents have a
  verified compatibility gap. XML-comment parity remains delegated to the
  explicitly installed Microsoft package rather than a partial custom clone.
- Further diagnostics event-model expansion. The existing optional
  `ActivitySource` and `Meter` adapter remains independent from core errors;
  production audits must justify additional signals.
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
  documented Result and Option workflows. The retained optional packages expose
  only Result/Option-specific, measured operations.
