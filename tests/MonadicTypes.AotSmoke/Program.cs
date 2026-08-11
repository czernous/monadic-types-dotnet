using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes;
using MonadicTypes.Async;
using MonadicTypes.AspNetCore;
using MonadicTypes.Effects;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
WebApplication app = builder.Build();
app.MapGet("/items/{id:int}", static (int id) =>
    Result<int, Error>.Ok(id).ToHttpResult(TypedResults.Ok))
    .ProducesErrors(ErrorType.NotFound);

Result<int, Error> result = Result<int, Error>.Fail(
    Error.NotFound("ITEM_MISSING", "The item was not found."));
Results<Ok<int>, ProblemHttpResult> response = result.ToHttpResult(TypedResults.Ok);

using Activity activity = new("monadic-types-aot-smoke");
activity.Start();
ErrorTelemetry.Record(activity, result.Error);

Result<long, Error> composed = await Result<int, Error>.Ok(41)
    .MapAsync(AotOperations.Functions.IncrementAsync)
    .Map(static value => value * 2);
Result<int, Error> captured = Effect.Try<int, Error>(
    static () => 42,
    static exception => Error.Unexpected(exception));
Task<int> completedEffect = Task.FromResult(43);
Result<int, Error> capturedTask = await Effect.TryTaskAsync(
    completedEffect,
    static task => task,
    static (TimeoutException exception) => Error.Timeout(
        "SMOKE_TIMEOUT",
        "Smoke effect timed out.",
        cause: exception));

return response.Result is ProblemHttpResult { StatusCode: StatusCodes.Status404NotFound }
    && composed is { IsSuccess: true, Value: 84 }
    && captured is { IsSuccess: true, Value: 42 }
    && capturedTask is { IsSuccess: true, Value: 43 }
    ? 0
    : 1;

public static partial class AotOperations
{
    [GenerateValueFunction]
    public static ValueTask<long> IncrementAsync(int value) => ValueTask.FromResult(value + 1L);
}
