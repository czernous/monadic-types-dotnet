# MonadicTypes.NET.AspNetCore.OpenApi

Explicit error-catalog documentation for `MonadicTypes.NET.AspNetCore`. The
adapter reads endpoint metadata and adds status-scoped error-code enums and
RFC problem examples to ASP.NET Core OpenAPI documents.

## Install

```bash
dotnet add package MonadicTypes.NET.AspNetCore.OpenApi --prerelease
```

The package pins compatible Microsoft OpenAPI assemblies but does not silently
activate Microsoft's XML-comment source generator. That generator emits
reflection-based document transformers and remains a direct, intentional
application choice.

The package includes the `MTAPI001` compatibility analyzer and a consumer build
target. The target removes only a transitive Microsoft XML generator. A direct
application reference to `Microsoft.AspNetCore.OpenApi` is treated as explicit
opt-in and is preserved.

## Configure

```csharp
using MonadicTypes;
using MonadicTypes.AspNetCore;
using MonadicTypes.AspNetCore.OpenApi;

builder.Services.AddErrorCatalogOpenApi();

app.MapGet("/items/{id:int}", GetItem)
    .WithSummary("Gets an item.")
    .WithDescription("Returns the requested item when it exists.")
    .ProducesErrorCatalog(
        new ErrorCatalogEntry(ErrorType.NotFound, "ITEM_NOT_FOUND", "The item was not found."),
        new ErrorCatalogEntry(
            ErrorType.Unavailable,
            "STORE_UNAVAILABLE",
            "The item store is unavailable."));
```

### NativeAOT JSON metadata

ASP.NET Core's OpenAPI schema exporter requires JSON metadata for application
endpoint types, including primitive route parameters such as `int`. A
reflection-free NativeAOT application must register those types in its own
`JsonSerializerContext`; this package cannot discover arbitrary application
types without reflection. `AddErrorCatalogOpenApi()` supplies package-owned,
source-generated metadata for the problem payload returned by
`ProblemHttpResult`; it does not register ASP.NET Core problem-details services.

```csharp
using System.Text.Json.Serialization;

builder.Services.ConfigureHttpJsonOptions(static options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(
        0,
        ApiJsonSerializerContext.Default));

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(ItemResponse))]
internal sealed partial class ApiJsonSerializerContext : JsonSerializerContext;
```

Register every request, response, and bound parameter type for which ASP.NET
Core produces a schema. Missing metadata fails document generation rather than
silently enabling reflection. The package's NativeAOT smoke test starts a real
server and generates a document under this profile.

Catalog descriptions are public API documentation. Do not construct them from
private diagnostic messages or retained exceptions. The transformer runs only
while producing an OpenAPI document and does not participate in request
execution.

## API

- `AddErrorCatalogOpenApi()` registers default-document services, the
  reflection-free transformer, and package-owned `ProblemHttpResult` JSON
  metadata without registering problem-details services.
- `AddErrorCatalogs()` registers only the transformer on `OpenApiOptions` for
  advanced or named-document configuration. Such callers must also register
  equivalent `ProblemDetails` JSON metadata in their application context.
- `ProducesErrorCatalog(...)`, from `MonadicTypes.NET.AspNetCore`, copies and
  validates endpoint entries before this transformer reads them.
- `[ProducesErrorCatalog(...)]` provides the same per-entry metadata for
  controllers.
- `ErrorProblemDetails.CreateExample(...)` produces the deterministic,
  trace-free problem shape used by examples.

Entries are grouped by the HTTP status derived from `ErrorType`. Each response
receives an ordinal code enum and one named problem example per code. Duplicate
codes across the complete endpoint are rejected rather than merged ambiguously,
including codes attached to different statuses. Catalog metadata is a cold
document/route-registration concern and is not read on request execution.

## XML Comments

The bundled `MTAPI001` analyzer appears in Rider and Visual Studio when a
documented Minimal API handler has neither explicit summary/description
metadata nor Microsoft's XML projection.

Choose intentionally:

- Stay reflection-free by attaching standard ASP.NET Core metadata and error
  catalogs explicitly.
- Directly reference the pinned `Microsoft.AspNetCore.OpenApi` package to use
  Microsoft's complete XML-comment behavior, accepting its document-generation
  reflection.
- Generate the document in a separate build/docs process and serve the static
  artifact so reflection is absent from the production execution path.

For the direct opt-in, add the Microsoft package to the application project,
not to a shared library that might enable it unintentionally for consumers:

```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.10" />
```

No MonadicTypes configuration switch is required. The direct package reference
is the opt-in signal and supplies Microsoft's analyzer/build assets.

## Compatibility

The runtime adapter contains no reflection, assembly scanning, service
location, or runtime code generation. Application endpoint types still follow
ASP.NET Core's source-generated JSON metadata contract described above.
Microsoft OpenAPI compatibility is pinned and tested separately because its
behavior and dependencies are third-party contracts.

Apache-2.0. Developed with AI assistance.
