// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Text;
using CppAst;

namespace Generator;

public static partial class CsCodeGenerator
{
    private static readonly Dictionary<string, string> s_knownEnumValueNames = new()
    {
        // VkStructureType
        { "VK_STRUCTURE_TYPE_MACOS_SURFACE_CREATE_INFO_MVK", "MacOSSurfaceCreateInfoMVK" },
        { "VK_STRUCTURE_TYPE_TEXTURE_LOD_GATHER_FORMAT_PROPERTIES_AMD", "TextureLODGatherFormatPropertiesAMD" },
        { "VK_STRUCTURE_TYPE_PRESENT_ID_KHR", "PresentIdKHR" },
        { "VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_TEXTURE_COMPRESSION_ASTC_HDR_FEATURES", "PhysicalDeviceTextureCompressionASTCHDRFeatures" },
        { "VK_STRUCTURE_TYPE_ANDROID_SURFACE_CREATE_INFO_KHR", "AndroidSurfaceCreateInfoKHR" },
        { "VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_RGBA10X6_FORMATS_FEATURES_EXT", "PhysicalDeviceRGBA10X6FormatsFeaturesEXT" },
        { "VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_PRESENT_ID_FEATURES_KHR", "PhysicalDevicePresentIdFeaturesKHR" },
        { "VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_8BIT_STORAGE_FEATURES", "PhysicalDevice8BitStorageFeatures" },
        { "VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_16BIT_STORAGE_FEATURES", "PhysicalDevice16BitStorageFeatures" },
        //{ "VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_EXTERNAL_MEMORY_RDMA_FEATURES_NV", "PhysicalDeviceExternalMemoryRDMAFeaturesNV" },

        {  "VK_STENCIL_FRONT_AND_BACK", "FrontAndBack" },
        {  "VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_FLAGS_INFO", "MemoryAllocateFlagsInfo" },

        // VkSampleCountFlagBits
        {  "VK_SAMPLE_COUNT_1_BIT", "Count1" },
        {  "VK_SAMPLE_COUNT_2_BIT", "Count2" },
        {  "VK_SAMPLE_COUNT_4_BIT", "Count4" },
        {  "VK_SAMPLE_COUNT_8_BIT", "Count8" },
        {  "VK_SAMPLE_COUNT_16_BIT", "Count16" },
        {  "VK_SAMPLE_COUNT_32_BIT", "Count32" },
        {  "VK_SAMPLE_COUNT_64_BIT", "Count64" },

        // VkImageType
        { "VK_IMAGE_TYPE_1D", "Image1D" },
        { "VK_IMAGE_TYPE_2D", "Image2D" },
        { "VK_IMAGE_TYPE_3D", "Image3D" },
    };

    private static readonly HashSet<string> s_ignoredParts = new(StringComparer.OrdinalIgnoreCase)
    {
        //"flags",
        "bit",
        //"nv",
    };

    private static readonly HashSet<string> s_preserveCaps = new(StringComparer.OrdinalIgnoreCase)
    {
        "khr",
        "khx",
        "ext",
        "nv",
        "nvx",
        "nvidia",
        "amd",
        "intel",
        "arm",
        "mvk",
        "nn",
        //"android",
        "google",
        "fuchsia",
        "huawei",
        "valve",
        "qcom",
        "macos",
        "ios",
        "id",
        "pci",
        "bit",
        "astc",
        "aabb",
        "sm",
        "rdma",
        "2d",
        "3d",
        "io",
        "sec",
        "lunarg",
        "d3d12",
    };

    public static void GenerateEnums(CppCompilation compilation, string outputPath)
    {
        using CodeWriter writer = new(Path.Combine(outputPath, "Enums.cs"), false, "System");
        Dictionary<string, string> createdEnums = new();

        foreach (CppEnum cppEnum in compilation.Enums)
        {
            bool isBitmask =
                cppEnum.Name == "WGPUTextureUsage" ||
                cppEnum.Name == "WGPUShaderStage" ||
                cppEnum.Name == "WGPUColorWriteMask";

            if (isBitmask)
            {
                writer.WriteLine("[Flags]");
            }

            string csName = GetCsCleanName(cppEnum.Name);

            if (csName == "WGPUTextureUsageFlags")
            {

            }

            // Rename FlagBits in Flags.
            //if (isBitmask)
            //{
            //    csName = csName.Replace("FlagBits", "Flags");
            //    AddCsMapping(cppEnum.Name, csName);
            //}

            createdEnums.Add(csName, cppEnum.Name);

            bool noneAdded = false;
            using (writer.PushBlock($"public enum {csName}"))
            {
                if (isBitmask &&
                    !cppEnum.Items.Any(item => item.Name == "None"))
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

                    string enumItemName = GetEnumItemName(cppEnum, enumItem.Name);

                    if (enumItemName == "None" && noneAdded)
                    {
                        continue;
                    }

                    writer.WriteLine($"/// <unmanaged>{enumItem.Name}</unmanaged>");
                    if (enumItem.ValueExpression is CppRawExpression rawExpression)
                    {
                        string enumValueName = GetEnumItemName(cppEnum, rawExpression.Text);
                        writer.WriteLine($"{enumItemName} = {enumValueName},");
                    }
                    else
                    {
                        writer.WriteLine($"{enumItemName} = {enumItem.Value},");
                    }
                }
            }

            writer.WriteLine();
        }

