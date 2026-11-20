// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Text;
using CppAst;

namespace Generator;

partial class CsCodeGenerator
{
    private readonly HashSet<string> _enumConstants = [];

    private void GenerateConstants(CppCompilation compilation)
    {
        string visibility = _options.PublicVisiblity ? "public" : "internal";
        using var writer = new CodeWriter(Path.Combine(_options.OutputPath, "Constants.cs"), false, _options.Namespace, Array.Empty<string>());
        using (writer.PushBlock($"{visibility} static partial class {_options.ClassName}"))
        {
            bool needNewLine = false;
            foreach (CppMacro cppMacro in compilation.Macros)
            {
                if (string.IsNullOrEmpty(cppMacro.Value)
                    || _options.ExcludeConstants.Contains(cppMacro.Name)
                    || cppMacro.Name.EndsWith("_H_", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("WGPU_EXPORT", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("WGPU_SHARED_LIBRARY", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("WGPU_IMPLEMENTATION", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("_wgpu_COMMA", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("_wgpu_MAKE_INIT_STRUCT", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("WGPU_STRING_VIEW_INIT", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("WGPU_STRLEN", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("_wgpu_ENUM_ZERO_INIT", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("_wgpu_STRUCT_ZERO_INIT", StringComparison.OrdinalIgnoreCase)
                    || (cppMacro.Name.StartsWith("WGPU_", StringComparison.OrdinalIgnoreCase) && cppMacro.Name.EndsWith("_INIT", StringComparison.OrdinalIgnoreCase))
                )
                {
                    continue;
                }

                //string csName = GetPrettyEnumName(cppMacro.Name, "VK_");

                string modifier = "const";
                string macroValue = NormalizeEnumValue(cppMacro.Value, out string csDataType);
                string assignmentOperator = "=";

                switch (cppMacro.Name)
                {
                    case "WGPU_WHOLE_MAP_SIZE":
                        modifier = "static readonly";
                        csDataType = "nuint";
                        macroValue = "nuint.MaxValue";
                        break;
                    case "WGPU_WHOLE_SIZE":
                    case "WGPU_LIMIT_U64_UNDEFINED":
                        csDataType = "ulong";
                        macroValue = "0xffffffffffffffff";
                        break;
                    case "WGPU_ARRAY_LAYER_COUNT_UNDEFINED":
                    case "WGPU_COPY_STRIDE_UNDEFINED":
                    case "WGPU_DEPTH_SLICE_UNDEFINED":
                    case "WGPU_LIMIT_U32_UNDEFINED":
                    case "WGPU_MIP_LEVEL_COUNT_UNDEFINED":
                    case "WGPU_QUERY_SET_INDEX_UNDEFINED":
                        macroValue = "0xffffffffu";
                        break;

                    case "WGPU_TRUE":
                        macroValue = "1u";
                        break;

                    case "WGPU_FALSE":
                        macroValue = "0u";
                        break;

                    case "WGPU_DEPTH_CLEAR_VALUE_UNDEFINED":
                        csDataType = "float";
                        macroValue = "float.NaN";
                        break;

                    // box3d
                    case "B3_NULL_INDEX":
                        csDataType = "int";
                        break;

                    case "B3_PI":
                    case "B3_DEG_TO_RAD":
                    case "B3_RAD_TO_DEG":
                    case "B3_MIN_SCALE":
                    case "B3_MAX_ROTATION":
                    case "B3_CONTACT_RECYCLE_ANGULAR_DISTANCE":
                    case "B3_TIME_TO_SLEEP":
                    case "B3_PARALLEL_EDGE_TOL":
                    case "B3_AABB_MARGIN_FRACTION":
                        csDataType = "float";
                        break;

                    case "B3_HUGE":
                    case "B3_LINEAR_SLOP":
                    case "B3_MIN_CAPSULE_LENGTH":
                    case "B3_MIN_FRICTION_WEIGHT":
                    case "B3_OVERLAP_SLOP":
                    case "B3_SPECULATIVE_DISTANCE":
                    case "B3_MESH_REST_OFFSET":
                    case "B3_CONTACT_RECYCLE_DISTANCE":
                    case "B3_MAX_AABB_MARGIN":
                        modifier = "static";
                        csDataType = "float";
                        assignmentOperator = "=>";
                        break;

                    case "B3_MAX_SHAPE_CAST_POINTS":
                        csDataType = "uint";
                        macroValue = "B3_MAX_HULL_VERTICES";
                        break;

                    case "B3_DEFAULT_CATEGORY_BITS":
                    case "B3_DEFAULT_MASK_BITS":
                        csDataType = "ulong";
                        macroValue = "ulong.MaxValue";
                        break;

                    case "B3_BOUNDARY_EDGE_ANGLE":
                    case "B3_CHILD_POWER":
                    case "B3_SHAPE_POWER":
                    case "B3_MAX_SHAPES":
                    case "B3_MAX_CHILD_SHAPES":
                        csDataType = "int";
                        break;

                    case "B3_DYNAMIC_TREE_VERSION":
                        csDataType = "ulong";
                        macroValue = "0x1D6F4C2A73B80E91ul";
                        break;

                    case "B3_HULL_VERSION":
                        csDataType = "ulong";
                        macroValue = "0x6B1E39D4A87C25F3ul";
                        break;

                    case "B3_MESH_VERSION":
                        csDataType = "ulong";
                        macroValue = "0x24AA0BF74682501Dul";
                        break;

                    case "B3_HEIGHT_FIELD_VERSION":
                        csDataType = "ulong";
                        macroValue = "0x5A69A297D7ACE1CDul";
                        break;

                    case "B3_COMPOUND_VERSION":
                        csDataType = "ulong";
                        macroValue = "0x9E4B17D3A25C68F1ul^B3_MESH_VERSION^B3_HULL_VERSION";
                        break;
                }

                writer.WriteLine($"/// <unmanaged>{cppMacro.Name}</unmanaged>");
                writer.WriteLine($"public {modifier} {csDataType} {cppMacro.Name} {assignmentOperator} {macroValue};");
                needNewLine = true;
            }

            if (needNewLine)
            {
                writer.WriteLine();
            }

            foreach (string enumConstant in _enumConstants)
            {
                writer.WriteLine($"public const {enumConstant};");
            }
        }
    }
}
