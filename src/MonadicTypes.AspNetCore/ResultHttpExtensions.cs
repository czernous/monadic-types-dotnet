using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using MonadicTypes;

namespace MonadicTypes.AspNetCore;

/// <summary>Maps result branches to strongly typed ASP.NET Core HTTP results.</summary>
public static class ResultHttpExtensions
{
    extension<T>(in Result<T, Error> result)
    {
        /// <summary>Maps success with a delegate and structured failure with the default problem policy.</summary>
        public Results<TSuccess, ProblemHttpResult> ToHttpResult<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess>(
            Func<T, TSuccess> success,
            HttpContext? httpContext = null)
            where TSuccess : IResult => result.IsSuccess
                ? success(result.Value)
                : ErrorProblemDetails.ToHttpResult(result.Error, httpContext);

        /// <summary>Maps success with a value-function struct and structured failure with the default problem policy.</summary>
        public Results<TSuccess, ProblemHttpResult> ToHttpResult<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess,
            TSuccessMapper>(
            TSuccessMapper success,
            HttpContext? httpContext = null)
            where TSuccess : IResult
            where TSuccessMapper : struct, IValueFunction<T, TSuccess> => result.IsSuccess
                ? success.Invoke(result.Value)
                : ErrorProblemDetails.ToHttpResult(result.Error, httpContext);

    }

    extension<T>(in Result<T, ValidationErrors> result)
    {
        /// <summary>Maps success with a delegate and failures to a validation problem result.</summary>
        public Results<TSuccess, ValidationProblem> ToHttpResult<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess>(
            Func<T, TSuccess> success,
            HttpContext? httpContext = null)
            where TSuccess : IResult => result.IsSuccess
                ? success(result.Value)
                : ValidationErrorProblemDetails.ToHttpResult(result.Error, httpContext);

        /// <summary>Maps success with a value-function struct and failures to a validation problem result.</summary>
        public Results<TSuccess, ValidationProblem> ToHttpResult<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess,
            TSuccessMapper>(
            TSuccessMapper success,
            HttpContext? httpContext = null)
            where TSuccess : IResult
            where TSuccessMapper : struct, IValueFunction<T, TSuccess> => result.IsSuccess
                ? success.Invoke(result.Value)
                : ValidationErrorProblemDetails.ToHttpResult(result.Error, httpContext);
    }

    extension<T, TError>(in Result<T, TError> result)
        where TError : notnull, IErrorConvertible<Error>
    {
        /// <summary>Maps success with a delegate and converts a domain error to the default problem result.</summary>
        public Results<TSuccess, ProblemHttpResult> ToHttpResult<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess>(
            Func<T, TSuccess> success,
            HttpContext? httpContext = null)
            where TSuccess : IResult => result.IsSuccess
                ? success(result.Value)
                : ErrorProblemDetails.ToHttpResult(result.Error.ToError(), httpContext);
    }

    extension<T, TError>(in Result<T, TError> result) where TError : notnull
    {
        /// <summary>
        /// Fully caller-owned mapping path for any error type. Use this to return
        /// custom ProblemDetails, framework results, or application-specific results.
        /// </summary>
        public Results<TSuccess, TFailure> ToHttpResult<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TFailure>(
            Func<T, TSuccess> success,
            Func<TError, TFailure> failure)
            where TSuccess : IResult
            where TFailure : IResult => result.IsSuccess
                ? success(result.Value)
                : failure(result.Error);

        /// <summary>Maps failure with a value-type mapper while retaining a delegate success mapper.</summary>
        public Results<TSuccess, TFailure> ToHttpResult<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TFailure,
            TMapper>(
            Func<T, TSuccess> success,
            TMapper failure,
            HttpContext? httpContext = null)
            where TSuccess : IResult
            where TFailure : IResult
            where TMapper : struct, IHttpResultMapper<TError, TFailure> => result.IsSuccess
                ? success(result.Value)
                : failure.Map(result.Error, httpContext);

        /// <summary>Maps both branches through value-type mappers for allocation-free dispatch.</summary>
        public Results<TSuccess, TFailure> ToHttpResult<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TSuccess,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods |
                                        DynamicallyAccessedMemberTypes.NonPublicMethods)] TFailure,
            TSuccessMapper,
            TFailureMapper>(
            TSuccessMapper success,
            TFailureMapper failure,
            HttpContext? httpContext = null)
            where TSuccess : IResult
            where TFailure : IResult
            where TSuccessMapper : struct, IValueFunction<T, TSuccess>
            where TFailureMapper : struct, IHttpResultMapper<TError, TFailure> => result.IsSuccess
                ? success.Invoke(result.Value)
                : failure.Map(result.Error, httpContext);
    }
}
