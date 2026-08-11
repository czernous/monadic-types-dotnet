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
    }
}
