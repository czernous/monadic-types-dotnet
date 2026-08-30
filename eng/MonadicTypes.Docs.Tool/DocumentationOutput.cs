using System.Buffers;
using System.Globalization;
using System.Text;

namespace MonadicTypes.Tooling;

internal static class DocumentationOutput
{
    private const string GeneratedStart = "<!-- BEGIN GENERATED API INDEX -->";
    private const string GeneratedEnd = "<!-- END GENERATED API INDEX -->";
    private static readonly SearchValues<char> JsonEscapes = SearchValues.Create(
        "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007"
        + "\u0008\u0009\u000A\u000B\u000C\u000D\u000E\u000F"
        + "\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017"
        + "\u0018\u0019\u001A\u001B\u001C\u001D\u001E\u001F\"\\");

    public static string RenderPackage(PackageDocumentation package)
    {
        StringBuilder builder = new();
        AppendPackage(builder, package, package.Navigation);
        return builder.ToString().TrimEnd() + "\n";
    }

    private static void AppendPackage(
        StringBuilder builder,
        PackageDocumentation package,
        PackageNavigation navigation)
    {
        builder.Append("## ");
        builder.Append(PackageHeading(package.PackageId)).Append("\n\n");

        builder.Append("**Types:** ");
        string? previousType = null;
        for (int index = 0; index < package.Members.Count; index++)
        {
            string declaringType = package.Members[index].DeclaringType;
            if (string.Equals(previousType, declaringType, StringComparison.Ordinal))
            {
                continue;
            }

            if (previousType is not null)
            {
                builder.Append(" · ");
            }

            builder.Append("[`").Append(declaringType).Append("`](#")
                .Append(navigation.TypeAnchors[index]).Append(')');
            previousType = declaringType;
        }

        builder.Append("\n\n");

        int typeStart = 0;
        while (typeStart < package.Members.Count)
        {
            string declaringType = package.Members[typeStart].DeclaringType;
            int typeEnd = typeStart + 1;
            while (typeEnd < package.Members.Count
                   && string.Equals(package.Members[typeEnd].DeclaringType, declaringType, StringComparison.Ordinal))
            {
                typeEnd++;
            }

            RenderType(builder, package.Members, navigation, typeStart, typeEnd, declaringType);
            typeStart = typeEnd;
        }

    }

    private static void RenderType(
        StringBuilder builder,
        List<DocumentedMember> members,
        PackageNavigation navigation,
        int start,
        int end,
        string declaringType)
    {
        builder.Append("### Type: `").Append(declaringType).Append("`\n\n");

        int memberStart = start;
        if (members[start].IsType)
        {
            builder.Append(members[start].Summary).Append("\n\n");
            if (members[start].XmlExample.Length is not 0)
            {
                builder.Append("**Example**\n\n```csharp\n")
                    .Append(members[start].XmlExample)
                    .Append("\n```\n\n");
            }

            memberStart++;
        }

        if (memberStart >= end)
        {
            return;
        }

        builder.Append("| Member | Description |\n| --- | --- |\n");
        string? previousName = null;
        for (int index = memberStart; index < end; index++)
        {
            DocumentedMember member = members[index];
            if (string.Equals(previousName, member.Name, StringComparison.Ordinal))
            {
                continue;
            }

            builder.Append("| [`").Append(EscapeTable(member.Name)).Append("`](#")
                .Append(navigation.FamilyAnchors[index])
                .Append(") | ").Append(EscapeTable(member.Summary)).Append(" |\n");
            previousName = member.Name;
        }

        builder.Append('\n');
        int familyStart = memberStart;
        while (familyStart < end)
        {
            string name = members[familyStart].Name;
            int familyEnd = familyStart + 1;
            while (familyEnd < end
                   && string.Equals(members[familyEnd].Name, name, StringComparison.Ordinal))
            {
                familyEnd++;
            }

            RenderFamily(builder, members, navigation, familyStart, familyEnd);
            familyStart = familyEnd;
        }
    }

