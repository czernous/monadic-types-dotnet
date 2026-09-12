using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace MonadicTypes.AspNetCore.Tests;

public class ProducesErrorMetadataTests
{
    [Fact]
    public void CatalogEntry_RetainsCompactStorageWithStatusOverrides()
    {
        // Preserve the accepted array ownership cost: two references plus one
        // pointer-sized slot for category and optional HTTP status.
        Assert.Equal(3 * IntPtr.Size, System.Runtime.CompilerServices.Unsafe.SizeOf<ErrorCatalogEntry>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(304)]
    [InlineData(399)]
    [InlineData(600)]
    public void MetadataConstructors_RejectNonErrorStatusOverrides(int status)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ErrorCatalogEntry(ErrorType.Custom, "CUSTOM", "Failure.", status));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProducesErrorAttribute(ErrorType.Custom, status));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ProducesErrorCatalogAttribute(ErrorType.Custom, "CUSTOM", "Failure.", status));
    }

    [Fact]
    public void MetadataConstructors_AcceptEntireHttpErrorRange()
    {
        for (int status = 400; status <= 599; status++)
        {
            ErrorCatalogEntry entry = new(ErrorType.Custom, "CUSTOM", "Failure.", status);
            IProducesResponseTypeMetadata response = new ProducesErrorAttribute(ErrorType.Custom, status);
            ProducesErrorCatalogAttribute catalog = new(ErrorType.Custom, "CUSTOM", "Failure.", status);

            Assert.Equal(status, entry.StatusCode);
            Assert.Equal(status, response.StatusCode);
            Assert.Equal(status, catalog.StatusCode);
        }
    }

    [Fact]
    public void ProducesErrors_RejectsInvalidCategoryEvenWhenItsBitCollides()
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        using var app = builder.Build();
        var endpoint = Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions.MapGet(app, "/invalid", static () => "ok");

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            endpoint.ProducesErrors(ErrorType.Failure, (ErrorType)33));
    }

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
    public void CatalogMetadata_ValidatesLargerCatalogs()
    {
        ErrorCatalogEntry[] entries =
        [
            new(ErrorType.NotFound, "MISSING_01", "The value was not found."),
            new(ErrorType.NotFound, "MISSING_02", "The value was not found."),
            new(ErrorType.NotFound, "MISSING_03", "The value was not found."),
            new(ErrorType.NotFound, "MISSING_04", "The value was not found."),
            new(ErrorType.NotFound, "MISSING_05", "The value was not found."),
            new(ErrorType.NotFound, "MISSING_06", "The value was not found."),
            new(ErrorType.NotFound, "MISSING_07", "The value was not found."),
            new(ErrorType.NotFound, "MISSING_08", "The value was not found."),
            new(ErrorType.NotFound, "MISSING_09", "The value was not found.")
        ];

        ErrorCatalogMetadata metadata = new(entries);

        Assert.Equal(entries.Length, metadata.Count);
        Assert.Equal("MISSING_09", metadata.AsSpan()[^1].Code);
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
