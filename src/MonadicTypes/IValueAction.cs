namespace MonadicTypes;

public interface IValueAction<in T>
{
    void Invoke(T value);
}
