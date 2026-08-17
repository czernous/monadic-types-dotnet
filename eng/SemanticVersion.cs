using System.Buffers;

namespace MonadicTypes.Tooling;

internal static class SemanticVersion
{
    private static readonly SearchValues<char> DecimalDigits = SearchValues.Create("0123456789");
    private static readonly SearchValues<char> IdentifierCharacters = SearchValues.Create(
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-.");

    public static bool IsValid(ReadOnlySpan<char> version)
    {
        if (version.Contains('+'))
        {
            return false;
        }

        int separator = version.IndexOf('-');
        ReadOnlySpan<char> core = separator < 0 ? version : version[..separator];
        ReadOnlySpan<char> suffix = separator < 0 ? [] : version[(separator + 1)..];
        int firstDot = core.IndexOf('.');
        int lastDot = core.LastIndexOf('.');
        return firstDot > 0
            && lastDot > firstDot + 1
            && lastDot < core.Length - 1
            && firstDot != lastDot
            && IsCoreNumber(core[..firstDot])
            && IsCoreNumber(core[(firstDot + 1)..lastDot])
            && IsCoreNumber(core[(lastDot + 1)..])
            && (separator < 0 || IsIdentifierList(suffix));
    }

    private static bool IsCoreNumber(ReadOnlySpan<char> value) =>
        !value.IsEmpty
        && (value.Length is 1 || value[0] is not '0')
        && value.IndexOfAnyExcept(DecimalDigits) < 0;

    private static bool IsIdentifierList(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty || value.IndexOfAnyExcept(IdentifierCharacters) >= 0)
        {
            return false;
        }

        while (true)
        {
            int separator = value.IndexOf('.');
            ReadOnlySpan<char> identifier = separator < 0 ? value : value[..separator];
            if (identifier.IsEmpty
                || (identifier.Length > 1
                    && identifier[0] is '0'
                    && identifier.IndexOfAnyExcept(DecimalDigits) < 0))
            {
                return false;
            }

            if (separator < 0)
            {
                return true;
            }

            value = value[(separator + 1)..];
        }
    }
}
