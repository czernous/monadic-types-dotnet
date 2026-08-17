using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MonadicTypes.AspNetCore.OpenApi;

namespace MonadicTypes.AspNetCore.Tests;

public sealed class OpenApiCompatibilityTests
{
    [Fact]
    public async Task GeneratedDocumentIncludesMinimalAndControllerProblemResponses()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddErrorCatalogOpenApi();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(OpenApiCompatibilityController).Assembly);

        await using WebApplication app = builder.Build();
        Assert.Null(app.Services.GetService<IProblemDetailsService>());
        app.MapOpenApi();
        app.MapGet("/minimal/{id:int}", (int id) => TypedResults.Ok(id))
            .ProducesErrorCatalog(
                new(ErrorType.NotFound, "ITEM_NOT_FOUND", "The item was not found."),
                new(ErrorType.Conflict, "ITEM_CONFLICT", "The item conflicts with existing state."));
        app.MapControllers();
        await app.StartAsync();

        string document = await app.GetTestClient().GetStringAsync("/openapi/v1.json");
        using JsonDocument json = JsonDocument.Parse(document);

        AssertProblemResponse(json.RootElement, "/minimal/{id}", "404");
        AssertProblemResponse(json.RootElement, "/minimal/{id}", "409");
        AssertProblemResponse(json.RootElement, "/compatibility/controller", "404");
        AssertCatalog(
            json.RootElement,
            "/minimal/{id}",
            "404",
            "ITEM_NOT_FOUND",
            "The item was not found.");
        AssertCatalog(
            json.RootElement,
            "/minimal/{id}",
            "409",
            "ITEM_CONFLICT",
            "The item conflicts with existing state.");
        AssertCatalog(
            json.RootElement,
            "/compatibility/controller",
            "404",
            "CONTROLLER_NOT_FOUND",
            "The controller resource was not found.");
    }

    [Fact]
    public async Task GeneratedDocumentRejectsDuplicateCodesAcrossStatuses()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddErrorCatalogOpenApi();

        await using WebApplication app = builder.Build();
        app.MapOpenApi();
        app.MapGet("/duplicate", static () => TypedResults.Ok(1))
            .WithMetadata(
                new ProducesErrorCatalogAttribute(
                    ErrorType.NotFound,
                    "DUPLICATE",
                    "Not found."),
                new ProducesErrorCatalogAttribute(
                    ErrorType.Conflict,
                    "DUPLICATE",
                    "Conflict."));
        await app.StartAsync();

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            app.GetTestClient().GetStringAsync("/openapi/v1.json"));

        Assert.Contains("duplicate code 'DUPLICATE'", exception.Message, StringComparison.Ordinal);
    }

    private static void AssertProblemResponse(JsonElement document, string path, string statusCode)
    {
        JsonElement response = document
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty(statusCode);

        Assert.True(response.GetProperty("content").TryGetProperty("application/problem+json", out _));
    }

    private static void AssertCatalog(
        JsonElement document,
        string path,
        string statusCode,
        string code,
        string description)
    {
        JsonElement mediaType = document
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty(statusCode)
            .GetProperty("content")
            .GetProperty("application/problem+json");
        JsonElement codes = mediaType
            .GetProperty("schema")
            .GetProperty("allOf")[1]
            .GetProperty("properties")
            .GetProperty("code")
            .GetProperty("enum");
        JsonElement example = mediaType.GetProperty("examples").GetProperty(code);

        Assert.Contains(codes.EnumerateArray(), value => string.Equals(
            value.GetString(),
            code,
            StringComparison.Ordinal));
        Assert.Equal(description, example.GetProperty("summary").GetString());
        Assert.Equal(code, example.GetProperty("value").GetProperty("code").GetString());
        Assert.Equal(int.Parse(statusCode, System.Globalization.CultureInfo.InvariantCulture),
            example.GetProperty("value").GetProperty("status").GetInt32());
        Assert.False(example.GetProperty("value").TryGetProperty("traceId", out _));
    }
}

[ApiController]
[Route("compatibility/controller")]
public sealed class OpenApiCompatibilityController : ControllerBase
{
    [HttpGet]
    [ProducesError(ErrorType.NotFound)]
    [ProducesErrorCatalog(
        ErrorType.NotFound,
        "CONTROLLER_NOT_FOUND",
        "The controller resource was not found.")]
    public ActionResult<int> Get() => Ok(1);
}
