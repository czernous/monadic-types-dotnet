using Microsoft.AspNetCore.Builder;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

/// <summary>Provides reflection-free error response metadata for Minimal API endpoints.</summary>
public static class ErrorEndpointConventionExtensions
{
    extension<TBuilder>(TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        /// <summary>Adds one problem response metadata entry for each error category.</summary>
        /// <param name="errorTypes">The categories the endpoint can return.</param>
        /// <returns>The same endpoint builder for continued convention composition.</returns>
        public TBuilder ProducesErrors(params ReadOnlySpan<ErrorType> errorTypes)
        {
            foreach (ErrorType errorType in errorTypes)
            {
                builder.WithMetadata(new ProducesErrorAttribute(errorType));
            }

            return builder;
        }

        /// <summary>
        /// Adds stable error-code metadata and corresponding problem responses to an endpoint.
        /// </summary>
        /// <param name="entries">The public errors the endpoint can return.</param>
        /// <returns>The same endpoint builder for continued convention composition.</returns>
        public TBuilder ProducesErrorCatalog(params ReadOnlySpan<ErrorCatalogEntry> entries)
        {
            ErrorCatalogMetadata catalog = new(entries);
            builder.WithMetadata(catalog);

            ReadOnlySpan<ErrorCatalogEntry> ownedEntries = catalog.AsSpan();
            for (int index = 0; index < ownedEntries.Length; index++)
            {
                ErrorType type = ownedEntries[index].Type;
                bool firstCategory = true;
                for (int previous = 0; previous < index; previous++)
                {
                    firstCategory &= ownedEntries[previous].Type != type;
                }

                if (firstCategory)
                {
                    builder.WithMetadata(new ProducesErrorAttribute(type));
                }
            }

            return builder;
        }
    }
}
