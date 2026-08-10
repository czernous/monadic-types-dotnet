using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes;
using MonadicTypes.AspNetCore;

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

return response.Result is ProblemHttpResult { StatusCode: StatusCodes.Status404NotFound }
    ? 0
    : 1;
