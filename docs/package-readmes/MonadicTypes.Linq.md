# MonadicTypes.NET LINQ

Opt-in C# LINQ names and query-expression support for `Result<T,E>` and
`Option<T>`. The core package does not reference this assembly.

## Install

```bash
dotnet add package MonadicTypes.NET.Linq --prerelease
```

## Fluent Syntax

```csharp
using MonadicTypes;
using MonadicTypes.Linq;

Result<Invoice, LoadError> invoice = LoadUser(id).SelectMany(
    static user => LoadAccount(user.AccountId),
    static (user, account) => new Invoice(user, account));

Option<UserView> active = optionalUser
    .Where(static user => user.IsActive)
    .Select(static user => new UserView(user.Id, user.Name));
```

`Select` maps the active value. `SelectMany` binds and projects two active
values. Option `Where` filters Some; Result intentionally has no `Where` because
a failed predicate needs an explicit error value.

## Query Syntax

```csharp
Result<Invoice, LoadError> invoice =
    from user in LoadUser(id)
    from account in LoadAccount(user.AccountId)
    select new Invoice(user, account);
```

Both forms short-circuit Result failure and Option None. Operators do not catch
callback exceptions, and uninitialized Results throw.

Fluent method syntax is recommended by default. Initial NativeAOT measurements
were `2.095 ns` fluent versus `2.355 ns` query syntax for `Select`, and
`4.509 ns` versus `4.952 ns` for `SelectMany`. All rows allocated `0 B`.

Apache-2.0. Developed with AI assistance.
