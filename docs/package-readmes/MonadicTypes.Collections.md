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
```

`TraverseToArray` accepts `IReadOnlyList<T>` and invokes the selector once per
item until the first failure. Empty input returns the shared empty array.
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

The initial NativeAOT benchmark measured delegate and caller-state traversal
within 2% of an allocation-equivalent manual loop. Struct-callable traversal was
13% faster. Every row allocated the same `88 B` eight-element output array and
no wrapper allocation.

Apache-2.0. Developed with AI assistance.
