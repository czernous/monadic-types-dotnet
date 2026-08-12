using System.Runtime.CompilerServices;

namespace MonadicTypes.Tests;

public class OptionTests
{
    [Fact]
    public void Default_IsNone()
    {
        Option<int> option = default;

        Assert.True(option.IsNone);
        Assert.Throws<InvalidOperationException>(() => option.Value);
    }

    [Fact]
    public void Some_RejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => Option<string>.Some(null!));
    }

    [Fact]
    public void NullImplicitConversion_ProducesNone()
    {
        Option<string> option = (string)null!;

        Assert.True(option.IsNone);
    }

    [Fact]
    public void MapBindFilter_Compose()
    {
        Option<string> option = Option<int>.Some(20)
            .Map(static value => value * 2)
            .Filter(static value => value > 10)
            .Bind(static value => Option<string>.Some(
                value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        Assert.Equal("40", option.Value);
    }

    [Fact]
    public void ValueOrElse_IsLazyForSome()
    {
        bool invoked = false;
        int value = Option<int>.Some(5).ValueOrElse(() =>
        {
            invoked = true;
            return 10;
        });

        Assert.Equal(5, value);
        Assert.False(invoked);
    }

    [Fact]
    public void Switch_InvokesExactlyOneActiveBranch()
    {
        int someCalls = 0;
        int noneCalls = 0;

        Option<int>.Some(5).Switch(
            _ => someCalls++,
            () => noneCalls++);
        Option<int>.None.Switch(
            _ => someCalls++,
            () => noneCalls++);

        Assert.Equal(1, someCalls);
        Assert.Equal(1, noneCalls);
    }

    [Fact]
    public void StructCallables_MapAndBindWithoutDelegates()
    {
        Option<int> source = Option<int>.Some(5);

        Option<long> mapped = source.Map<long, Widen>(default(Widen));
        Option<long> bound = source.Bind<long, WidenOption>(default(WidenOption));

        Assert.Equal(6L, mapped.Value);
        Assert.Equal(6L, bound.Value);
    }

    [Fact]
    public void StructCallables_AreNotInvokedForNone()
    {
        Option<int> source = Option<int>.None;

        Option<int> mapped = source.Map<int, ThrowingMap>(default(ThrowingMap));

        Assert.True(mapped.IsNone);
    }

    [Fact]
    public void StateOverloads_PassStateWithoutCapturedClosures()
    {
        Option<int> source = Option<int>.Some(5);

        Option<long> mapped = source.Map(2L, static (value, state) => value + state);
        Option<long> bound = source.Bind(2L, static (value, state) => Option<long>.Some(value + state));
        Option<int> filtered = source.Filter(4, static (value, minimum) => value > minimum);

        Assert.Equal(7L, mapped.Value);
        Assert.Equal(7L, bound.Value);
        Assert.Equal(5, filtered.Value);
    }

    private readonly struct Widen : IValueFunction<int, long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Invoke(int value) => value + 1L;
    }

    private readonly struct WidenOption : IValueFunction<int, Option<long>>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<long> Invoke(int value) => Option<long>.Some(value + 1L);
    }

    private readonly struct ThrowingMap : IValueFunction<int, int>
    {
        public int Invoke(int value) => throw new InvalidOperationException();
    }
}
