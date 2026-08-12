using System.Diagnostics;

namespace MonadicTypes.Tests;

public class ErrorTelemetryTests
{
    [Fact]
    public void Record_IsANoOpWithoutAnActivity()
    {
        ErrorTelemetry.Record(null, default);
    }

    [Fact]
    public void Record_ExpectedErrorPreservesSpanStatus()
    {
        using Activity activity = new("request");
        activity.Start();

        ErrorTelemetry.Record(activity, Error.Validation("INVALID", "Invalid input"));

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        ActivityEvent errorEvent = Assert.Single(activity.Events);
        Assert.Equal("error", errorEvent.Name);
        Assert.Contains(activity.Tags, tag =>
            string.Equals(tag.Key, "error.type", StringComparison.Ordinal)
            && string.Equals(tag.Value, "INVALID", StringComparison.Ordinal));
    }

    [Fact]
    public void Record_UnexpectedExceptionMarksSpanAndAddsExceptionEvent()
    {
        using Activity activity = new("request");
        activity.Start();
        InvalidOperationException cause = new("database unavailable");

        ErrorTelemetry.Record(activity, Error.Unexpected(cause));

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        ActivityEvent exceptionEvent = Assert.Single(activity.Events);
        Assert.Equal("exception", exceptionEvent.Name);
        Assert.Contains(exceptionEvent.Tags, tag =>
            string.Equals(tag.Key, "exception.type", StringComparison.Ordinal)
            && tag.Value is string value
            && string.Equals(value, cause.GetType().FullName, StringComparison.Ordinal));
    }

    [Fact]
    public void Record_StatusPolicyCanPreserveUnexpectedSpanStatus()
    {
        using Activity activity = new("request");
        activity.Start();

        ErrorTelemetry.Record(
            activity,
            Error.Unexpected("failed"),
            ErrorActivityStatusPolicy.Preserve);

        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
    }
}
