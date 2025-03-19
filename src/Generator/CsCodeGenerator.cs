// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using CppAst;

namespace Generator;

public partial class CsCodeGenerator
{
    private static readonly HashSet<string> s_keywords =
    [
        "object",
        "event",
        "base",
        "delegate",
        "string",
        "int",
    ];

    private static readonly Dictionary<string, string> s_knownTypeNameMappings = new()
    {
        { "bool", "bool" },
        { "uint8_t", "byte" },
        { "uint16_t", "ushort" },
        { "uint32_t", "uint" },
        { "uint64_t", "ulong" },
        { "int8_t", "sbyte" },
        { "int32_t", "int" },
        { "int16_t", "short" },
        { "int64_t", "long" },
        { "int64_t*", "long*" },
        { "char", "byte" },
        { "size_t", "nuint" },
        { "intptr_t", "nint" },
        { "uintptr_t", "nuint" },

        { "WGPUSubmissionIndex", "ulong" },
        { "WGPUProc", "nint" },
        // { "WGPUInstanceFlag", "WGPUInstanceFlags" },
    };

    private readonly Dictionary<string, string> _csNameMappings = [];

    private readonly CsCodeGeneratorOptions _options;

    public CsCodeGenerator(CsCodeGeneratorOptions options)
    {
        _options = options;
    }

    public void Generate(CppCompilation compilation)
    {
        GenerateConstants(compilation);
        GenerateEnums(compilation);
        GenerateHandles(compilation);
        GenerateStructAndUnions(compilation);
        GenerateCommands(compilation);
    }

    public void AddCsMapping(string typeName, string csTypeName)
    {
        _csNameMappings[typeName] = csTypeName;
    }

    private static string PrettyString(string str)
    {
        return char.ToUpperInvariant(str[0]) + str.Substring(1);
    }

    private static string NormalizeFieldName(string name)
    {
        if (s_keywords.Contains(name))
            return "@" + name;

        return name;
    }

    private string GetCsCleanName(string name)
    {
        if (s_knownTypeNameMappings.TryGetValue(name, out string? knownMappedName))
        {
            return GetCsCleanName(knownMappedName);
        }
        else if(_csNameMappings.TryGetValue(name, out string? mappedName))
        {
            return GetCsCleanName(mappedName);
        }
        else if (name.StartsWith("PFN"))
        {
            return "nint";
        }

        return name;
    }

    private string GetCsTypeName(CppType? type)
    {
        if (type is CppPrimitiveType primitiveType)
        {
            return GetCsTypeName(primitiveType);
        }

        if (type is CppQualifiedType qualifiedType)
        {
            return GetCsTypeName(qualifiedType.ElementType);
        }

        if (type is CppEnum enumType)
        {
            string enumCsName = GetCsCleanName(enumType.Name);
            return enumCsName;
        }

        if (type is CppTypedef typedef)
        {
            if (typedef.ElementType is CppClass classElementType)
            {
                return GetCsTypeName(classElementType);
            }

            string typeDefCsName = GetCsCleanName(typedef.Name);
            return typeDefCsName;
        }

        if (type is CppClass @class)
        {
            string className = GetCsCleanName(@class.Name);
            return className;
        }

        if (type is CppPointerType pointerType)
        {
            string csPointerTypeName = GetCsTypeName(pointerType);
            if (csPointerTypeName == "IntPtr" || csPointerTypeName == "nint" /*&& s_csNameMappings.ContainsKey(pointerType.)*/)
            {
                return csPointerTypeName;
            }

            return csPointerTypeName + "*";
        }

        if (type is CppArrayType arrayType)
        {
            return GetCsTypeName(arrayType.ElementType) + "*";
        }

        return string.Empty;
    }

    private static string GetCsTypeName(CppPrimitiveType primitiveType)
    {
        switch (primitiveType.Kind)
        {
            case CppPrimitiveKind.Void:
                return "void";

            case CppPrimitiveKind.Char:
                return "byte";

            case CppPrimitiveKind.Bool:
                return "bool";

            case CppPrimitiveKind.WChar:
                return "char";

            case CppPrimitiveKind.Short:
                return "short";
            case CppPrimitiveKind.Int:
                return "int";

            case CppPrimitiveKind.LongLong:
                return "long";
            case CppPrimitiveKind.UnsignedChar:
                return "byte";
            case CppPrimitiveKind.UnsignedShort:
                return "ushort";
            case CppPrimitiveKind.UnsignedInt:
                return "uint";

            case CppPrimitiveKind.UnsignedLongLong:
                return "ulong";
            case CppPrimitiveKind.Float:
                return "float";
            case CppPrimitiveKind.Double:
                return "double";
            case CppPrimitiveKind.LongDouble:
                return "double";

            default:
                throw new InvalidOperationException($"Unknown primitive type: {primitiveType.Kind}");
        }
    }

    private string GetCsTypeName(CppPointerType pointerType)
    {
        if (pointerType.ElementType is CppQualifiedType qualifiedType)
        {
            if (qualifiedType.ElementType is CppPrimitiveType primitiveType)
            {
                return GetCsTypeName(primitiveType);
            }
            else if (qualifiedType.ElementType is CppClass @classType)
            {
                return GetCsTypeName(@classType);
            }
            else if (qualifiedType.ElementType is CppPointerType subPointerType)
            {
                return GetCsTypeName(subPointerType) + "*";
            }
            else if (qualifiedType.ElementType is CppTypedef typedef)
            {
                return GetCsTypeName(typedef);
            }
            else if (qualifiedType.ElementType is CppEnum @enum)
            {
                return GetCsTypeName(@enum);
            }

            return GetCsTypeName(qualifiedType.ElementType);
        }

        return GetCsTypeName(pointerType.ElementType);
    }
}
