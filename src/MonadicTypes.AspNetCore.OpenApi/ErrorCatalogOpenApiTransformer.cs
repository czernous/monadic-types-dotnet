using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MonadicTypes.AspNetCore.OpenApi;

internal sealed class ErrorCatalogOpenApiTransformer : IOpenApiOperationTransformer
{
    private const int MaxStackBuckets = 256;

    internal static ErrorCatalogOpenApiTransformer Instance { get; } = new();

    private ErrorCatalogOpenApiTransformer()
    {
    }

    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        IList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata;
        ValidateUniqueCodes(metadata);
        for (int metadataIndex = 0; metadataIndex < metadata.Count; metadataIndex++)
        {
            switch (metadata[metadataIndex])
            {
                case ErrorCatalogMetadata catalog:
                    AddCatalog(operation, catalog.AsSpan());
                    break;
                case ProducesErrorCatalogAttribute attribute:
                    AddEntry(operation, attribute.Entry);
                    break;
            }
        }

        return Task.CompletedTask;
    }

    private static void ValidateUniqueCodes(IList<object> metadata)
    {
        int entryCount = CountEntries(metadata);
        if (entryCount < 2)
        {
            return;
        }

        int bucketCount = GetBucketCount(entryCount);
        int[]? rented = null;
        Span<int> buckets = bucketCount <= MaxStackBuckets
            ? stackalloc int[bucketCount]
            : (rented = ArrayPool<int>.Shared.Rent(bucketCount)).AsSpan(0, bucketCount);
        buckets.Clear();

        try
        {
            int bucketMask = bucketCount - 1;
            int entryIndex = 0;
            var entries = new CatalogEntryEnumerator(metadata);
            while (entries.MoveNext())
            {
                ErrorCatalogEntry entry = entries.Current;
                int slot = StringComparer.Ordinal.GetHashCode(entry.Code) & bucketMask;
                while (buckets[slot] is not 0)
                {
                    ErrorCatalogEntry previous = GetEntryAt(metadata, buckets[slot] - 1);
                    if (string.Equals(previous.Code, entry.Code, StringComparison.Ordinal))
                    {
                        throw DuplicateCode(entry.Code);
                    }

                    slot = (slot + 1) & bucketMask;
                }

                buckets[slot] = ++entryIndex;
            }
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<int>.Shared.Return(rented);
            }
        }
    }

    private static int CountEntries(IList<object> metadata)
    {
        int count = 0;
        for (int index = 0; index < metadata.Count; index++)
        {
            count = checked(count + GetEntryCount(metadata[index]));
        }

        return count;
    }

    private static int GetBucketCount(int entryCount)
    {
        uint required = checked((uint)entryCount * 2u);
        uint bucketCount = BitOperations.RoundUpToPowerOf2(required);
        return bucketCount is 0 or > int.MaxValue
            ? throw new InvalidOperationException("The endpoint error catalog is too large.")
            : (int)bucketCount;
    }

    private static ErrorCatalogEntry GetEntryAt(IList<object> metadata, int targetIndex)
    {
        var entries = new CatalogEntryEnumerator(metadata);
        for (int index = 0; entries.MoveNext(); index++)
        {
            if (index == targetIndex)
            {
                return entries.Current;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(targetIndex));
    }

    private static int GetEntryCount(object metadata) => metadata switch
    {
        ErrorCatalogMetadata catalog => catalog.Count,
        ProducesErrorCatalogAttribute => 1,
        _ => 0
    };

    private static ErrorCatalogEntry GetEntry(object metadata, int index) => metadata switch
    {
        ErrorCatalogMetadata catalog => catalog.AsSpan()[index],
        ProducesErrorCatalogAttribute attribute when index == 0 => attribute.Entry,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    private static InvalidOperationException DuplicateCode(string code) =>
        new($"The endpoint error catalog contains duplicate code '{code}'.");

    private struct CatalogEntryEnumerator(IList<object> metadata)
    {
        private int _metadataIndex;
        private int _entryIndex;

        public ErrorCatalogEntry Current { get; private set; }

        public bool MoveNext()
        {
            while (_metadataIndex < metadata.Count)
            {
                object currentMetadata = metadata[_metadataIndex];
                if (_entryIndex < GetEntryCount(currentMetadata))
                {
                    Current = GetEntry(currentMetadata, _entryIndex++);
                    return true;
                }

                _metadataIndex++;
                _entryIndex = 0;
            }

            return false;
        }
    }

    private static void AddCatalog(OpenApiOperation operation, ReadOnlySpan<ErrorCatalogEntry> entries)
    {
        for (int index = 0; index < entries.Length; index++)
        {
            AddEntry(operation, entries[index]);
        }
    }

    private static void AddEntry(OpenApiOperation operation, in ErrorCatalogEntry entry)
    {
        string statusCode = ErrorProblemDetails
            .GetStatusCode(entry.Type)
            .ToString(CultureInfo.InvariantCulture);
        if (operation.Responses is null
            || !operation.Responses.TryGetValue(statusCode, out IOpenApiResponse? response)
            || response.Content is not { } content
            || !content.TryGetValue("application/problem+json", out OpenApiMediaType? mediaType))
        {
            return;
        }

        OpenApiSchema codeSchema = GetOrCreateCodeSchema(mediaType);
        codeSchema.Enum ??= [];
        for (int index = 0; index < codeSchema.Enum.Count; index++)
        {
            if (string.Equals(codeSchema.Enum[index]?.GetValue<string>(), entry.Code, StringComparison.Ordinal))
            {
                throw DuplicateCode(entry.Code);
            }
        }

        codeSchema.Enum.Add(JsonValue.Create(entry.Code));
        mediaType.Examples ??= new Dictionary<string, IOpenApiExample>(StringComparer.Ordinal);
        mediaType.Examples.Add(entry.Code, CreateExample(entry));
    }

    private static OpenApiSchema GetOrCreateCodeSchema(OpenApiMediaType mediaType)
    {
        if (mediaType.Schema is OpenApiSchema { AllOf: { Count: 2 } allOf }
            && allOf[1] is OpenApiSchema { Properties: var properties }
            && properties is not null
            && properties.TryGetValue("code", out IOpenApiSchema? existing)
            && existing is OpenApiSchema existingCode)
        {
            return existingCode;
        }

        OpenApiSchema codeSchema = new()
        {
            Type = JsonSchemaType.String,
            Description = "Stable machine-readable error code."
        };
        OpenApiSchema catalogSchema = new()
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal)
            {
                ["code"] = codeSchema
            },
            Required = new HashSet<string>(StringComparer.Ordinal) { "code" }
        };

        mediaType.Schema = new OpenApiSchema
        {
            AllOf = mediaType.Schema is { } generatedSchema
                ? [generatedSchema, catalogSchema]
                : [catalogSchema]
        };
        return codeSchema;
    }

    private static OpenApiExample CreateExample(in ErrorCatalogEntry entry)
    {
        Error error = entry.Type switch
        {
            ErrorType.Custom => Error.Custom(1, entry.Code, entry.Description, isMessagePublic: true),
            _ => new Error(entry.Type, entry.Code, entry.Description, isMessagePublic: true)
        };
        ProblemDetails details = ErrorProblemDetails.CreateExample(error);

        return new OpenApiExample
        {
            Summary = entry.Description,
            Value = new JsonObject
            {
                ["type"] = details.Type,
                ["title"] = details.Title,
                ["status"] = details.Status,
                ["detail"] = details.Detail,
                ["code"] = entry.Code
            }
        };
    }
}
