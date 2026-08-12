using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes;
using MonadicTypes.AspNetCore;
using MonadicTypes.Async;
using MonadicTypes.Effects;
using MonadicTypes.PackageSmoke;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
WebApplication app = builder.Build();
app.MapGet("/items/{id:int}", static (int id) =>
    Result<int, Error>.Ok(id).ToHttpResult(TypedResults.Ok))
    .ProducesErrors(ErrorType.NotFound);

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

return response.Result is ProblemHttpResult { StatusCode: StatusCodes.Status404NotFound }
    && composed is { IsSuccess: true, Value: 84 }
    && captured is { IsSuccess: true, Value: 42 }
    ? 0
    : 1;