        // Map missing flags with typedefs to VkFlags
        foreach (CppTypedef typedef in compilation.Typedefs)
        {
            if (typedef.Name.StartsWith("PFN_")
                || typedef.Name.Equals("VkBool32", StringComparison.OrdinalIgnoreCase)
                || typedef.Name.Equals("VkFlags", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (typedef.ElementType is CppPointerType)
            {
                continue;
            }

            if (createdEnums.ContainsKey(typedef.Name))
            {
                continue;
            }

            if (typedef.Name.EndsWith("Flags", StringComparison.OrdinalIgnoreCase) ||
                typedef.Name.EndsWith("FlagsKHR", StringComparison.OrdinalIgnoreCase) ||
                typedef.Name.EndsWith("FlagsEXT", StringComparison.OrdinalIgnoreCase) ||
                typedef.Name.EndsWith("FlagsNV", StringComparison.OrdinalIgnoreCase) ||
                typedef.Name.EndsWith("FlagsAMD", StringComparison.OrdinalIgnoreCase) ||
                typedef.Name.EndsWith("FlagsMVK", StringComparison.OrdinalIgnoreCase) ||
                typedef.Name.EndsWith("FlagsNN", StringComparison.OrdinalIgnoreCase))
            {
                writer.WriteLine("[Flags]");
                using (writer.PushBlock($"public enum {typedef.Name}"))
                {
                    writer.WriteLine("None = 0,");
                }
                writer.WriteLine();
            }
        }

        // Defined with specs 1.2.170 => VK_KHR_synchronization2
        string lastCreatedEnum = string.Empty;
        foreach (CppField cppField in compilation.Fields)
        {
            string? fieldType = GetCsTypeName(cppField.Type, false);
            string createdEnumName;

            if (!createdEnums.ContainsKey(fieldType))
            {
                if (!string.IsNullOrEmpty(lastCreatedEnum))
                {
                    writer.EndBlock();
                    writer.WriteLine();
                }

                createdEnums.Add(fieldType, fieldType);
                lastCreatedEnum = fieldType;

                string baseType = "uint";
                if (cppField.Type is CppQualifiedType qualifiedType)
                {
                    if (qualifiedType.ElementType is CppTypedef typedef)
                    {
                        baseType = GetCsTypeName(typedef.ElementType, false);
                    }
                    else
                    {
                        baseType = GetCsTypeName(qualifiedType.ElementType, false);
                    }
                }

                if (fieldType.EndsWith("FlagBits2"))
                {
                    fieldType = fieldType.Replace("FlagBits2", "Flags2");
                }

                writer.WriteLine("[Flags]");
                writer.BeginBlock($"public enum {fieldType} : {baseType}");
                createdEnumName = fieldType;
            }
            else
            {
                createdEnumName = createdEnums[fieldType];
            }

            string csFieldName = string.Empty;
            if (cppField.Name.StartsWith("VK_PIPELINE_STAGE_2"))
            {
                csFieldName = GetPrettyEnumName(cppField.Name, "VK_PIPELINE_STAGE_2");
            }
            else if (cppField.Name.StartsWith("VK_ACCESS_2"))
            {
                csFieldName = GetPrettyEnumName(cppField.Name, "VK_ACCESS_2");
            }
            else if (cppField.Name.StartsWith("VK_FORMAT_FEATURE_2"))
            {
                csFieldName = GetPrettyEnumName(cppField.Name, "VK_FORMAT_FEATURE_2");
            }
            else
            {
                csFieldName = NormalizeFieldName(cppField.Name);
            }

            // Remove vendor suffix from enum value if enum already contains it
            if (csFieldName.EndsWith("KHR", StringComparison.Ordinal) &&
                createdEnumName.EndsWith("KHR", StringComparison.Ordinal))
            {
                csFieldName = csFieldName.Substring(0, csFieldName.Length - 3);
            }

            writer.WriteLine($"{csFieldName} = {cppField.InitValue},");
        }

        if (!string.IsNullOrEmpty(lastCreatedEnum))
        {
            writer.EndBlock();
        }
    }

    private static string GetEnumItemName(CppEnum @enum, string cppEnumItemName)
    {
        string enumItemName = cppEnumItemName.Split('_')[1];
        if (char.IsNumber(enumItemName[0]))
        {
            return $"_{enumItemName}";
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

    private static string GetPrettyEnumName(string value, string enumPrefix)
    {
        if (s_knownEnumValueNames.TryGetValue(value, out string? knownName))
        {
            return knownName;
        }

        if (value.IndexOf(enumPrefix) != 0)
        {
            return value;
        }

        string[] parts = value[enumPrefix.Length..].Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);

        var sb = new StringBuilder();
        foreach (string part in parts)
        {
            if (s_ignoredParts.Contains(part))
            {
                continue;
            }

            if (s_preserveCaps.Contains(part))
            {
                sb.Append(part);
            }
            else
            {
                sb.Append(char.ToUpper(part[0]));
                for (int i = 1; i < part.Length; i++)
                {
                    sb.Append(char.ToLower(part[i]));
                }
            }
        }

        string prettyName = sb.ToString();
        if (char.IsNumber(prettyName[0]))
        {
            if (enumPrefix.EndsWith("_IDC"))
            {
                return "Idc" + prettyName;
            }

            if (enumPrefix.EndsWith("_POC_TYPE"))
            {
                return "Type" + prettyName;
            }

            if (enumPrefix.EndsWith("_CTB_SIZE"))
            {
                return "Size" + prettyName;
            }

            if (enumPrefix.EndsWith("_BLOCK_SIZE"))
            {
                return "Size" + prettyName;
            }

            if (enumPrefix.EndsWith("_FIXED_RATE"))
            {
                return "Rate" + prettyName;
            }

            if (enumPrefix.EndsWith("_SUBSAMPLING"))
            {
                return "Subsampling" + prettyName;
            }

            if (enumPrefix.EndsWith("_BIT_DEPTH"))
            {
                return "Depth" + prettyName;
            }

            return "_" + prettyName;
        }

        return prettyName;
    }
}