    private static void RenderFamily(
        StringBuilder builder,
        List<DocumentedMember> members,
        PackageNavigation navigation,
        int start,
        int end)
    {
        DocumentedMember first = members[start];
        builder.Append("#### Member: `").Append(first.DeclaringType).Append('.')
            .Append(first.Name).Append("`\n\n");

        string? example = null;
        for (int index = start; index < end && example is null; index++)
        {
            example = members[index].XmlExample.Length is 0 ? null : members[index].XmlExample;
        }

        if (example is not null)
        {
            builder.Append("**Example**\n\n```csharp\n")
                .Append(example)
                .Append("\n```\n\n");
        }

        builder.Append("| Overload | Description |\n| --- | --- |\n");
        for (int index = start; index < end; index++)
        {
            DocumentedMember member = members[index];
            builder.Append("| [`").Append(EscapeTable(member.Signature)).Append("`](#")
                .Append(navigation.OverloadAnchors[index])
                .Append(") | ").Append(EscapeTable(member.Summary)).Append(" |\n");
        }

        builder.Append('\n');
        for (int index = start; index < end; index++)
        {
            DocumentedMember member = members[index];
            builder.Append("##### Overload: `").Append(member.Signature).Append("` on `")
                .Append(member.DeclaringType).Append("`\n\n")
                .Append(member.Summary).Append("\n\n");
            if (member.Remarks.Length is not 0)
            {
                builder.Append(member.Remarks).Append("\n\n");
            }

            AppendNamedDocumentation(builder, "Type parameters", member.TypeParameters);
            AppendNamedDocumentation(builder, "Parameters", member.Parameters);

            if (member.Returns.Length is not 0)
            {
                builder.Append("**Returns:** ").Append(member.Returns).Append("\n\n");
            }

            if (member.Exceptions is { Length: > 0 } exceptions)
            {
                builder.Append("**Throws**\n\n");
                for (int exceptionIndex = 0; exceptionIndex < exceptions.Length; exceptionIndex++)
                {
                    ApiExceptionDocumentation exception = exceptions[exceptionIndex];
                    builder.Append("- `").Append(exception.Type).Append("`: ")
                        .Append(exception.Description).Append('\n');
                }

                builder.Append('\n');
            }
        }
    }

    private static void AppendNamedDocumentation(
        StringBuilder builder,
        string heading,
        ApiNamedDocumentation[]? items)
    {
        if (items is not { Length: > 0 })
        {
            return;
        }

        builder.Append("**").Append(heading).Append("**\n\n");
        foreach (ApiNamedDocumentation item in items)
        {
            builder.Append("- `").Append(item.Name).Append("`: ")
                .Append(item.Description).Append('\n');
        }

        builder.Append('\n');
    }

    public static string RenderReference(DocumentationModel model)
    {
        ValidateNavigation(model);
        string reference = RenderReferenceUnchecked(model);
        return reference;
    }

    internal static string RenderReferenceUnchecked(DocumentationModel model)
    {
        StringBuilder builder = new("# API Reference\n\nGenerated from public PE metadata and compiler XML documentation.\n\n## Packages\n\n");
        foreach (PackageDocumentation package in model.Packages)
        {
            builder.Append("- [").Append(package.PackageId).Append("](#")
                .Append(package.Navigation.PackageAnchor).Append(")\n");
        }

        builder.Append('\n');
        foreach (PackageDocumentation package in model.Packages)
        {
            AppendPackage(builder, package, package.Navigation);
            builder.Append('\n');
        }

        return builder.ToString().TrimEnd() + "\n";
    }

