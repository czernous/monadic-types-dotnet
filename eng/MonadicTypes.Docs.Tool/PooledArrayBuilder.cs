using System.Buffers;

namespace MonadicTypes.Tooling;

internal struct PooledArrayBuilder<T>
{
    private T[]? _items;
    private int _count;

    public void Add(T item)
    {
        if (_items is null || _count == _items.Length)
        {
            Grow();
        }

        _items![_count++] = item;
    }

    public readonly T[] ToArray()
    {
        if (_count is 0)
        {
            return [];
        }

        T[] result = GC.AllocateUninitializedArray<T>(_count);
        _items.AsSpan(0, _count).CopyTo(result);
        return result;
    }

    public void Dispose()
    {
        T[]? items = _items;
        this = default;
        if (items is not null)
        {
            ArrayPool<T>.Shared.Return(items, clearArray: true);
        }
    }

    private void Grow()
    {
        int capacity = _items is null ? 4 : _items.Length * 2;
        T[] replacement = ArrayPool<T>.Shared.Rent(capacity);
        _items.AsSpan(0, _count).CopyTo(replacement);
        T[]? previous = _items;
        _items = replacement;
        if (previous is not null)
        {
            ArrayPool<T>.Shared.Return(previous, clearArray: true);
        }
    }
}
