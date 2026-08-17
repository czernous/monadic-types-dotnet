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

    [Fact]
    public void CatalogMetadata_CopiesEntriesAndRejectsDuplicateCodes()
    {
        ErrorCatalogEntry[] entries =
        [
            new(ErrorType.NotFound, "MISSING", "The value was not found.")
        ];
        ErrorCatalogMetadata metadata = new(entries);
        entries[0] = new(ErrorType.Conflict, "CHANGED", "The caller changed its array.");

        Assert.Equal("MISSING", metadata.AsSpan()[0].Code);
        Assert.Throws<ArgumentException>(() => new ErrorCatalogMetadata(
        [
            new(ErrorType.NotFound, "DUPLICATE", "First."),
            new(ErrorType.Conflict, "DUPLICATE", "Second.")
        ]));
    }

    [Fact]
    public void CatalogMetadata_RejectsEmptyAndUninitializedEntries()
    {
        Assert.Throws<ArgumentException>(() => new ErrorCatalogMetadata([]));
        Assert.Throws<ArgumentException>(() => new ErrorCatalogMetadata(
        [
            default
        ]));
    }

    [Fact]
    public void MetadataConstructors_RejectUndefinedErrorTypes()
    {
        const ErrorType undefined = (ErrorType)byte.MaxValue;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ErrorCatalogEntry(undefined, "UNDEFINED", "Undefined category."));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProducesErrorAttribute(undefined));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ProducesErrorCatalogAttribute(undefined, "UNDEFINED", "Undefined category."));
    }
}
