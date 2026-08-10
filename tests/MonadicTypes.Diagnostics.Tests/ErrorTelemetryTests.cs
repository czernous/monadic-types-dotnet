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
            tag.Key == "error.type" && Equals(tag.Value, "INVALID"));
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
            tag.Key == "exception.type" && Equals(tag.Value, cause.GetType().FullName));
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
