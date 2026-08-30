using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace MonadicTypes.Tooling;

internal readonly record struct PublicApiMember(string DeclaringType, string Signature);

internal static class PublicApiReader
{
    public static Dictionary<string, PublicApiMember> Read(string assemblyPath)
    {
        using FileStream stream = File.OpenRead(assemblyPath);
        using PEReader pe = new(stream);
        MetadataReader metadata = pe.GetMetadataReader();
        ApiTypeProvider provider = new(metadata);
        Dictionary<string, PublicApiMember> members = new(metadata.TypeDefinitions.Count, StringComparer.Ordinal);
        foreach (TypeDefinitionHandle handle in metadata.TypeDefinitions)
        {
            TypeDefinition type = metadata.GetTypeDefinition(handle);
            if (!IsPubliclyVisible(metadata, handle) || IsCompilerGeneratedType(metadata, handle))
            {
                continue;
            }

            AddType(metadata, provider, handle, type, members);
        }

        return members;
    }

    private static bool IsCompilerGeneratedType(MetadataReader metadata, TypeDefinitionHandle handle)
    {
        TypeDefinitionHandle current = handle;
        while (!current.IsNil)
        {
            TypeDefinition type = metadata.GetTypeDefinition(current);
            if (metadata.GetString(type.Name) is ['<', ..])
            {
                return true;
            }

            current = type.GetDeclaringType();
        }

        return false;
    }

    private static bool IsPubliclyVisible(MetadataReader metadata, TypeDefinitionHandle handle)
    {
        TypeDefinitionHandle current = handle;
        while (!current.IsNil)
        {
            TypeDefinition type = metadata.GetTypeDefinition(current);
            TypeDefinitionHandle declaring = type.GetDeclaringType();
            TypeAttributes visibility = type.Attributes & TypeAttributes.VisibilityMask;
            if (declaring.IsNil)
            {
                return visibility is TypeAttributes.Public;
            }

            if (visibility is not TypeAttributes.NestedPublic)
            {
                return false;
            }

            current = declaring;
        }

        return false;
    }

    private static void AddType(
        MetadataReader metadata,
        ApiTypeProvider provider,
        TypeDefinitionHandle handle,
        TypeDefinition type,
        Dictionary<string, PublicApiMember> members)
    {
        string documentationName = provider.DocumentationName(handle);
        string[] typeParameters = GenericParameterNames(metadata, type.GetGenericParameters());
        string displayName = DisplayTypeName(metadata.GetString(type.Name), typeParameters);
        byte nullableContext = NullableContext(metadata, type);
        members.Add("T:" + documentationName, new PublicApiMember(displayName, displayName));

        AddMethods(
            metadata,
            provider,
            type,
            documentationName,
            displayName,
            typeParameters,
            nullableContext,
            members);
        AddProperties(
            metadata,
            provider,
            type,
            documentationName,
            typeParameters,
            displayName,
            nullableContext,
            members);
        AddFields(metadata, provider, type, documentationName, typeParameters, displayName, members);
        AddEvents(metadata, provider, type, documentationName, typeParameters, displayName, members);
    }

