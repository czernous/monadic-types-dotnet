using System.Buffers;

namespace MonadicTypes.Tooling;

internal ref struct PooledTextBuilder(Span<char> initialBuffer)
{
    private Span<char> _buffer = initialBuffer;
    private char[]? _rented;
    private int _length;

    public readonly int Length => _length;

    public readonly char Last => _buffer[_length - 1];

    public readonly ReadOnlySpan<char> WrittenSpan => _buffer[.._length];

    public void Append(char value)
    {
        EnsureCapacity(1);
        _buffer[_length++] = value;
    }

    public void Append(string? value)
    {
        if (value is not null)
        {
            Append(value.AsSpan());
        }
    }

    public void Append(ReadOnlySpan<char> value)
    {
        EnsureCapacity(value.Length);
        value.CopyTo(_buffer[_length..]);
        _length += value.Length;
    }

    public void Append(int value)
    {
        EnsureCapacity(11);
        _ = value.TryFormat(_buffer[_length..], out int written);
        _length += written;
    }

    public override readonly string ToString() => new(_buffer[.._length]);

    public void Dispose()
    {
        char[]? rented = _rented;
        this = default;
        if (rented is not null)
        {
            ArrayPool<char>.Shared.Return(rented);
        }
    }

    private void EnsureCapacity(int additional)
    {
        int required = _length + additional;
        if (required <= _buffer.Length)
        {
            return;
        }

        int capacity = Math.Max(required, Math.Max(256, _buffer.Length * 2));
        char[] replacement = ArrayPool<char>.Shared.Rent(capacity);
        _buffer[.._length].CopyTo(replacement);
        char[]? previous = _rented;
        _buffer = replacement;
        _rented = replacement;
        if (previous is not null)
        {
            ArrayPool<char>.Shared.Return(previous);
        }
    }
}
