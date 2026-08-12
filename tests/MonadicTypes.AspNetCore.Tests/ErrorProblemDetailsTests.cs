using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MonadicTypes.AspNetCore.Tests;

public class ErrorProblemDetailsTests
{
    [Fact]
    public void Create_DoesNotExposePrivateMessageOrCause()
    {
        Error error = Error.Unexpected(new InvalidOperationException("database password leaked"));

        var details = ErrorProblemDetails.Create(error);

        Assert.Equal(StatusCodes.Status500InternalServerError, details.Status);
        Assert.Null(details.Detail);
        Assert.Equal("UNEXPECTED_FAILURE", details.Extensions["code"]);
        Assert.DoesNotContain("password", details.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_ExposesExplicitlyPublicMessage()
    {
        Error error = Error.NotFound("CUSTOMER_NOT_FOUND", "Customer 42 was not found.");

        var details = ErrorProblemDetails.Create(error);

        Assert.Equal(StatusCodes.Status404NotFound, details.Status);
        Assert.Equal("Customer 42 was not found.", details.Detail);
        Assert.Equal("urn:problem-type:not-found", details.Type);
    }

    [Fact]
    public void ToHttpResult_AllowsCallerOwnedProblemDetails()
    {
        Result<int, Error> result = Error.Conflict("VERSION", "Version mismatch");

        Results<Ok<int>, ProblemHttpResult> mapped = result.ToHttpResult(
            TypedResults.Ok,
            static error => TypedResults.Problem(
                statusCode: StatusCodes.Status418ImATeapot,
                title: error.Code));

        ProblemHttpResult problem = Assert.IsType<ProblemHttpResult>(mapped.Result);
        Assert.Equal(StatusCodes.Status418ImATeapot, problem.StatusCode);
        Assert.Equal("VERSION", problem.ProblemDetails.Title);
    }

    [Fact]
    public void ToHttpResult_AcceptsAllocationFreeStructMapper()
    {
        Result<int, Error> result = Error.NotFound("MISSING", "Missing");

        Results<Ok<int>, NotFound> mapped = result.ToHttpResult<int, Error, Ok<int>, NotFound, NotFoundMapper>(
            TypedResults.Ok,
            default);

        Assert.IsType<NotFound>(mapped.Result);
    }

    [Fact]
    public void ToHttpResult_MapsAnyResultErrorType()
    {
        Result<int, DomainFailure> result = Result<int, DomainFailure>.Fail(
            new DomainFailure("ORDER_MISSING"));

        Results<Ok<int>, NotFound<string>> mapped = result.ToHttpResult(
            TypedResults.Ok,
            static failure => TypedResults.NotFound(failure.Code));

        NotFound<string> notFound = Assert.IsType<NotFound<string>>(mapped.Result);
        Assert.Equal("ORDER_MISSING", notFound.Value);
    }

    [Fact]
    public void ToHttpResult_AutomaticallyConvertsDomainErrorWithoutNarrowing()
    {
        Result<int, ConvertibleDomainFailure> result = Result<int, ConvertibleDomainFailure>.Fail(
            new ConvertibleDomainFailure("ORDER_MISSING"));

        Results<Ok<int>, ProblemHttpResult> mapped = result.ToHttpResult(TypedResults.Ok);

        ProblemHttpResult problem = Assert.IsType<ProblemHttpResult>(mapped.Result);
        Assert.Equal(StatusCodes.Status404NotFound, problem.StatusCode);
        Assert.Equal("ORDER_MISSING", problem.ProblemDetails.Extensions["code"]);
    }

    [Fact]
    public void ToHttpResult_CustomFailureCanReturnATypedStatusUnion()
    {
        Result<int, DomainFailure> result = Result<int, DomainFailure>.Fail(
            new DomainFailure("ORDER_CONFLICT"));

        Results<Ok<int>, Results<NotFound<string>, Conflict<string>>> mapped = result.ToHttpResult(
            TypedResults.Ok,
            MapDomainFailure);

        var failures = Assert.IsType<Results<NotFound<string>, Conflict<string>>>(mapped.Result);
        Assert.IsType<Conflict<string>>(failures.Result);
    }

    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.RateLimited, StatusCodes.Status429TooManyRequests)]
    [InlineData(ErrorType.Unavailable, StatusCodes.Status503ServiceUnavailable)]
    [InlineData(ErrorType.Unexpected, StatusCodes.Status500InternalServerError)]
    public void DefaultMapping_SelectsStatusFromErrorType(ErrorType type, int expected)
    {
        Error error = new(type, "TEST", "failure");

        Assert.Equal(expected, ErrorProblemDetails.ToHttpResult(error).StatusCode);
    }

    [Fact]
    public void ValidationErrors_MapToTypedValidationProblemWithCodes()
    {
        Result<int, ValidationErrors> result = Result<int, ValidationErrors>.Fail(new ValidationErrors(
            new ValidationIssue("email", "REQUIRED", "Email is required."),
            new ValidationIssue("email", "FORMAT", "Email is invalid.")));

        Results<Ok<int>, ValidationProblem> mapped = result.ToHttpResult(TypedResults.Ok);

        ValidationProblem problem = Assert.IsType<ValidationProblem>(mapped.Result);
        Assert.Equal(
            ["Email is required.", "Email is invalid."],
            problem.ProblemDetails.Errors["email"]);
        var codes = Assert.IsType<Dictionary<string, string[]>>(problem.ProblemDetails.Extensions["codes"]);
        Assert.Equal(["REQUIRED", "FORMAT"], codes["email"]);
    }

    private readonly record struct DomainFailure(string Code);

    private readonly record struct ConvertibleDomainFailure(string Code) : IErrorConvertible<Error>
    {
        public Error ToError() => Error.NotFound(Code, "Order was not found.");
    }

    private static Results<NotFound<string>, Conflict<string>> MapDomainFailure(DomainFailure failure) =>
        failure.Code switch
        {
            "ORDER_MISSING" => TypedResults.NotFound(failure.Code),
            _ => TypedResults.Conflict(failure.Code)
        };

    private readonly struct NotFoundMapper : IHttpResultMapper<Error, NotFound>
    {
        public NotFound Map(in Error failure, HttpContext? httpContext) => TypedResults.NotFound();
    }
}
