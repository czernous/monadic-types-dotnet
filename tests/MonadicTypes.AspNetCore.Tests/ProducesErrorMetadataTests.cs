using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace MonadicTypes.AspNetCore.Tests;

public class ProducesErrorMetadataTests
{
    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Unexpected, 500)]
    public void Attribute_ProvidesStandardOpenApiResponseMetadata(ErrorType type, int statusCode)
    {
        IProducesResponseTypeMetadata metadata = new ProducesErrorAttribute(type);

        Assert.Equal(statusCode, metadata.StatusCode);
        Assert.Equal(typeof(ProblemDetails), metadata.Type);
        Assert.Contains("application/problem+json", metadata.ContentTypes);
    }
}
