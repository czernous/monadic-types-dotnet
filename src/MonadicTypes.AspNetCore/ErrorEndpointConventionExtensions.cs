using Microsoft.AspNetCore.Builder;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

/// <summary>Provides reflection-free error response metadata for Minimal API endpoints.</summary>
public static class ErrorEndpointConventionExtensions
{
    extension<TBuilder>(TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        /// <summary>Adds one problem response metadata entry for each error category.</summary>
        /// <example><code>app.MapGet("/users/{id}", GetUser).ProducesErrors(ErrorType.NotFound, ErrorType.Unavailable);</code></example>
        /// <param name="errorTypes">The categories the endpoint can return.</param>
        /// <returns>The same endpoint builder for continued convention composition.</returns>
        public TBuilder ProducesErrors(params ReadOnlySpan<ErrorType> errorTypes)
        {
            uint seenTypes = 0;
            foreach (ErrorType errorType in errorTypes)
            {
                if (errorType is < ErrorType.Failure or > ErrorType.NotImplemented)
                {
                    throw new ArgumentOutOfRangeException(nameof(errorTypes), errorType, "Invalid error category.");
                }

                uint bit = 1u << (int)errorType;
                if ((seenTypes & bit) is 0)
                {
                    seenTypes |= bit;
                    builder.WithMetadata(new ProducesErrorAttribute(errorType));
                }
            }

            return builder;
        }

        /// <summary>
        /// Adds stable error-code metadata and corresponding problem responses to an endpoint.
        /// </summary>
        /// <example><code>app.MapGet("/users/{id}", GetUser).ProducesErrorCatalog(new(ErrorType.NotFound, "USER_NOT_FOUND", "User not found."));</code></example>
        /// <param name="entries">The public errors the endpoint can return.</param>
        /// <returns>The same endpoint builder for continued convention composition.</returns>
        public TBuilder ProducesErrorCatalog(params ReadOnlySpan<ErrorCatalogEntry> entries)
        {
            ErrorCatalogMetadata catalog = new(entries);
            builder.WithMetadata(catalog);

            ReadOnlySpan<ErrorCatalogEntry> ownedEntries = catalog.AsSpan();
            Span<ulong> seenStatuses = stackalloc ulong[4];
            seenStatuses.Clear();
            for (int index = 0; index < ownedEntries.Length; index++)
            {
                ErrorCatalogEntry entry = ownedEntries[index];
                int status = entry.StatusCode ?? ErrorProblemDetails.GetStatusCode(entry.Type);
                int offset = status - 400;
                ulong bit = 1UL << (offset & 63);
                ref ulong seen = ref seenStatuses[offset >> 6];
                if ((seen & bit) is 0)
                {
                    seen |= bit;
                    builder.WithMetadata(new ProducesErrorAttribute(entry.Type, status));
                }
            }

            return builder;
        }
    }
}