    private static void AddMethods(
        MetadataReader metadata,
        ApiTypeProvider provider,
        TypeDefinition type,
        string documentationType,
        string displayType,
        string[] typeParameters,
        byte nullableContext,
        Dictionary<string, PublicApiMember> members)
    {
        Span<char> idBuffer = stackalloc char[256];
        Span<char> displayBuffer = stackalloc char[256];
        foreach (MethodDefinitionHandle handle in type.GetMethods())
        {
            MethodDefinition method = metadata.GetMethodDefinition(handle);
            string metadataName = metadata.GetString(method.Name);
            if (!IsPublic(method.Attributes)
                || IsAccessor(metadataName)
                || HasCompilerGeneratedAttribute(metadata, method.GetCustomAttributes()))
            {
                continue;
            }

            string[] methodParameters = GenericParameterNames(metadata, method.GetGenericParameters());
            GenericContext context = new(typeParameters, methodParameters);
            MethodSignature<ApiType> signature = method.DecodeSignature(provider, context);
            PooledTextBuilder id = new(idBuffer);
            string identity;
            try
            {
                id.Append("M:");
                id.Append(documentationType);
                id.Append('.');
                id.Append(metadataName switch
                {
                    ".ctor" => "#ctor",
                    ".cctor" => "#cctor",
                    _ => metadataName
                });
                if (methodParameters.Length is not 0)
                {
                    id.Append("``");
                    id.Append(methodParameters.Length);
                }
                AppendDocumentationParameters(ref id, signature.ParameterTypes);
                if (metadataName is "op_Implicit" or "op_Explicit")
                {
                    id.Append('~');
                    id.Append(signature.ReturnType.Documentation);
                }

                identity = id.ToString();
            }
            finally
            {
                id.Dispose();
            }

            PooledTextBuilder display = new(displayBuffer);
            try
            {
                AppendMethodDisplay(
                    ref display,
                    metadata,
                    method,
                    metadataName,
                    displayType,
                    methodParameters,
                    signature,
                    nullableContext);
                members.Add(identity, new PublicApiMember(displayType, display.ToString()));
            }
            finally
            {
                display.Dispose();
            }
        }
    }

    private static void AddProperties(
        MetadataReader metadata,
        ApiTypeProvider provider,
        TypeDefinition type,
        string documentationType,
        string[] typeParameters,
        string displayType,
        byte nullableContext,
        Dictionary<string, PublicApiMember> members)
    {
        GenericContext context = new(typeParameters, []);
        Span<char> idBuffer = stackalloc char[256];
        Span<char> displayBuffer = stackalloc char[128];
        foreach (PropertyDefinitionHandle handle in type.GetProperties())
        {
            PropertyDefinition property = metadata.GetPropertyDefinition(handle);
            PropertyAccessors accessors = property.GetAccessors();
            if (!IsPublic(metadata, accessors.Getter) && !IsPublic(metadata, accessors.Setter))
            {
                continue;
            }

            MethodSignature<ApiType> signature = property.DecodeSignature(provider, context);
            byte nullability = NullableAnnotation(metadata, property.GetCustomAttributes())
                ?? PropertyReturnNullability(metadata, accessors, nullableContext);
            ApiType propertyType = ApplyNullability(signature.ReturnType, nullability);
            string name = metadata.GetString(property.Name);
            PooledTextBuilder id = new(idBuffer);
            string identity;
            try
            {
                id.Append("P:");
                id.Append(documentationType);
                id.Append('.');
                id.Append(name);
                AppendDocumentationParameters(ref id, signature.ParameterTypes);
                identity = id.ToString();
            }
            finally
            {
                id.Dispose();
            }

            PooledTextBuilder display = new(displayBuffer);
            try
            {
                display.Append(propertyType.Display);
                display.Append(' ');
                if (signature.ParameterTypes.IsEmpty)
                {
                    display.Append(name);
                }
                else
                {
                    display.Append("this[");
                    AppendDisplayTypes(ref display, signature.ParameterTypes);
                    display.Append(']');
                }

                members.Add(identity, new PublicApiMember(displayType, display.ToString()));
            }
            finally
            {
                display.Dispose();
            }
        }
    }

    private static void AddFields(
        MetadataReader metadata,
        ApiTypeProvider provider,
        TypeDefinition type,
        string documentationType,
        string[] typeParameters,
        string displayType,
        Dictionary<string, PublicApiMember> members)
    {
        GenericContext context = new(typeParameters, []);
        foreach (FieldDefinitionHandle handle in type.GetFields())
        {
            FieldDefinition field = metadata.GetFieldDefinition(handle);
            if ((field.Attributes & FieldAttributes.FieldAccessMask) is not FieldAttributes.Public)
            {
                continue;
            }

            string name = metadata.GetString(field.Name);
            if (name is "value__")
            {
                continue;
            }

            ApiType fieldType = field.DecodeSignature(provider, context);
            members.Add(
                "F:" + documentationType + "." + name,
                new PublicApiMember(displayType, fieldType.Display + " " + name));
        }
    }

