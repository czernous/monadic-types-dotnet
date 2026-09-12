using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MonadicTypes.AspNetCore.Tests;

public class ErrorProblemDetailsTests
{
    [Fact]
    public void ExplicitStatus_ReusesProblemIdentityAcrossRequests()
    {
        Error error = Error.Validation("INVALID", "Invalid input.");
        ProblemDetails first = ErrorProblemDetails.CreateExample(error, 422);
        ProblemDetails second = ErrorProblemDetails.CreateExample(error, 422);

        Assert.Same(first.Type, second.Type);
        Assert.Same(first.Title, second.Title);
    }

    [Theory]
    [InlineData(400, "Bad Request")]
    [InlineData(401, "Unauthorized")]
    [InlineData(402, "Payment Required")]
    [InlineData(403, "Forbidden")]
    [InlineData(404, "Not Found")]
    [InlineData(405, "Method Not Allowed")]
    [InlineData(406, "Not Acceptable")]
    [InlineData(407, "Proxy Authentication Required")]
    [InlineData(408, "Request Timeout")]
    [InlineData(409, "Conflict")]
    [InlineData(410, "Gone")]
    [InlineData(411, "Length Required")]
    [InlineData(412, "Precondition Failed")]
    [InlineData(413, "Content Too Large")]
    [InlineData(414, "URI Too Long")]
    [InlineData(415, "Unsupported Media Type")]
    [InlineData(416, "Range Not Satisfiable")]
    [InlineData(417, "Expectation Failed")]
    [InlineData(421, "Misdirected Request")]
    [InlineData(422, "Unprocessable Content")]
    [InlineData(423, "Locked")]
    [InlineData(424, "Failed Dependency")]
    [InlineData(425, "Too Early")]
    [InlineData(426, "Upgrade Required")]
    [InlineData(428, "Precondition Required")]
    [InlineData(429, "Too Many Requests")]
    [InlineData(431, "Request Header Fields Too Large")]
    [InlineData(451, "Unavailable For Legal Reasons")]
    [InlineData(500, "Internal Server Error")]
    [InlineData(501, "Not Implemented")]
    [InlineData(502, "Bad Gateway")]
    [InlineData(503, "Service Unavailable")]
    [InlineData(504, "Gateway Timeout")]
    [InlineData(505, "HTTP Version Not Supported")]
    [InlineData(506, "Variant Also Negotiates")]
    [InlineData(507, "Insufficient Storage")]
    [InlineData(508, "Loop Detected")]
    [InlineData(510, "Not Extended")]
    [InlineData(511, "Network Authentication Required")]
    public void ExplicitStatus_UsesRegisteredHttpIdentityWithoutReinterpretingCategory(int status, string title)
    {
        Error error = Error.Custom(10_001, "VENDOR_REJECTED", "private diagnostic");
        DefaultHttpContext context = new() { TraceIdentifier = "request-42" };

        ProblemDetails details = ErrorProblemDetails.Create(error, status, context);
        ProblemHttpResult result = ErrorProblemDetails.ToHttpResult(error, status, context);
        ProblemDetails example = ErrorProblemDetails.CreateExample(error, status);

        Assert.Equal(status, details.Status);
        Assert.Equal(status, result.StatusCode);
        Assert.Equal(title, details.Title);
        Assert.Equal($"urn:problem-type:http-{status}", details.Type);
        Assert.Equal(details.Type, example.Type);
        Assert.Equal(title, example.Title);
        Assert.Null(details.Detail);
        Assert.Equal("VENDOR_REJECTED", details.Extensions["code"]);
        Assert.True(details.Extensions.ContainsKey("traceId"));
        Assert.False(example.Extensions.ContainsKey("traceId"));
        Assert.Equal(500, ErrorProblemDetails.Create(error).Status);
        Assert.Equal(10_001, error.NumericType);
    }

