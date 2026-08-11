namespace MonadicTypes.Effects;

internal static class ExceptionFilter
{
    public static bool IsRecoverable(Exception exception) => exception is not (
        OperationCanceledException or
        OutOfMemoryException or
        AccessViolationException or
        AppDomainUnloadedException or
        BadImageFormatException);
}
