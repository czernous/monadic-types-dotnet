using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MonadicTypes.AspNetCore.Tests;

public sealed class OpenApiCompatibilityTests
{
    [Fact]
    public async Task GeneratedDocumentIncludesMinimalAndControllerProblemResponses()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddOpenApi();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(OpenApiCompatibilityController).Assembly);

        await using WebApplication app = builder.Build();
        app.MapOpenApi();
        app.MapGet("/minimal/{id:int}", (int id) => TypedResults.Ok(id))
            .ProducesErrors(ErrorType.NotFound, ErrorType.Conflict);
        app.MapControllers();
        await app.StartAsync();

        string document = await app.GetTestClient().GetStringAsync("/openapi/v1.json");
        using JsonDocument json = JsonDocument.Parse(document);

        AssertProblemResponse(json.RootElement, "/minimal/{id}", "404");
        AssertProblemResponse(json.RootElement, "/minimal/{id}", "409");
        AssertProblemResponse(json.RootElement, "/compatibility/controller", "404");
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
}

[ApiController]
[Route("compatibility/controller")]
public sealed class OpenApiCompatibilityController : ControllerBase
{
    [HttpGet]
    [ProducesError(ErrorType.NotFound)]
    public ActionResult<int> Get() => Ok(1);
}
