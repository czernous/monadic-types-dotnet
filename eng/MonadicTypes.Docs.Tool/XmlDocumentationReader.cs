using System.Buffers;
using System.Xml;

namespace MonadicTypes.Tooling;

internal static class XmlDocumentationReader
{
    private static readonly SearchValues<char> Whitespace = SearchValues.Create(
        "\u0009\u000A\u000B\u000C\u000D\u0020\u0085\u00A0\u1680"
        + "\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A"
        + "\u2028\u2029\u202F\u205F\u3000");

    public static List<DocumentedMember> Read(string assemblyPath, string xmlPath)
    {
        Dictionary<string, PublicApiMember> publicMembers = PublicApiReader.Read(assemblyPath);
        return Read(publicMembers, xmlPath);
    }

    internal static List<DocumentedMember> Read(
        Dictionary<string, PublicApiMember> publicMembers,
        string xmlPath)
    {
        Dictionary<string, XmlMember> documentation = ReadDocumentation(xmlPath, publicMembers.Count);
        List<DocumentedMember> members = new(publicMembers.Count);
        foreach (KeyValuePair<string, PublicApiMember> item in publicMembers)
        {
            if (item.Key.Length < 3
                || item.Key[0] is not ('T' or 'M' or 'P' or 'F' or 'E'))
            {
                continue;
            }

            XmlMember member = ResolveMember(documentation, item.Key);
            members.Add(new DocumentedMember(
                item.Key,
                member.Summary,
                member.Example,
                DisplaySignature: item.Value.Signature,
                DisplayDeclaringType: item.Value.DeclaringType,
                Remarks: member.Remarks,
                Returns: member.Returns,
                Exceptions: member.Exceptions,
                Parameters: member.Parameters,
                TypeParameters: member.TypeParameters));
        }

        members.Sort(static (left, right) => CompareMembers(left, right));
        return members;
    }

    private static int CompareMembers(DocumentedMember left, DocumentedMember right)
    {
        int type = StringComparer.Ordinal.Compare(left.DeclaringType, right.DeclaringType);
        if (type is not 0)
        {
            return type;
        }

        if (left.IsType != right.IsType)
        {
            return left.IsType ? -1 : 1;
        }

        int label = StringComparer.Ordinal.Compare(
            left.Label,
            right.Label);
        return label is 0 ? StringComparer.Ordinal.Compare(left.Id, right.Id) : label;
    }

    private static Dictionary<string, XmlMember> ReadDocumentation(
        string xmlPath,
        int capacity)
    {
        Dictionary<string, XmlMember> documentation = new(capacity, StringComparer.Ordinal);
        using XmlReader reader = XmlReader.Create(
            xmlPath,
            new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true });
        while (reader.Read())
        {
            if (reader is not { NodeType: XmlNodeType.Element, Name: "member" }
                || reader.GetAttribute("name") is not { } id)
            {
                continue;
            }

            documentation[id] = ParseMember(reader);
        }

