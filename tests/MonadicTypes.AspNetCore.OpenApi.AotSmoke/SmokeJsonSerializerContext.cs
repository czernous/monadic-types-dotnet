using System.Text.Json.Serialization;

namespace MonadicTypes.AspNetCore.OpenApi.AotSmoke;

[JsonSerializable(typeof(int))]
internal sealed partial class SmokeJsonSerializerContext : JsonSerializerContext;