    internal static void ValidateReference(string reference)
    {
        HashSet<string> fragments = new(StringComparer.Ordinal);
        ReadOnlySpan<char> content = reference;
        foreach (Range range in content.Split('\n'))
        {
            ReadOnlySpan<char> line = content[range].TrimEnd('\r');
            ReadOnlySpan<char> heading = MarkdownHeading(line);
            ReadOnlySpan<char> fragment = heading.IsEmpty ? ExplicitFragment(line) : heading;
            if (fragment.IsEmpty)
            {
                continue;
            }

            string value = heading.IsEmpty ? fragment.ToString() : CreateMarkdownAnchor(fragment);
            if (!fragments.Add(value))
            {
                throw new InvalidDataException($"Duplicate generated documentation fragment: {value}.");
            }
        }

        HashSet<string>.AlternateLookup<ReadOnlySpan<char>> lookup =
            fragments.GetAlternateLookup<ReadOnlySpan<char>>();
        int offset = 0;
        while ((offset = reference.IndexOf("](#", offset, StringComparison.Ordinal)) >= 0)
        {
            int start = offset + 3;
            int end = reference.IndexOf(')', start);
            if (end < 0)
            {
                throw new InvalidDataException("Generated documentation contains an incomplete fragment link.");
            }

            ReadOnlySpan<char> target = reference.AsSpan(start, end - start);
            if (!lookup.Contains(target))
            {
                throw new InvalidDataException($"Generated documentation fragment has no target: {target.ToString()}.");
            }

            offset = end + 1;
        }
    }

