using Microsoft.AspNetCore.OpenApi;

namespace MonadicTypes.AspNetCore.OpenApi;

/// <summary>Registers MonadicTypes error-catalog document generation.</summary>
public static class OpenApiOptionsExtensions
{
    extension(OpenApiOptions options)
    {
        /// <summary>
        /// Adds the explicit endpoint error-catalog transformer without reflection or DI activation.
        /// </summary>
        /// <returns>The same options instance for continued configuration.</returns>
        public OpenApiOptions AddErrorCatalogs()
        {
            ArgumentNullException.ThrowIfNull(options);
            return options.AddOperationTransformer(ErrorCatalogOpenApiTransformer.Instance);
        }
    }
}
