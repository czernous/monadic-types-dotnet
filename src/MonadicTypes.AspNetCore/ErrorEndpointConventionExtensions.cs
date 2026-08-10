using Microsoft.AspNetCore.Builder;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

public static class ErrorEndpointConventionExtensions
{
    extension<TBuilder>(TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
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
