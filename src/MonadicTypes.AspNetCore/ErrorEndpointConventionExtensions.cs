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
            uint seenTypes = 0;
            for (int index = 0; index < ownedEntries.Length; index++)
            {
                ErrorType type = ownedEntries[index].Type;
                uint bit = 1u << (int)type;
                if ((seenTypes & bit) is 0)
                {
                    seenTypes |= bit;
                    builder.WithMetadata(new ProducesErrorAttribute(type));
                }
            }

            return builder;
        }
    }
}