        return documentation;
    }

    private static XmlMember ParseMember(XmlReader reader)
    {
        XmlParseState state = default;
        int memberDepth = reader.Depth;
        if (reader.IsEmptyElement || !reader.Read())
        {
            return XmlMember.Empty;
        }

        try
        {
            while (reader.NodeType is not XmlNodeType.EndElement || reader.Depth != memberDepth)
            {
                if (reader is not { NodeType: XmlNodeType.Element })
                {
                    if (!reader.Read())
                    {
                        break;
                    }

                    continue;
                }

                state = reader.Name switch
                {
                    "summary" => state with { Summary = ReadFormattedElement(reader) },
                    "remarks" => state with { Remarks = ReadFormattedElement(reader) },
                    "returns" => state with { Returns = ReadFormattedElement(reader) },
                    "example" => state with { Example = ReadExample(reader) },
                    "exception" => ReadException(reader, state),
                    "param" => ReadParameter(reader, state),
                    "typeparam" => ReadTypeParameter(reader, state),
                    "inheritdoc" => ReadInheritdoc(reader, state),
                    _ => Advance(reader, state)
                };
            }

            return new XmlMember(
                state.Summary ?? string.Empty,
                state.Example ?? string.Empty,
                state.Inheritdoc,
                state.Remarks ?? string.Empty,
                state.Returns ?? string.Empty,
                state.Exceptions.ToArray(),
                state.Parameters.ToArray(),
                state.TypeParameters.ToArray());
        }
        finally
        {
            state.Dispose();
        }
    }

    private static XmlParseState ReadInheritdoc(XmlReader reader, XmlParseState state)
    {
        string? inheritdoc = reader.GetAttribute("cref");
        reader.Skip();
        return state with { Inheritdoc = inheritdoc };
    }

    private static XmlParseState ReadException(XmlReader reader, XmlParseState state)
    {
        string type = ShortName(reader.GetAttribute("cref") ?? string.Empty);
        string description = ReadFormattedElement(reader);
        state.Exceptions.Add(new ApiExceptionDocumentation(type, description));
        return state;
    }

    private static XmlParseState ReadParameter(XmlReader reader, XmlParseState state)
    {
        ApiNamedDocumentation parameter = ReadNamedDocumentation(reader);
        state.Parameters.Add(parameter);
        return state;
    }

    private static XmlParseState ReadTypeParameter(XmlReader reader, XmlParseState state)
    {
        ApiNamedDocumentation parameter = ReadNamedDocumentation(reader);
        state.TypeParameters.Add(parameter);
        return state;
    }

    private static ApiNamedDocumentation ReadNamedDocumentation(XmlReader reader)
    {
        string name = reader.GetAttribute("name") ?? string.Empty;
        return new ApiNamedDocumentation(name, ReadFormattedElement(reader));
    }

    private static XmlParseState Advance(XmlReader reader, XmlParseState state)
    {
        _ = reader.Read();
        return state;
    }

    private static string ReadExample(XmlReader reader)
    {
        int exampleDepth = reader.Depth;
        string? code = null;
        Span<char> initial = stackalloc char[128];
        PooledTextBuilder fallback = new(initial);
        try
        {
            if (reader.IsEmptyElement || !reader.Read())
            {
                return string.Empty;
            }

            while (reader.NodeType is not XmlNodeType.EndElement || reader.Depth != exampleDepth)
            {
                code = reader switch
                {
                    { NodeType: XmlNodeType.Element, Name: "code" } => ReadExampleCode(reader, code),
                    { NodeType: XmlNodeType.Text or XmlNodeType.CDATA } =>
                        ReadExampleText(reader, ref fallback, code),
                    _ => AdvanceExample(reader, code)
                };
            }

            _ = reader.Read();
            return code ?? fallback.ToString().Trim();
        }
        finally
        {
            fallback.Dispose();
        }
    }

    private static string? ReadExampleCode(XmlReader reader, string? current)
    {
        string code = Dedent(reader.ReadElementContentAsString());
        return current ?? code;
    }

    private static string? ReadExampleText(
        XmlReader reader,
        ref PooledTextBuilder builder,
        string? current)
    {
        AppendCollapsed(ref builder, reader.Value);
        _ = reader.Read();
        return current;
    }

    private static string? AdvanceExample(XmlReader reader, string? current)
    {
        _ = reader.Read();
        return current;
    }

    private static XmlMember ResolveMember(Dictionary<string, XmlMember> documentation, string id)
    {
        string current = id;
        for (int depth = 0; depth < 16 && documentation.TryGetValue(current, out XmlMember member); depth++)
        {
            if (member.Summary.Length is not 0
                || member.Example.Length is not 0
                || member.Remarks.Length is not 0
                || member.Returns.Length is not 0
                || member.Exceptions.Length is not 0
                || member.Parameters.Length is not 0
                || member.TypeParameters.Length is not 0)
            {
                return member;
            }

            if (member.Inheritdoc is null)
            {
                return XmlMember.Empty;
            }

            current = member.Inheritdoc;
        }

        return XmlMember.Empty;
    }

    private static string ReadFormattedElement(XmlReader reader)
    {
        int elementDepth = reader.Depth;
        Span<char> initial = stackalloc char[256];
        PooledTextBuilder builder = new(initial);
        try
        {
            if (reader.IsEmptyElement || !reader.Read())
            {
                return string.Empty;
            }

            while (reader.NodeType is not XmlNodeType.EndElement || reader.Depth != elementDepth)
            {
                _ = ProcessFormattedNode(reader, ref builder);
            }

            _ = reader.Read();
            return builder.ToString().Trim();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private static bool ProcessFormattedNode(
        XmlReader reader,
        ref PooledTextBuilder builder) => reader switch
        {
            { NodeType: XmlNodeType.Text or XmlNodeType.CDATA } => AppendText(reader, ref builder),
            { NodeType: XmlNodeType.Element, Name: "c" } => AppendInlineElement(reader, ref builder),
            { NodeType: XmlNodeType.Element, Name: "paramref" or "typeparamref" } =>
                AppendNamedReference(reader, ref builder),
            { NodeType: XmlNodeType.Element, Name: "see" } => AppendSeeReference(reader, ref builder),
            _ => Advance(reader)
        };

    private static bool AppendText(XmlReader reader, ref PooledTextBuilder builder)
    {
        AppendCollapsed(ref builder, reader.Value);
        _ = reader.Read();
        return true;
    }

    private static bool AppendInlineElement(XmlReader reader, ref PooledTextBuilder builder)
    {
        AppendInlineCode(ref builder, reader.ReadElementContentAsString());
        return true;
    }

    private static bool AppendNamedReference(XmlReader reader, ref PooledTextBuilder builder)
    {
        AppendInlineCode(ref builder, ReadAttributeAndSkip(reader, "name"));
        return true;
    }

    private static bool AppendSeeReference(XmlReader reader, ref PooledTextBuilder builder)
    {
        AppendInlineCode(ref builder, ReadSeeText(reader));
        return true;
    }

    private static string ReadSeeText(XmlReader reader) => reader.GetAttribute("cref") switch
    {
        { } cref => SkipAndReturn(reader, ShortName(cref)),
        _ => reader.GetAttribute("langword") switch
        {
            { } langword => SkipAndReturn(reader, langword),
            _ => reader.ReadElementContentAsString()
        }
    };

    private static string ReadAttributeAndSkip(XmlReader reader, string name) =>
        SkipAndReturn(reader, reader.GetAttribute(name) ?? string.Empty);

    private static string SkipAndReturn(XmlReader reader, string value)
    {
        reader.Skip();
        return value;
    }

    private static bool Advance(XmlReader reader)
    {
        _ = reader.Read();
        return true;
    }

    private static void AppendInlineCode(ref PooledTextBuilder builder, string text)
    {
        if (text.Length is 0)
        {
            return;
        }

        if (builder.Length is not 0 && !char.IsWhiteSpace(builder.Last))
        {
            builder.Append(' ');
        }

        builder.Append('`');
        builder.Append(text);
        builder.Append('`');
    }

    private static void AppendCollapsed(ref PooledTextBuilder builder, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        ReadOnlySpan<char> text = value.AsSpan().Trim();
        if (text.IsEmpty)
        {
            return;
        }

        if (builder.Length is not 0
            && !char.IsWhiteSpace(builder.Last)
            && text[0] is not ('.' or ',' or ';' or ':' or '!' or '?' or ')' or ']'))
        {
            builder.Append(' ');
        }

        while (!text.IsEmpty)
        {
            int whitespace = text.IndexOfAny(Whitespace);
            if (whitespace < 0)
            {
                builder.Append(text);
                return;
            }

            builder.Append(text[..whitespace]);
            int next = whitespace + 1;
            while (next < text.Length && Whitespace.Contains(text[next]))
            {
                next++;
            }

            if (next < text.Length && builder.Length is not 0)
            {
                builder.Append(' ');
            }

            text = text[next..];
        }
    }

    private static string Dedent(string value)
    {
        ReadOnlySpan<char> content = value.AsSpan().Trim();
        int indentation = int.MaxValue;
        bool firstLine = true;
        foreach (Range range in content.Split('\n'))
        {
            ReadOnlySpan<char> line = content[range].TrimEnd('\r');
            int offset = 0;
            while (offset < line.Length && line[offset] is ' ' or '\t')
            {
                offset++;
            }

            if (!firstLine && offset < line.Length && offset < indentation)
            {
                indentation = offset;
            }

            firstLine = false;
        }

        if (indentation is 0 or int.MaxValue)
        {
            return content.ToString();
        }

        Span<char> initial = stackalloc char[Math.Min(content.Length, 256)];
        PooledTextBuilder builder = new(initial);
        try
        {
            bool first = true;
            foreach (Range range in content.Split('\n'))
            {
                if (!first)
                {
                    builder.Append('\n');
                }

                ReadOnlySpan<char> line = content[range].TrimEnd('\r');
                int remove = first ? 0 : Math.Min(indentation, line.Length);
                builder.Append(line[remove..]);
                first = false;
            }

            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private static string ShortName(string cref)
    {
        ReadOnlySpan<char> name = cref;
        if (name is [_, ':', .. var identity])
        {
            name = identity;
        }

        int parameters = name.IndexOf('(');
        name = parameters < 0 ? name : name[..parameters];

        int methodArity = name.IndexOf("``", StringComparison.Ordinal);
        name = methodArity < 0 ? name : name[..methodArity];

        int separator = name.LastIndexOf('.');
        name = separator < 0 ? name : name[(separator + 1)..];

        int typeArity = name.IndexOf('`');
        name = typeArity < 0 ? name : name[..typeArity];
        return name.ToString();
    }

    private struct XmlParseState
    {
        public string? Summary;
        public string? Example;
        public string? Inheritdoc;
        public string? Remarks;
        public string? Returns;
        public PooledArrayBuilder<ApiExceptionDocumentation> Exceptions;
        public PooledArrayBuilder<ApiNamedDocumentation> Parameters;
        public PooledArrayBuilder<ApiNamedDocumentation> TypeParameters;

        public void Dispose()
        {
            Exceptions.Dispose();
            Parameters.Dispose();
            TypeParameters.Dispose();
        }
    }

    private readonly record struct XmlMember(
        string Summary,
        string Example,
        string? Inheritdoc,
        string Remarks,
        string Returns,
        ApiExceptionDocumentation[] Exceptions,
        ApiNamedDocumentation[] Parameters,
        ApiNamedDocumentation[] TypeParameters)
    {
        public static XmlMember Empty { get; } = new(
            string.Empty,
            string.Empty,
            null,
            string.Empty,
            string.Empty,
            [],
            [],
            []);
    }
}
