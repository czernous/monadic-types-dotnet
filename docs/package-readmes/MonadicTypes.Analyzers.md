# MonadicTypes.NET.Analyzers

Opt-in Roslyn diagnostics for precise `MonadicTypes.NET` usage hazards. The
package is a build-time analyzer and adds no runtime dependency.

## Install

```bash
dotnet add package MonadicTypes.NET.Analyzers --prerelease
```

The current rule, `MT0001`, warns when `Option<T>` is compared directly with
`null`. `Option<T>` is a value type, so that syntax does not test absence; use
`IsNone`, `IsSome`, or compare with `Option<T>.None`. Nullable reference and
nullable value comparisons remain unaffected.

`MT0002` warns when an instance or multi-input `Map` produces another `Result`
or `Option`, or when `MapError` produces a railway in the error slot. These
create nested railway values where `Bind` or `BindError` is usually intended.
Nested values can be legitimate data; suppress `MT0002` at those explicit
boundaries.

The diagnostic is intentionally opt-in. Consumers can configure its severity in
`.editorconfig` or suppress it at a documented boundary.

Apache-2.0. Developed with AI assistance.

<!-- BEGIN GENERATED API INDEX -->

[Repository API reference](https://github.com/czernous/monadic-types-dotnet/blob/HEAD/docs/api-reference.md)

Documented public members: 0

<!-- END GENERATED API INDEX -->
