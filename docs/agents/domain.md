# Domain documentation

Use one shared domain context for this solution. Feature packages separate
dependencies and capabilities within one library domain; their number alone
does not justify a context map or a glossary per package.

## Read for the task

- [README](../../README.md): intent, concepts, package selection, and usage.
- [API reference](../api-reference.md): generated contracts; inspect corresponding
  source XML and implementation when changing behavior.
- [Compatibility](../compatibility.md): integration boundaries.
- [Development policy](../development.md): implementation and verification rules.
- [Benchmarks](../benchmarks.md): performance evidence for hot-path changes.
- [Documentation architecture](../documentation-architecture.md): authoritative
  sources and generated outputs.

If root `CONTEXT.md` or `docs/adr/` exists, read the material relevant to the task.
Their absence does not block work. Create a glossary or ADR when an actual term
or consequential decision is settled, rather than adding empty scaffolding.

## Preserve existing distinctions

- `Result<T,E>` distinguishes success and failure; observing its default
  uninitialized state is invalid. `Option<T>` distinguishes presence and absence;
  its default is `None`, and `Some(null)` is rejected.
- Expected absence, typed failure, and exceptions at controlled boundaries are
  different concepts. Check existing composition APIs before proposing new ones.
- An application-defined numeric error category is distinct from an HTTP status.
  HTTP mapping belongs at the ASP.NET Core boundary. Changing that relationship
  requires an explicit design decision, not an assumed bug fix.
- Delegate, caller-state, callable-struct, and generated-wrapper APIs serve
  different call-site needs. Preserve branch semantics and verify type inference
  using natural consumer syntax.
- Allocation claims need evidence for the relevant path. Owning an output array
  and avoiding per-element callback allocations are separate claims.

Use existing contract terminology. Identify conflicts with documented contracts
or ADRs and resolve the decision before replacing them. Record accepted changes
at their authoritative source and regenerate dependent documentation rather than
maintaining competing definitions.