    private static void ValidateNavigation(DocumentationModel model)
    {
        int capacity = 2 + model.Packages.Count;
        for (int index = 0; index < model.Packages.Count; index++)
        {
            capacity += model.Packages[index].Members.Count * 2;
        }

        string[] anchors = ArrayPool<string>.Shared.Rent(capacity);
        int written = 0;
        try
        {
            anchors[written++] = "api-reference";
            anchors[written++] = "packages";
            for (int packageIndex = 0; packageIndex < model.Packages.Count; packageIndex++)
            {
                PackageDocumentation package = model.Packages[packageIndex];
                PackageNavigation navigation = package.Navigation;
                anchors[written++] = navigation.PackageAnchor;
                string? previousType = null;
                string? previousName = null;
                for (int memberIndex = 0; memberIndex < package.Members.Count; memberIndex++)
                {
                    DocumentedMember member = package.Members[memberIndex];
                    if (member.IsType)
                    {
                        anchors[written++] = navigation.TypeAnchors[memberIndex];
                        previousType = member.DeclaringType;
                        previousName = null;
                        continue;
                    }

                    if (!string.Equals(previousType, member.DeclaringType, StringComparison.Ordinal)
                        || !string.Equals(previousName, member.Name, StringComparison.Ordinal))
                    {
                        anchors[written++] = navigation.FamilyAnchors[memberIndex];
                        previousType = member.DeclaringType;
                        previousName = member.Name;
                    }

                    anchors[written++] = navigation.OverloadAnchors[memberIndex];
                }
            }

            Array.Sort(anchors, 0, written, StringComparer.Ordinal);
            for (int index = 1; index < written; index++)
            {
                if (string.Equals(anchors[index - 1], anchors[index], StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Duplicate generated documentation fragment: {anchors[index]}.");
                }
            }
        }
        finally
        {
            ArrayPool<string>.Shared.Return(anchors, clearArray: true);
        }
    }

    private static ReadOnlySpan<char> ExplicitFragment(ReadOnlySpan<char> line)
    {
        if (!line.StartsWith("<h", StringComparison.Ordinal))
        {
            return [];
        }

        int marker = line.IndexOf(" id=\"", StringComparison.Ordinal);
        if (marker < 0)
        {
            return [];
        }

        int start = marker + 5;
        int length = line[start..].IndexOf('"');
        return length < 0 ? [] : line.Slice(start, length);
    }

    private static ReadOnlySpan<char> MarkdownHeading(ReadOnlySpan<char> line)
    {
        int level = 0;
        while (level < line.Length && level < 6 && line[level] is '#')
        {
            level++;
        }

        if (level is 0 || level >= line.Length || line[level] is not ' ')
        {
            return [];
        }

        ReadOnlySpan<char> heading = line[(level + 1)..].Trim();
        while (!heading.IsEmpty && heading[^1] is '#')
        {
            heading = heading[..^1].TrimEnd();
        }

        return heading;
    }

    internal static string CreateMarkdownAnchor(ReadOnlySpan<char> value) =>
        CreateMarkdownAnchor(value, [], [], []);

    internal static string CreateMarkdownAnchor(
        ReadOnlySpan<char> first,
        ReadOnlySpan<char> second) =>
        CreateMarkdownAnchor(first, second, [], []);

    internal static string CreateMarkdownAnchor(
        ReadOnlySpan<char> first,
        ReadOnlySpan<char> second,
        ReadOnlySpan<char> third,
        ReadOnlySpan<char> fourth)
    {
        int capacity = first.Length + second.Length + third.Length + fourth.Length;
        char[]? rented = null;
        Span<char> buffer = capacity <= 512
            ? stackalloc char[capacity]
            : (rented = ArrayPool<char>.Shared.Rent(capacity));
        try
        {
            int written = 0;
            bool separator = false;
            NormalizeAnchorPart(first, buffer, ref written, ref separator);
            NormalizeAnchorPart(second, buffer, ref written, ref separator);
            NormalizeAnchorPart(third, buffer, ref written, ref separator);
            NormalizeAnchorPart(fourth, buffer, ref written, ref separator);

            return new string(buffer[..written]);
        }
        finally
        {
            if (rented is not null)
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }
    }

    public static string RenderManifest(DocumentationModel model)
    {
        StringBuilder builder = new("{\n  \"packages\": [\n");
        for (int index = 0; index < model.Packages.Count; index++)
        {
            PackageDocumentation package = model.Packages[index];
            PackageNavigation navigation = package.Navigation;
            if (index is not 0)
            {
                builder.Append(",\n");
            }

            builder.Append("    {\n      \"id\": ");
            AppendJsonString(builder, package.PackageId);
            builder.Append(",\n      \"project\": ");
            AppendJsonString(builder, package.ProjectName);
            builder.Append(",\n      \"reference\": \"api-reference.md#")
                .Append(navigation.PackageAnchor)
                .Append("\",\n      \"memberCount\": ")
                .Append(package.Members.Count)
                .Append(",\n      \"members\": [");
            for (int memberIndex = 0; memberIndex < package.Members.Count; memberIndex++)
            {
                DocumentedMember member = package.Members[memberIndex];
                builder.Append(memberIndex is 0 ? "\n" : ",\n")
                    .Append("        {\n          \"kind\": ");
                AppendJsonString(builder, member.IsType ? "type" : "member");
                builder.Append(",\n          \"id\": ");
                AppendJsonString(builder, member.Id);
                builder.Append(",\n          \"declaringType\": ");
                AppendJsonString(builder, member.DeclaringType);
                builder.Append(",\n          \"name\": ");
                AppendJsonString(builder, member.Name);
                builder.Append(",\n          \"signature\": ");
                AppendJsonString(builder, member.Signature);
                builder.Append(",\n          \"summary\": ");
                AppendJsonString(builder, member.Summary);
                builder.Append(",\n          \"familyReference\": \"api-reference.md#");
                if (member.IsType)
                {
                    builder.Append(navigation.TypeAnchors[memberIndex]);
                }
                else
                {
                    builder.Append(navigation.FamilyAnchors[memberIndex]);
                }

                builder.Append("\",\n          \"reference\": \"api-reference.md#");
                if (member.IsType)
                {
                    builder.Append(navigation.TypeAnchors[memberIndex]);
                }
                else
                {
                    builder.Append(navigation.OverloadAnchors[memberIndex]);
                }

                builder.Append("\"\n        }");
            }

            builder.Append(package.Members.Count is 0 ? "]\n    }" : "\n      ]\n    }");
        }

        return builder.Append("\n  ]\n}\n").ToString();
    }

    public static string RenderRootIndex(DocumentationModel model)
    {
        return GeneratedStart + "\n\n[Complete API reference](docs/api-reference.md)\n\n"
            + model.Packages.Sum(static package => package.Members.Count).ToString(CultureInfo.InvariantCulture)
            + " documented public members\n\n" + GeneratedEnd;
    }

    internal static string RewriteApiReferenceLinks(string markdown, DocumentationModel model)
    {
        const string targetPrefix = "docs/api-reference.md#";
        StringBuilder builder = new(markdown.Length);
        int copied = 0;
        int offset = 0;
        while (offset < markdown.Length)
        {
            int relative = markdown.AsSpan(offset).IndexOf("](" + targetPrefix, StringComparison.Ordinal);
            if (relative < 0)
            {
                break;
            }

            int targetStart = offset + relative + 2;
            int targetEnd = markdown.IndexOf(')', targetStart);
            if (targetEnd < 0)
            {
                break;
            }

            int labelEnd = targetStart - 2;
            int closingCode = markdown.LastIndexOf('`', labelEnd);
            int openingCode = closingCode < 0 ? -1 : markdown.LastIndexOf('`', closingCode - 1);
            int labelSearchEnd = openingCode < 0 ? labelEnd : openingCode;
            int labelStart = markdown.LastIndexOf('[', labelSearchEnd);
            string? fragment = labelStart < 0
                ? null
                : ResolveApiFragment(markdown.AsSpan(labelStart + 1, labelEnd - labelStart - 1), model);
            if (fragment is not null)
            {
                builder.Append(markdown.AsSpan(copied, targetStart - copied))
                    .Append(targetPrefix)
                    .Append(fragment);
                copied = targetEnd;
            }

            offset = targetEnd + 1;
        }

        return copied is 0
            ? markdown
            : builder.Append(markdown.AsSpan(copied)).ToString();
    }

    private static string? ResolveApiFragment(ReadOnlySpan<char> markdownLabel, DocumentationModel model)
    {
        ReadOnlySpan<char> label = markdownLabel.Trim().Trim('`');
        if (label.IsEmpty)
        {
            return null;
        }

        bool implicitConversions = label.EndsWith(" implicit conversion", StringComparison.Ordinal)
            || label.EndsWith(" implicit conversions", StringComparison.Ordinal);
        int memberSeparator = label.LastIndexOf('.');
        ReadOnlySpan<char> visibleType;
        ReadOnlySpan<char> memberName;
        if (implicitConversions)
        {
            visibleType = label[..label.IndexOf(' ')];
            memberName = "implicit operator";
        }
        else if (memberSeparator >= 0)
        {
            visibleType = label[..memberSeparator];
            memberName = label[(memberSeparator + 1)..];
            memberName = memberName.StartsWith("this[", StringComparison.Ordinal) ? "this[]" : memberName;
        }
        else
        {
            string? typeFragment = ResolveTypeFragment(label, model);
            if (typeFragment is not null)
            {
                return typeFragment;
            }

            visibleType = [];
            memberName = label;
        }

        int generic = visibleType.IndexOf('<');
        ReadOnlySpan<char> visibleBaseType = generic < 0 ? visibleType : visibleType[..generic];
        string? sole = null;
        string? typeMatch = null;
        string? extensionMatch = null;
        int familyCount = 0;
        string? previousDeclaringType = null;
        string? previousName = null;
        foreach (PackageDocumentation package in model.Packages)
        {
            PackageNavigation navigation = package.Navigation;
            for (int index = 0; index < package.Members.Count; index++)
            {
                DocumentedMember member = package.Members[index];
                if (member.IsType || !member.Name.AsSpan().SequenceEqual(memberName))
                {
                    continue;
                }

                string fragment = navigation.FamilyAnchors[index];
                if (!string.Equals(previousDeclaringType, member.DeclaringType, StringComparison.Ordinal)
                    || !string.Equals(previousName, member.Name, StringComparison.Ordinal))
                {
                    familyCount++;
                    sole = fragment;
                    previousDeclaringType = member.DeclaringType;
                    previousName = member.Name;
                }

                ReadOnlySpan<char> declaringType = member.DeclaringType;
                int declaringGeneric = declaringType.IndexOf('<');
                ReadOnlySpan<char> declaringBase = declaringGeneric < 0
                    ? declaringType
                    : declaringType[..declaringGeneric];
                if (!visibleBaseType.IsEmpty
                    && declaringBase.SequenceEqual(visibleBaseType)
                    && (generic >= 0) == (declaringGeneric >= 0))
                {
                    typeMatch = fragment;
                }
                else if (!visibleBaseType.IsEmpty
                         && declaringBase.StartsWith(visibleBaseType, StringComparison.Ordinal)
                         && declaringBase.EndsWith("Extensions", StringComparison.Ordinal))
                {
                    extensionMatch = fragment;
                }
            }
        }

        return typeMatch ?? extensionMatch ?? (familyCount is 1 ? sole : null);
    }

    private static string? ResolveTypeFragment(ReadOnlySpan<char> label, DocumentationModel model)
    {
        foreach (PackageDocumentation package in model.Packages)
        {
            for (int index = 0; index < package.Members.Count; index++)
            {
                DocumentedMember member = package.Members[index];
                if (member.IsType && member.DeclaringType.AsSpan().SequenceEqual(label))
                {
                    return package.Navigation.TypeAnchors[index];
                }
            }
        }

        return null;
    }

    public static string RenderPackageIndex(PackageDocumentation package) =>
        GeneratedStart + "\n\n[Complete API reference](" + package.RepositoryUrl + "/blob/HEAD/docs/api-reference.md#"
        + package.Navigation.PackageAnchor + ")\n\n"
        + "Documented public members: " + package.Members.Count.ToString(CultureInfo.InvariantCulture) + "\n\n"
        + GeneratedEnd;

    private static string EscapeTable(string value) => value.ReplaceLineEndings(" ")
        .Replace("|", "\\|", StringComparison.Ordinal);

    private static string PackageHeading(string packageId) => "Package " + packageId;

    private static void NormalizeAnchorPart(
        ReadOnlySpan<char> value,
        Span<char> destination,
        ref int written,
        ref bool separator)
    {
        foreach (char character in value)
        {
            if (character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9')
            {
                if (separator && written is not 0)
                {
                    destination[written++] = '-';
                }

                destination[written++] = character is >= 'A' and <= 'Z'
                    ? (char)(character | 0x20)
                    : character;
                separator = false;
            }
            else if (char.IsWhiteSpace(character))
            {
                separator = true;
            }
        }
    }

    private static void AppendJsonString(StringBuilder builder, string value)
    {
        builder.Append('"');
        ReadOnlySpan<char> remaining = value;
        while (!remaining.IsEmpty)
        {
            int escape = remaining.IndexOfAny(JsonEscapes);
            if (escape < 0)
            {
                builder.Append(remaining);
                break;
            }

            builder.Append(remaining[..escape]);
            char character = remaining[escape];
            string? escaped = character switch
            {
                '"' => "\\\"",
                '\\' => "\\\\",
                '\b' => "\\b",
                '\f' => "\\f",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                _ => null
            };
            if (escaped is not null)
            {
                builder.Append(escaped);
            }
            else if (character is < ' ')
            {
                builder.Append("\\u").Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(character);
            }

            remaining = remaining[(escape + 1)..];
        }

        builder.Append('"');
    }
}
