using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes;
using MonadicTypes.AspNetCore;
using MonadicTypes.AspNetCore.OpenApi;
using MonadicTypes.Async;
using MonadicTypes.Collections;
using MonadicTypes.Effects;
using MonadicTypes.Linq;
using MonadicTypes.PackageSmoke;
using MonadicTypes.Testing;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddErrorCatalogOpenApi();
WebApplication app = builder.Build();
app.MapGet("/items/{id:int}", static (int id) =>
    Result<int, Error>.Ok(id).ToHttpResult(TypedResults.Ok))
    .ProducesErrorCatalog(
        new ErrorCatalogEntry(ErrorType.NotFound, "ITEM_MISSING", "The item was not found."));
app.MapGet("/documented", PackageOperations.DocumentedEndpoint);

Result<int, Error> failure = Result<int, Error>.Fail(
    Error.NotFound("ITEM_MISSING", "The item was not found."));
Results<Ok<int>, ProblemHttpResult> response = failure.ToHttpResult(TypedResults.Ok);

using Activity activity = new("monadic-types-package-smoke");
activity.Start();
ErrorTelemetry.Record(activity, failure.Error);

Result<long, Error> composed = await Result<int, Error>.Ok(41)
    .MapAsync(PackageOperations.Functions.IncrementAsync)
    .Map(static value => value * 2);
Result<int, Error> captured = Effect.Try<int, Error>(
    static () => 42,
    static exception => Error.Unexpected(exception));
int testedValue = Result<int, Error>.Ok(7).ShouldBeOk().ValueOrFail();
IReadOnlyList<int> values = new[] { 1, 2, 3 };
Result<int[], Error> traversed = values.TraverseToArray(
    static value => Result<int, Error>.Ok(value + 1));
Result<int, Error> selected = traversed.Select(static items => items[0]);

var generatedList = values.TraverseToArray(PackageOperations.Functions.Widen);
int[] spanValues = [1, 2, 3];
var generatedSpan = spanValues.AsSpan().TraverseToArray(PackageOperations.Functions.Widen);
var generatedOption = Option<int>.Some(41).Traverse(PackageOperations.Functions.Widen);
var required = Result<Option<int>, int>.Ok(Option<int>.None).RequireSome(7, static id => id);
var recovered = Result<int, string>.Fail("missing").Recover(
    42,
    static (error, state) => Result<int, string>.Ok(error.Length + state));
int fallback = Result<int, string>.Fail("missing").ValueOrElse(
    42,
    static (error, state) => error.Length + state);
Option<string> nullableReference = Option<int>.Some(1).MapNullable(static _ => (string?)null);
Option<int> nullableValue = Option<int>.Some(1).MapNullableValue(static _ => (int?)null);
Option<(int First, string Second)> zipped = Option<int>.Some(1).Zip(Option<string>.Some("two"));
ProblemHttpResult explicitResponse = ErrorProblemDetails.ToHttpResult(failure.Error, 410);
ErrorCatalogEntry explicitEntry = new(ErrorType.NotFound, "REMOVED", "The item was removed.", 410);

return response.Result is ProblemHttpResult { StatusCode: StatusCodes.Status404NotFound }
    && composed is { IsSuccess: true, Value: 84 }
    && captured is { IsSuccess: true, Value: 42 }
    && testedValue is 7
    && selected is { IsSuccess: true, Value: 2 }
    && generatedList.Value[2] is 4L
    && generatedSpan.Value[2] is 4L
    && generatedOption.Value.Value is 42L
    && required.Error is 7
    && recovered.Value is 49
    && fallback is 49
    && nullableReference.IsNone
    && nullableValue.IsNone
    && zipped.Value is (1, "two")
    && explicitResponse.StatusCode is 410
    && explicitResponse.ProblemDetails.Type is "urn:problem-type:http-410"
    && explicitEntry.StatusCode is 410
    ? 0
    : 1;