    private static void AddEvents(
        MetadataReader metadata,
        ApiTypeProvider provider,
        TypeDefinition type,
        string documentationType,
        string[] typeParameters,
        string displayType,
        Dictionary<string, PublicApiMember> members)
    {
        GenericContext context = new(typeParameters, []);
        foreach (EventDefinitionHandle handle in type.GetEvents())
        {
            EventDefinition @event = metadata.GetEventDefinition(handle);
            EventAccessors accessors = @event.GetAccessors();
            if (!IsPublic(metadata, accessors.Adder) && !IsPublic(metadata, accessors.Remover))
            {
                continue;
            }

            string name = metadata.GetString(@event.Name);
            ApiType eventType = provider.GetTypeFromHandle(metadata, @event.Type, context);
            members.Add(
                "E:" + documentationType + "." + name,
                new PublicApiMember(displayType, "event " + eventType.Display + " " + name));
        }
    }

    private static void AppendMethodDisplay(
        ref PooledTextBuilder builder,
        MetadataReader metadata,
        MethodDefinition method,
        string metadataName,
        string displayType,
        string[] genericParameters,
        MethodSignature<ApiType> signature,
        byte declaringNullableContext)
    {
        byte nullableContext = NullableContext(metadata, method, declaringNullableContext);
        ApiType returnType = ApplyNullability(
            signature.ReturnType,
            ReturnNullability(metadata, method, nullableContext));
        string name = metadataName switch
        {
            ".ctor" => displayType,
            ".cctor" => displayType,
            "op_Implicit" => "implicit operator " + returnType.Display,
            "op_Explicit" => "explicit operator " + returnType.Display,
            _ => metadataName
        };
        if (metadataName is not (".ctor" or ".cctor" or "op_Implicit" or "op_Explicit"))
        {
            builder.Append(returnType.Display);
            builder.Append(' ');
        }

        builder.Append(name);
        AppendGenericParameters(ref builder, genericParameters);
        builder.Append('(');
        AppendDisplayParameters(
            ref builder,
            metadata,
            signature.ParameterTypes,
            method.GetParameters(),
            nullableContext);
        builder.Append(')');
    }

    private static void AppendDocumentationParameters(
        ref PooledTextBuilder builder,
        ImmutableArray<ApiType> parameters)
    {
        if (parameters.IsEmpty)
        {
            return;
        }

        builder.Append('(');
        for (int index = 0; index < parameters.Length; index++)
        {
            if (index is not 0)
            {
                builder.Append(',');
            }

            builder.Append(parameters[index].Documentation);
        }

        builder.Append(')');
    }

    private static void AppendDisplayParameters(
        ref PooledTextBuilder builder,
        MetadataReader metadata,
        ImmutableArray<ApiType> parameters,
        ParameterHandleCollection handles,
        byte nullableContext)
    {
        ParameterHandleCollection.Enumerator enumerator = handles.GetEnumerator();
        bool hasParameter = enumerator.MoveNext();
        for (int index = 0; index < parameters.Length; index++)
        {
            if (index is not 0)
            {
                builder.Append(", ");
            }

            Parameter definition = default;
            while (hasParameter)
            {
                definition = metadata.GetParameter(enumerator.Current);
                if (definition.SequenceNumber >= index + 1)
                {
                    break;
                }

                hasParameter = enumerator.MoveNext();
            }

            ApiType parameterType = ApplyNullability(
                parameters[index],
                definition.Name.IsNil
                    ? nullableContext
                    : NullableAnnotation(metadata, definition.GetCustomAttributes()) ?? nullableContext);
            string display = parameterType.Display;
            if (display.StartsWith("ref ", StringComparison.Ordinal))
            {
                string modifier = definition.Attributes switch
                {
                    var attributes when (attributes & ParameterAttributes.Out) is not 0 => "out ",
                    var attributes when (attributes & ParameterAttributes.In) is not 0 => "in ",
                    _ => "ref "
                };
                builder.Append(modifier);
                builder.Append(display.AsSpan(4));
            }
            else
            {
                builder.Append(display);
            }

            if (!definition.Name.IsNil)
            {
                builder.Append(' ');
                builder.Append(metadata.GetString(definition.Name));
            }

            hasParameter = hasParameter && enumerator.MoveNext();
        }
    }