    [Theory]
    [InlineData(418)]
    [InlineData(419)]
    [InlineData(599)]
    public void ExplicitUnassignedStatus_UsesDeterministicGenericTitle(int statusCode)
    {
        ProblemDetails details = ErrorProblemDetails.CreateExample(
            Error.Custom(statusCode, "CUSTOM", "Failure."),
            statusCode);

        Assert.Equal("HTTP error", details.Title);
        Assert.Equal($"urn:problem-type:http-{statusCode}", details.Type);
    }

    [Fact]
    public void ExplicitStatus_AcceptsEntireErrorRangeAndRejectsOtherResponses()
    {
        Error error = Error.Validation("INVALID", "Public explanation.");
        for (int status = 400; status <= 599; status++)
        {
            ProblemDetails details = ErrorProblemDetails.Create(error, status);
            Assert.Equal(status, details.Status);
            Assert.False(string.IsNullOrWhiteSpace(details.Title));
            Assert.Equal("Public explanation.", details.Detail);
        }

        foreach (int status in new[] { -1, 0, 200, 304, 399, 600, 10_001 })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ErrorProblemDetails.Create(error, status));
            Assert.Throws<ArgumentOutOfRangeException>(() => ErrorProblemDetails.CreateExample(error, status));
            Assert.Throws<ArgumentOutOfRangeException>(() => ErrorProblemDetails.ToHttpResult(error, status));
        }
    }

    [Fact]
    public void CreateExample_OmitsAmbientTraceIdentifier()
    {
        using System.Diagnostics.Activity activity = new("openapi-example");
        activity.Start();

        ProblemDetails details = ErrorProblemDetails.CreateExample(
            Error.NotFound("MISSING", "The value was not found."));

        Assert.False(details.Extensions.ContainsKey("traceId"));
    }

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

    [Theory]
    [InlineData(ErrorType.PaymentRequired, 402, "payment-required")]
    [InlineData(ErrorType.NotAcceptable, 406, "not-acceptable")]
    [InlineData(ErrorType.RequestTimeout, 408, "request-timeout")]
    [InlineData(ErrorType.Gone, 410, "gone")]
    [InlineData(ErrorType.PreconditionFailed, 412, "precondition-failed")]
    [InlineData(ErrorType.ContentTooLarge, 413, "content-too-large")]
    [InlineData(ErrorType.UnsupportedMediaType, 415, "unsupported-media-type")]
    [InlineData(ErrorType.UnprocessableContent, 422, "unprocessable-content")]
    [InlineData(ErrorType.Locked, 423, "locked")]
    [InlineData(ErrorType.PreconditionRequired, 428, "precondition-required")]
    [InlineData(ErrorType.BadGateway, 502, "bad-gateway")]
    [InlineData(ErrorType.NotImplemented, 501, "not-implemented")]
    public void DefaultMapping_CoversIssue22Categories(ErrorType type, int expected, string slug)
    {
        Error error = new(type, "TEST", "failure");

        ProblemDetails details = ErrorProblemDetails.Create(error);

        Assert.Equal(expected, details.Status);
        Assert.Equal($"urn:problem-type:{slug}", details.Type);
    }

    [Fact]
    public void CustomHttpNumericType_ControlsStatusAndProblemIdentity()
    {
        Error error = Error.Custom(413, "BODY_TOO_LARGE", "request body is too large", isMessagePublic: true);

        ProblemDetails details = ErrorProblemDetails.Create(error);

        Assert.Equal(413, details.Status);
        Assert.Equal("Content Too Large", details.Title);
        Assert.Equal("urn:problem-type:http-413", details.Type);
        Assert.Equal("request body is too large", details.Detail);
    }

    [Fact]
    public void CustomNonHttpNumericType_RetainsGenericCustomMapping()
    {
        Error error = Error.Custom(10_001, "VENDOR_REJECTED", "private diagnostic");

        ProblemDetails details = ErrorProblemDetails.Create(error);

        Assert.Equal(500, details.Status);
        Assert.Equal("urn:problem-type:unexpected", details.Type);
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
