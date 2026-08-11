namespace MonadicTypes;

/// <summary>Converts a domain-specific error into a wider error representation.</summary>
/// <typeparam name="TError">Wider error type.</typeparam>
public interface IErrorConvertible<out TError> where TError : notnull
{
    /// <summary>Creates the wider error representation.</summary>
    /// <returns>The converted error.</returns>
    TError ToError();
}
