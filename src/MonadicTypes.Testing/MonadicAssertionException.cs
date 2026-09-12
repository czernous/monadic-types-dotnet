namespace MonadicTypes.Testing;

/// <summary>Exception thrown by framework-neutral MonadicTypes.NET test assertions.</summary>
public sealed class MonadicAssertionException(string message) : Exception(message);