    private static byte NullableContext(
        MetadataReader metadata,
        MethodDefinition method,
        byte declaringContext)
    {
        byte? methodContext = NullableAnnotation(
            metadata,
            method.GetCustomAttributes(),
            "NullableContextAttribute");
        if (methodContext is not null)
        {
            return methodContext.GetValueOrDefault();
        }

        return declaringContext;
    }

    private static byte NullableContext(MetadataReader metadata, TypeDefinition type) =>
        NullableAnnotation(
            metadata,
            type.GetCustomAttributes(),
            "NullableContextAttribute") ?? 0;

    private static byte PropertyReturnNullability(
        MetadataReader metadata,
        PropertyAccessors accessors,
        byte nullableContext) =>
        accessors.Getter.IsNil
            ? nullableContext
            : ReturnNullability(
                metadata,
                metadata.GetMethodDefinition(accessors.Getter),
                nullableContext);

    private static byte ReturnNullability(
        MetadataReader metadata,
        MethodDefinition method,
        byte nullableContext)
    {
        foreach (ParameterHandle handle in method.GetParameters())
        {
            Parameter parameter = metadata.GetParameter(handle);
            if (parameter.SequenceNumber is 0)
            {
                return NullableAnnotation(metadata, parameter.GetCustomAttributes()) ?? nullableContext;
            }
        }

        return nullableContext;
    }

    private static ApiType ApplyNullability(ApiType type, byte annotation) =>
        annotation is 2 && type.IsNullableCapable
            ? type with { Display = type.Display + "?" }
            : type;

    private static byte? NullableAnnotation(
        MetadataReader metadata,
        CustomAttributeHandleCollection attributes,
        string attributeName = "NullableAttribute")
    {
        foreach (CustomAttributeHandle handle in attributes)
        {
            CustomAttribute attribute = metadata.GetCustomAttribute(handle);
            if (!IsAttribute(metadata, attribute, attributeName, "System.Runtime.CompilerServices"))
            {
                continue;
            }

            BlobReader value = metadata.GetBlobReader(attribute.Value);
            if (value.RemainingBytes < 3 || value.ReadUInt16() is not 1)
            {
                return null;
            }

            if (value.RemainingBytes is 3)
            {
                return value.ReadByte();
            }

            int count = value.ReadInt32();
            return count > 0 && value.RemainingBytes >= count + 2
                ? value.ReadByte()
                : null;
        }

        return null;
    }

    private static bool IsAttribute(
        MetadataReader metadata,
        CustomAttribute attribute,
        string name,
        string @namespace)
    {
        EntityHandle type = attribute.Constructor.Kind switch
        {
            HandleKind.MemberReference => metadata.GetMemberReference(
                (MemberReferenceHandle)attribute.Constructor).Parent,
            HandleKind.MethodDefinition => metadata.GetMethodDefinition(
                (MethodDefinitionHandle)attribute.Constructor).GetDeclaringType(),
            _ => default
        };

        return type.Kind switch
        {
            HandleKind.TypeReference => IsAttribute(
                metadata,
                metadata.GetTypeReference((TypeReferenceHandle)type),
                name,
                @namespace),
            HandleKind.TypeDefinition => IsAttribute(
                metadata,
                metadata.GetTypeDefinition((TypeDefinitionHandle)type),
                name,
                @namespace),
            _ => false
        };
    }

    private static bool IsAttribute(
        MetadataReader metadata,
        TypeReference type,
        string name,
        string @namespace) =>
        metadata.StringComparer.Equals(type.Name, name)
        && metadata.StringComparer.Equals(type.Namespace, @namespace);

    private static bool IsAttribute(
        MetadataReader metadata,
        TypeDefinition type,
        string name,
        string @namespace) =>
        metadata.StringComparer.Equals(type.Name, name)
        && metadata.StringComparer.Equals(type.Namespace, @namespace);

    private static void AppendDisplayTypes(ref PooledTextBuilder builder, ImmutableArray<ApiType> parameters)
    {
        for (int index = 0; index < parameters.Length; index++)
        {
            if (index is not 0)
            {
                builder.Append(", ");
            }

            builder.Append(parameters[index].Display);
        }
    }

