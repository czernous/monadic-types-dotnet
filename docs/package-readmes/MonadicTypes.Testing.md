# MonadicTypes.NET.Testing

Framework-neutral assertions for `MonadicTypes.NET` tests. The package has no
dependency on xUnit, NUnit, MSTest, or any other test framework; it provides a
small assertion exception that each test framework can report normally.

## Install

```bash
dotnet add package MonadicTypes.NET.Testing --prerelease
```

## Quick Start

```csharp
using MonadicTypes.Testing;

Result<User, LookupError> result = LoadUser();
User user = result.ShouldBeOk("load user").ValueOrFail();
LookupError error = result.ShouldBeError().ErrorOrFail();
Option<User> optional = FindUser();
User present = optional.ShouldBeSome().ValueOrFail();
```

`ShouldBeOk`, `ShouldBeError`, `ShouldBeSome`, and `ShouldBeNone` return the
unchanged value so assertions can be composed with ordinary test-framework
assertions. `ValueOrFail` and `ErrorOrFail` throw
`MonadicAssertionException` with the actual case in the message. The helpers
use only public Result/Option APIs and do not affect production consumers.

Apache-2.0. Developed with AI assistance.

<!-- BEGIN GENERATED API INDEX -->

[Complete API reference](https://github.com/czernous/monadic-types-dotnet/blob/HEAD/docs/api-reference.md#package-monadictypesnettesting)

Documented public members: 10

<!-- END GENERATED API INDEX -->
