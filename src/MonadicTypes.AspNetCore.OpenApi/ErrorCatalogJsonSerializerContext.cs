using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace MonadicTypes.AspNetCore.OpenApi;

[JsonSerializable(typeof(ProblemDetails))]
internal sealed partial class ErrorCatalogJsonSerializerContext : JsonSerializerContext;