    private static void AppendGenericParameters(ref PooledTextBuilder builder, string[] parameters)
    {
        if (parameters.Length is 0)
        {
            return;
        }

        builder.Append('<');
        builder.Append(parameters[0]);
        for (int index = 1; index < parameters.Length; index++)
        {
            builder.Append(", ");
            builder.Append(parameters[index]);
        }

        builder.Append('>');
    }

    private static string[] GenericParameterNames(
        MetadataReader metadata,
        GenericParameterHandleCollection handles)
    {
        if (handles.Count is 0)
        {
            return [];
        }

        string[] names = new string[handles.Count];
        foreach (GenericParameterHandle handle in handles)
        {
            GenericParameter parameter = metadata.GetGenericParameter(handle);
            names[parameter.Index] = metadata.GetString(parameter.Name);
        }

        return names;
    }

    private static string DisplayTypeName(string metadataName, string[] parameters)
    {
        int arity = metadataName.IndexOf('`');
        string name = arity < 0 ? metadataName : metadataName[..arity];
        if (parameters.Length is 0)
        {
            return name;
        }

        Span<char> initial = stackalloc char[128];
        PooledTextBuilder builder = new(initial);
        try
        {
            builder.Append(name);
            AppendGenericParameters(ref builder, parameters);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private static bool IsPublic(MetadataReader metadata, MethodDefinitionHandle handle) =>
        !handle.IsNil && IsPublic(metadata.GetMethodDefinition(handle).Attributes);

    private static bool IsAccessor(string name) => name is
        ['g', 'e', 't', '_', ..]
        or ['s', 'e', 't', '_', ..]
        or ['a', 'd', 'd', '_', ..]
        or ['r', 'e', 'm', 'o', 'v', 'e', '_', ..];

    private static bool HasCompilerGeneratedAttribute(
        MetadataReader metadata,
        CustomAttributeHandleCollection attributes)
    {
        foreach (CustomAttributeHandle handle in attributes)
        {
            CustomAttribute attribute = metadata.GetCustomAttribute(handle);
            EntityHandle type = attribute.Constructor.Kind switch
            {
                HandleKind.MemberReference => metadata.GetMemberReference(
                    (MemberReferenceHandle)attribute.Constructor).Parent,
                HandleKind.MethodDefinition => metadata.GetMethodDefinition(
                    (MethodDefinitionHandle)attribute.Constructor).GetDeclaringType(),
                _ => default
            };

            bool compilerGenerated = type.Kind switch
            {
                HandleKind.TypeReference => IsCompilerGeneratedAttribute(
                    metadata,
                    metadata.GetTypeReference((TypeReferenceHandle)type)),
                HandleKind.TypeDefinition => IsCompilerGeneratedAttribute(
                    metadata,
                    metadata.GetTypeDefinition((TypeDefinitionHandle)type)),
                _ => false
            };
            if (compilerGenerated)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsCompilerGeneratedAttribute(MetadataReader metadata, TypeReference type) =>
        metadata.StringComparer.Equals(type.Name, "CompilerGeneratedAttribute")
        && metadata.StringComparer.Equals(type.Namespace, "System.Runtime.CompilerServices");

    private static bool IsCompilerGeneratedAttribute(MetadataReader metadata, TypeDefinition type) =>
        metadata.StringComparer.Equals(type.Name, "CompilerGeneratedAttribute")
        && metadata.StringComparer.Equals(type.Namespace, "System.Runtime.CompilerServices");

    private static bool IsPublic(MethodAttributes attributes) =>
        (attributes & MethodAttributes.MemberAccessMask) is MethodAttributes.Public;
}

internal readonly record struct GenericContext(string[] TypeParameters, string[] MethodParameters);

internal readonly record struct ApiType(
    string Documentation,
    string Display,
    bool IsNullableCapable = false)
{
    public ApiType MakeByRef() => new(Documentation + "@", "ref " + Display, IsNullableCapable);
}

internal sealed class ApiTypeProvider(MetadataReader metadata) : ISignatureTypeProvider<ApiType, GenericContext>
{
    private static readonly string[] TypeParameterDocumentation =
    [
        "`0", "`1", "`2", "`3", "`4", "`5", "`6", "`7",
        "`8", "`9", "`10", "`11", "`12", "`13", "`14", "`15"
    ];
    private static readonly string[] MethodParameterDocumentation =
    [
        "``0", "``1", "``2", "``3", "``4", "``5", "``6", "``7",
        "``8", "``9", "``10", "``11", "``12", "``13", "``14", "``15"
    ];
    private readonly MetadataReader _metadata = metadata;
    private readonly ApiType[] _definitions = new ApiType[metadata.TypeDefinitions.Count + 1];
    private readonly ApiType[] _references = new ApiType[metadata.TypeReferences.Count + 1];

    public string DocumentationName(TypeDefinitionHandle handle) =>
        GetTypeFromDefinitionCore(handle).Documentation;

    public ApiType GetArrayType(ApiType elementType, ArrayShape shape)
    {
        string rank = shape.Rank is 1 ? "[]" : "[" + new string(',', shape.Rank - 1) + "]";
        return new ApiType(elementType.Documentation + rank, elementType.Display + rank, true);
    }

    public ApiType GetByReferenceType(ApiType elementType) => elementType.MakeByRef();

    public ApiType GetFunctionPointerType(MethodSignature<ApiType> signature) =>
        new("System.IntPtr", "delegate*");

    public ApiType GetGenericInstantiation(ApiType genericType, ImmutableArray<ApiType> typeArguments)
    {
        return new ApiType(
            FormatGenericType(genericType.Documentation, typeArguments, documentation: true),
            FormatGenericType(genericType.Display, typeArguments, documentation: false),
            genericType.IsNullableCapable);
    }

    public ApiType GetGenericMethodParameter(GenericContext genericContext, int index) =>
        new(GenericParameterDocumentation(MethodParameterDocumentation, "``", index), genericContext.MethodParameters[index], true);

    public ApiType GetGenericTypeParameter(GenericContext genericContext, int index) =>
        new(GenericParameterDocumentation(TypeParameterDocumentation, "`", index), genericContext.TypeParameters[index], true);

    public ApiType GetModifiedType(ApiType modifier, ApiType unmodifiedType, bool isRequired) => unmodifiedType;

    public ApiType GetPinnedType(ApiType elementType) => elementType;

    public ApiType GetPointerType(ApiType elementType) =>
        new(elementType.Documentation + "*", elementType.Display + "*");

    public ApiType GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode switch
    {
        PrimitiveTypeCode.Boolean => new("System.Boolean", "bool"),
        PrimitiveTypeCode.Byte => new("System.Byte", "byte"),
        PrimitiveTypeCode.Char => new("System.Char", "char"),
        PrimitiveTypeCode.Double => new("System.Double", "double"),
        PrimitiveTypeCode.Int16 => new("System.Int16", "short"),
        PrimitiveTypeCode.Int32 => new("System.Int32", "int"),
        PrimitiveTypeCode.Int64 => new("System.Int64", "long"),
        PrimitiveTypeCode.IntPtr => new("System.IntPtr", "nint"),
        PrimitiveTypeCode.Object => new("System.Object", "object", true),
        PrimitiveTypeCode.SByte => new("System.SByte", "sbyte"),
        PrimitiveTypeCode.Single => new("System.Single", "float"),
        PrimitiveTypeCode.String => new("System.String", "string", true),
        PrimitiveTypeCode.UInt16 => new("System.UInt16", "ushort"),
        PrimitiveTypeCode.UInt32 => new("System.UInt32", "uint"),
        PrimitiveTypeCode.UInt64 => new("System.UInt64", "ulong"),
        PrimitiveTypeCode.UIntPtr => new("System.UIntPtr", "nuint"),
        PrimitiveTypeCode.Void => new("System.Void", "void"),
        _ => new("System.Object", "object")
    };

    public ApiType GetSZArrayType(ApiType elementType) =>
        new(elementType.Documentation + "[]", elementType.Display + "[]", true);

    public ApiType GetTypeFromDefinition(
        MetadataReader reader,
        TypeDefinitionHandle handle,
        byte rawTypeKind)
    {
        ApiType type = GetTypeFromDefinitionCore(handle);
        return rawTypeKind is (byte)SignatureTypeKind.Class
            ? type with { IsNullableCapable = true }
            : type;
    }

    public ApiType GetTypeFromReference(
        MetadataReader reader,
        TypeReferenceHandle handle,
        byte rawTypeKind)
    {
        int row = MetadataTokens.GetRowNumber(handle);
        ApiType type = _references[row];
        if (type.Documentation is null)
        {
            TypeReference reference = reader.GetTypeReference(handle);
            string name = reader.GetString(reference.Name);
            string documentation = reference.ResolutionScope.Kind is HandleKind.TypeReference
                ? string.Concat(
                    GetTypeFromReference(reader, (TypeReferenceHandle)reference.ResolutionScope, 0).Documentation,
                    "+",
                    name)
                : QualifiedName(reader.GetString(reference.Namespace), name);
            type = new ApiType(documentation, ShortDisplayName(documentation));
            _references[row] = type;
        }

        return rawTypeKind is (byte)SignatureTypeKind.Class
            ? type with { IsNullableCapable = true }
            : type;
    }

    public ApiType GetTypeFromSpecification(
        MetadataReader reader,
        GenericContext genericContext,
        TypeSpecificationHandle handle,
        byte rawTypeKind) => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);

    public ApiType GetTypeFromHandle(MetadataReader reader, EntityHandle handle, GenericContext context) => handle.Kind switch
    {
        HandleKind.TypeDefinition => GetTypeFromDefinition(reader, (TypeDefinitionHandle)handle, 0),
        HandleKind.TypeReference => GetTypeFromReference(reader, (TypeReferenceHandle)handle, 0),
        HandleKind.TypeSpecification => GetTypeFromSpecification(reader, context, (TypeSpecificationHandle)handle, 0),
        _ => new ApiType("System.Object", "object")
    };

    private static string StripArity(string value)
    {
        int arity = value.LastIndexOf('`');
        return arity < 0 ? value : value[..arity];
    }

    private static string GenericParameterDocumentation(string[] cache, string prefix, int index) =>
        index < cache.Length
            ? cache[index]
            : string.Concat(prefix, index.ToString(CultureInfo.InvariantCulture));

    private static string FormatGenericType(
        string genericType,
        ImmutableArray<ApiType> typeArguments,
        bool documentation)
    {
        Span<char> initial = stackalloc char[256];
        PooledTextBuilder builder = new(initial);
        try
        {
            builder.Append(StripArity(genericType));
            builder.Append(documentation ? '{' : '<');
            for (int index = 0; index < typeArguments.Length; index++)
            {
                if (index is not 0)
                {
                    builder.Append(documentation ? "," : ", ");
                }

                builder.Append(documentation
                    ? typeArguments[index].Documentation
                    : typeArguments[index].Display);
            }

            builder.Append(documentation ? '}' : '>');
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private static string ShortDisplayName(string documentation)
    {
        int separator = documentation.LastIndexOfAny(['.', '+']);
        string name = separator < 0 ? documentation : documentation[(separator + 1)..];
        return StripArity(name);
    }

    private ApiType GetTypeFromDefinitionCore(TypeDefinitionHandle handle)
    {
        int row = MetadataTokens.GetRowNumber(handle);
        ApiType cached = _definitions[row];
        if (cached.Documentation is not null)
        {
            return cached;
        }

        MetadataReader metadata = _metadata;
        TypeDefinition definition = metadata.GetTypeDefinition(handle);
        string name = metadata.GetString(definition.Name);
        TypeDefinitionHandle declaring = definition.GetDeclaringType();
        string documentation;
        if (!declaring.IsNil)
        {
            documentation = string.Concat(GetTypeFromDefinitionCore(declaring).Documentation, "+", name);
        }
        else
        {
            documentation = QualifiedName(metadata.GetString(definition.Namespace), name);
        }

        cached = new ApiType(documentation, ShortDisplayName(documentation));
        _definitions[row] = cached;
        return cached;
    }

    private static string QualifiedName(string @namespace, string name) =>
        @namespace.Length is 0 ? name : string.Concat(@namespace, ".", name);
}
