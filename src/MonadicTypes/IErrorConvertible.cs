namespace MonadicTypes;

public interface IErrorConvertible<out TError> where TError : notnull
{
    TError ToError();
}
