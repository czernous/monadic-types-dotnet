using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using MonadicTypes;
using MonadicTypes.AspNetCore;
using MonadicTypes.AspNetCore.OpenApi;
using MonadicTypes.AspNetCore.OpenApi.AotSmoke;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.ConfigureHttpJsonOptions(static options =>
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, SmokeJsonSerializerContext.Default));
builder.Services.AddErrorCatalogOpenApi();

await using WebApplication app = builder.Build();
app.MapGet("/items/{id:int}", static (int id) => TypedResults.Ok(id))
    .ProducesErrorCatalog(
        new ErrorCatalogEntry(ErrorType.NotFound, "ITEM_NOT_FOUND", "The item was not found."),
        new ErrorCatalogEntry(
            ErrorType.Conflict,
            "ITEM_CONFLICT",
            "The item conflicts with existing state."));
app.MapOpenApi();

await app.StartAsync();
string? address = null;
foreach (string candidate in app.Urls)
{
    address = candidate;
    break;
}

if (address is null)
{
    return 1;
}

using HttpClient client = new();
using JsonDocument document = JsonDocument.Parse(
    await client.GetStringAsync($"{address}/openapi/v1.json"));
JsonElement operation = document.RootElement
    .GetProperty("paths")
    .GetProperty("/items/{id}")
    .GetProperty("get");
JsonElement notFoundCodes = operation
    .GetProperty("responses")
    .GetProperty("404")
    .GetProperty("content")
    .GetProperty("application/problem+json")
    .GetProperty("schema")
    .GetProperty("allOf")[1]
    .GetProperty("properties")
    .GetProperty("code")
    .GetProperty("enum");

await app.StopAsync();

return notFoundCodes[0].GetString() is "ITEM_NOT_FOUND" ? 0 : 1;
