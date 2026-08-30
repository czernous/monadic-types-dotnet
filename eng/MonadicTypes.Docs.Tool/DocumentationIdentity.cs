namespace MonadicTypes.Tooling;

internal static class DocumentationIdentity
{
    public static string DeclaringType(string id)
    {
        ReadOnlySpan<char> member = id.Length > 2 ? id.AsSpan(2) : id;
        int signature = member.IndexOf('(');
        ReadOnlySpan<char> declaration = signature < 0 ? member : member[..signature];
        if (id is ['T', ':', ..])
        {
            return TypeDisplayName(declaration);
        }

        int separator = declaration.LastIndexOf('.');
        return TypeDisplayName(separator < 0 ? declaration : declaration[..separator]);
    }

    public static string MemberName(string id)
    {
        if (id is ['T', ':', ..])
        {
            return DeclaringType(id);
        }

        ReadOnlySpan<char> member = id.Length > 2 ? id.AsSpan(2) : id;
        int signature = member.IndexOf('(');
        ReadOnlySpan<char> declaration = signature < 0 ? member : member[..signature];
        int separator = declaration.LastIndexOf('.');
        ReadOnlySpan<char> name = separator < 0 ? declaration : declaration[(separator + 1)..];
        int genericArity = name.IndexOf("``", StringComparison.Ordinal);
        name = genericArity < 0 ? name : name[..genericArity];
        return DisplayMemberName(name, DeclaringType(id));
    }

    public static string Signature(string id)
    {
        string name = MemberName(id);
        if (id is ['T' or 'P' or 'F' or 'E', ':', ..])
        {
            return name;
        }

        ReadOnlySpan<char> member = id.AsSpan(2);
        int parameters = member.IndexOf('(');
        int declarationEnd = parameters < 0 ? member.Length : parameters;
        ReadOnlySpan<char> declaration = member[..declarationEnd];
        int separator = declaration.LastIndexOf('.');
        ReadOnlySpan<char> encodedName = separator < 0 ? declaration : declaration[(separator + 1)..];
        name = DisplayMemberName(encodedName, DeclaringType(id));
        return parameters < 0
            ? name + "()"
            : string.Concat(name, FormatParameters(member[parameters..]));
    }

    public static string MemberLabel(string id)
    {
        ReadOnlySpan<char> member = id.Length > 2 ? id.AsSpan(2) : id;
        int signature = member.IndexOf('(');
        ReadOnlySpan<char> declaration = signature < 0 ? member : member[..signature];
        int separator = declaration.LastIndexOf('.');
        ReadOnlySpan<char> type = separator < 0 ? declaration : declaration[..separator];
        ReadOnlySpan<char> name = separator < 0 ? [] : declaration[(separator + 1)..];
        int namespaceSeparator = type.LastIndexOf('.');
        type = namespaceSeparator < 0 ? type : type[(namespaceSeparator + 1)..];
        type = RemoveGenericArity(type);
        name = RemoveGenericArity(name);
        return name.IsEmpty ? type.ToString() : string.Concat(type, ".", name);
    }

    private static string DisplayMemberName(ReadOnlySpan<char> value, string declaringType)
    {
        int genericArity = value.IndexOf("``", StringComparison.Ordinal);
        ReadOnlySpan<char> name = genericArity < 0 ? value : value[..genericArity];
        string display = name switch
        {
            "#ctor" => declaringType.AsSpan(0, declaringType.IndexOf('<') switch
            {
                < 0 => declaringType.Length,
                var index => index
            }).ToString(),
            "op_Implicit" => "implicit operator",
            "op_Explicit" => "explicit operator",
            "Item" => "this[]",
            _ => name.ToString()
        };
        if (genericArity < 0)
        {
            return display;
        }

        int count = 0;
        foreach (char character in value[(genericArity + 2)..])
        {
            count = character is >= '0' and <= '9' ? (count * 10) + character - '0' : count;
        }

        return string.Concat(display, GenericArguments(count));
    }

    private static string TypeDisplayName(ReadOnlySpan<char> value)
    {
        int namespaceSeparator = value.LastIndexOf('.');
        ReadOnlySpan<char> name = namespaceSeparator < 0 ? value : value[(namespaceSeparator + 1)..];
        int arity = name.IndexOf('`');
        if (arity < 0)
        {
            return name.ToString();
        }

        int count = 0;
        foreach (char character in name[(arity + 1)..])
        {
            count = character is >= '0' and <= '9' ? (count * 10) + character - '0' : count;
        }

        return string.Concat(name[..arity], GenericArguments(count));
    }

    private static string GenericArguments(int count)
    {
        if (count <= 0)
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new("<T1");
        for (int index = 2; index <= count; index++)
        {
            builder.Append(", T").Append(index);
        }

        return builder.Append('>').ToString();
    }

    private static string FormatParameters(ReadOnlySpan<char> value)
    {
        System.Text.StringBuilder builder = new(value.Length);
        int identifierStart = -1;
        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            if (character is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '.' or '+' or '`')
            {
                identifierStart = identifierStart < 0 ? index : identifierStart;
                continue;
            }

            AppendIdentifier(builder, value, identifierStart, index);
            identifierStart = -1;
            builder.Append(character switch
            {
                '{' => '<',
                '}' => '>',
                '@' => '&',
                _ => character
            });
        }

        AppendIdentifier(builder, value, identifierStart, value.Length);
        return builder.ToString();
    }

    private static void AppendIdentifier(
        System.Text.StringBuilder builder,
        ReadOnlySpan<char> source,
        int start,
        int end)
    {
        if (start < 0)
        {
            return;
        }

        ReadOnlySpan<char> identifier = source[start..end];
        if (identifier[0] is '`')
        {
            int offset = identifier.Length > 1 && identifier[1] is '`' ? 2 : 1;
            int ordinal = 0;
            foreach (char character in identifier[offset..])
            {
                ordinal = character is >= '0' and <= '9' ? (ordinal * 10) + character - '0' : ordinal;
            }

            builder.Append(offset is 1 ? 'T' : "TMethod").Append(ordinal + 1);
            return;
        }

        int namespaceSeparator = identifier.LastIndexOf('.');
        ReadOnlySpan<char> shortName = namespaceSeparator < 0 ? identifier : identifier[(namespaceSeparator + 1)..];
        int arity = shortName.IndexOf('`');
        builder.Append(arity < 0 ? shortName : shortName[..arity]);
    }

    private static ReadOnlySpan<char> RemoveGenericArity(ReadOnlySpan<char> value)
    {
        int arity = value.IndexOf('`');
        return arity < 0 ? value : value[..arity];
    }
}
