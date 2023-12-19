// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Text;
using CppAst;

namespace Generator;

public static partial class CsCodeGenerator
{
    public static void GenerateEnums(CppCompilation compilation)
    {
        string visibility = _options.PublicVisiblity ? "public" : "internal";
        using CodeWriter writer = new(Path.Combine(_options.OutputPath, "Enums.cs"), false, _options.Namespace, new string[] { "System" });
        Dictionary<string, string> createdEnums = new();

        foreach (CppEnum cppEnum in compilation.Enums)
        {
            if (string.IsNullOrEmpty(cppEnum.Name))
            {
                continue;
            }

            bool isBitmask =
                cppEnum.Name == "WGPUBufferUsage" ||
                cppEnum.Name == "WGPUTextureUsage" ||
                cppEnum.Name == "WGPUShaderStage" ||
                cppEnum.Name == "WGPUColorWriteMask" ||
                cppEnum.Name == "WGPUMapMode" ||
                cppEnum.Name == "WGPUInstanceBackend" ||
                cppEnum.Name.EndsWith("Flag") ||
                cppEnum.Name.EndsWith("Flags");

            if (isBitmask)
            {
                writer.WriteLine("[Flags]");
            }

            string csName = GetCsCleanName(cppEnum.Name);

            createdEnums.Add(csName, cppEnum.Name);

            bool noneAdded = false;
            using (writer.PushBlock($"{visibility} enum {csName}"))
            {
                if (isBitmask &&
                    !cppEnum.Items.Any(enumItem => GetEnumItemName(enumItem.Name) == "None"))
                {
                    writer.WriteLine("None = 0,");
                    noneAdded = true;
                }

                foreach (CppEnumItem enumItem in cppEnum.Items)
                {
                    if (enumItem.Name.EndsWith("_BEGIN_RANGE") ||
                        enumItem.Name.EndsWith("_END_RANGE") ||
                        enumItem.Name.EndsWith("_RANGE_SIZE") ||
                        enumItem.Name.EndsWith("_Force32")
                        )
                    {
                        continue;
                    }

                    string enumItemName = GetEnumItemName(enumItem.Name);

                    if (enumItemName == "None" && noneAdded)
                    {
                        continue;
                    }

                    if (enumItemName == "Default")
                    {
                        continue;
                    }

                    if (enumItemName != "Count" && _options.EnumWriteUnmanagedTag)
                    {
                        writer.WriteLine($"/// <unmanaged>{enumItem.Name}</unmanaged>");
                    }

                    if (enumItem.ValueExpression is CppRawExpression rawExpression)
                    {
                        string enumValueName = GetEnumItemName(rawExpression.Text);
                        writer.WriteLine($"{enumItemName} = {enumValueName},");
                    }
                    else if (enumItem.ValueExpression is CppLiteralExpression literalExpression)
                    {
                        writer.WriteLine($"{enumItemName} = {literalExpression.Value},");
                    }
                    else if (enumItem.ValueExpression is CppBinaryExpression binaryExpression)
                    {
                        StringBuilder builder = new();
                        FormatCppBinaryExpression(binaryExpression, builder);
                        writer.WriteLine($"{enumItemName} = {builder},");
                    }
                    else
                    {
                        writer.WriteLine($"{enumItemName} = {enumItem.Value},");
                    }
                }
            }

            writer.WriteLine();

            // Map missing flags with typedefs to VkFlags
            foreach (CppTypedef typedef in compilation.Typedefs)
            {
                if (typedef.Name.EndsWith("Flags", StringComparison.OrdinalIgnoreCase) == false
                    || typedef.Name.Equals("WGPUFlags", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (typedef.ElementType is CppPointerType ||
                    typedef.Name == "WGPUInstanceFlags")
                {
                    continue;
                }

                if (createdEnums.ContainsKey(typedef.Name))
                {
                    continue;
                }

                csName = typedef.Name.Replace("Flags", "");
                AddCsMapping(typedef.Name, csName);
            }
        }
    }

    private static void FormatExpression(CppExpression expression, StringBuilder builder)
    {
        if (expression is CppRawExpression rawExpression)
        {
            builder.Append(GetEnumItemName(rawExpression.Text));
        }
        else if (expression is CppLiteralExpression literalExpression)
        {
            builder.Append(literalExpression.Value);
        }
        else if (expression is CppBinaryExpression binaryExpression)
        {
            FormatCppBinaryExpression(binaryExpression, builder);
        }
    }

    private static void FormatCppBinaryExpression(CppBinaryExpression expression, StringBuilder builder)
    {
        if (expression.Arguments != null && expression.Arguments.Count > 0)
        {
            FormatExpression(expression.Arguments[0], builder);
        }

        builder.Append(" ");
        builder.Append(expression.Operator);
        builder.Append(" ");

        if (expression.Arguments != null && expression.Arguments.Count > 1)
        {
            FormatExpression(expression.Arguments[1], builder);
        }
    }

    private static string GetEnumItemName(string cppEnumItemName)
    {
        string[] splits = cppEnumItemName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        string enumItemName = string.Join("", splits.Skip(1));
        if (char.IsNumber(enumItemName[0]))
        {
            return $"_{enumItemName}";
        }

        /// WGPUNativeFeature
        if (enumItemName == "PUSHCONSTANTS")
        {
            return "PushConstants";
        }
        else if (enumItemName == "TEXTUREADAPTERSPECIFICFORMATFEATURES")
        {
            return "TextureAdapterSpecificFormatFeatures";
        }
        else if (enumItemName == "MULTIDRAWINDIRECT")
        {
            return "MultiDrawIndirect";
        }
        else if (enumItemName == "MULTIDRAWINDIRECTCOUNT")
        {
            return "MultiDrawIndirectCount";
        }
        else if (enumItemName == "VERTEXWRITABLESTORAGE")
        {
            return "VertexWritableStorage";
        }

        return enumItemName;
    }

    private static string NormalizeEnumValue(string value)
    {
        if (value == "(~0U)")
        {
            return "~0u";
        }

        if (value == "(~0ULL)")
        {
            return "~0ul";
        }

        if (value == "(~0U-1)")
        {
            return "~0u - 1";
        }

        if (value == "(~0U-2)")
        {
            return "~0u - 2";
        }

        if (value == "(~0U-3)")
        {
            return "~0u - 3";
        }

        if (value.StartsWith("(") && value.EndsWith(")"))
        {
            value = value.Substring(1, value.Length - 2);
        }

        return value.Replace("ULL", "UL");
    }
}
