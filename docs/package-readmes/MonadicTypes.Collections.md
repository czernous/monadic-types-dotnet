# MonadicTypes.NET Collections

Count-known, fail-fast collection traversal for `MonadicTypes.NET`, without
`IEnumerable<T>` iterators, reflection, builders, or hidden resizing.

## Install

```bash
dotnet add package MonadicTypes.NET.Collections --prerelease
```

## Traverse

```csharp
using MonadicTypes;
using MonadicTypes.Collections;

Result<User[], LookupError> users = ids.TraverseToArray(LoadUser);

Result<User[], LookupError> withoutCapture = ids.TraverseToArray(
    repository,
    static (id, state) => state.Load(id));

ReadOnlySpan<UserId> contiguousIds = ids;
Result<User[], LookupError> contiguous = contiguousIds.TraverseToArray(LoadUser);
```

`TraverseToArray` accepts `IReadOnlyList<T>` or `ReadOnlySpan<T>` and invokes the
selector once per item until the first failure. Empty input returns the shared empty array.
Non-empty input allocates exactly one owned output array, including when a later
item fails. Delegate, caller-state, and struct-callable overloads share the same
branch behavior.

## Sequence

```csharp
ReadOnlySpan<Result<User, LookupError>> results = loaded;
Result<User[], LookupError> users = results.SequenceToArray();
```

`SequenceToArray` preserves order and the first failure. It has the same empty
and one-array allocation contract as traversal.

The package does not catch selector or indexer exceptions. Exceptions propagate
unchanged with their original stack; use `MonadicTypes.NET.Effects` at an
intentional exception boundary. Uninitialized Result values throw.

## Performance

The initial list NativeAOT benchmark measured delegate and caller-state traversal
within 2% of an allocation-equivalent manual loop. Struct-callable traversal was
13% faster. Every row allocated the same `88 B` eight-element output array and
no wrapper allocation.

The subsequent span benchmark retained the same `88 B` owned array and measured
the struct-callable path at 19.918 ns, 47.1% faster than the same-run list
struct-callable path. Measurements are host-specific regression evidence rather
than universal throughput guarantees.

Apache-2.0. Developed with AI assistance.

<!-- BEGIN GENERATED API INDEX -->

[Complete API reference](https://github.com/czernous/monadic-types-dotnet/blob/HEAD/docs/api-reference.md#package-monadictypesnetcollections)

Documented public members: 8

<!-- END GENERATED API INDEX -->
