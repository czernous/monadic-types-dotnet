using Microsoft.Extensions.DependencyInjection;

namespace MonadicTypes.AspNetCore.OpenApi;

/// <summary>Registers reflection-free error-catalog OpenAPI services.</summary>
public static class OpenApiServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds OpenAPI error-catalog transformation and source-generated JSON metadata for
        /// the problem payload returned by <c>ProblemHttpResult</c>.
        /// </summary>
        /// <returns>The same service collection for continued configuration.</returns>
        public IServiceCollection AddErrorCatalogOpenApi()
        {
            ArgumentNullException.ThrowIfNull(services);
            services.ConfigureHttpJsonOptions(static options =>
                options.SerializerOptions.TypeInfoResolverChain.Insert(
                    0,
                    ErrorCatalogJsonSerializerContext.Default));
            return services.AddOpenApi(static options => options.AddErrorCatalogs());
        }
    }
}
