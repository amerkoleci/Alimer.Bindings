// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Text;
using CppAst;

namespace Generator;

partial class CsCodeGenerator
{
    private void GenerateConstants(CppCompilation compilation)
    {
        string visibility = _options.PublicVisiblity ? "public" : "internal";
        using var writer = new CodeWriter(Path.Combine(_options.OutputPath, "Constants.cs"), false, _options.Namespace, Array.Empty<string>());
        using (writer.PushBlock($"{visibility} static partial class {_options.ClassName}"))
        {
            foreach (CppMacro cppMacro in compilation.Macros)
            {
                if (string.IsNullOrEmpty(cppMacro.Value)
                    || _options.ExcludeConstants.Contains(cppMacro.Name)
                    || cppMacro.Name.EndsWith("_H_", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("WGPU_EXPORT", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("WGPU_SHARED_LIBRARY", StringComparison.OrdinalIgnoreCase)
                    || cppMacro.Name.Equals("WGPU_IMPLEMENTATION", StringComparison.OrdinalIgnoreCase)
                    )
                {
                    continue;
                }

                //string csName = GetPrettyEnumName(cppMacro.Name, "VK_");

                string modifier = "const";
                string csDataType = "string";
                string macroValue = NormalizeEnumValue(cppMacro.Value);
                if (macroValue.EndsWith("F", StringComparison.OrdinalIgnoreCase))
                {
                    csDataType = "float";
                }
                else if (macroValue.EndsWith("UL", StringComparison.OrdinalIgnoreCase))
                {
                    csDataType = "ulong";
                }
                else if (macroValue.EndsWith("U", StringComparison.OrdinalIgnoreCase))
                {
                    csDataType = "uint";
                }
                else if (uint.TryParse(macroValue, out _) || macroValue.StartsWith("0x"))
                {
                    csDataType = "uint";
                }

                if (cppMacro.Name == "WGPU_WHOLE_MAP_SIZE")
                {
                    modifier = "static readonly";
                    csDataType = "nuint";
                    macroValue = "nuint.MaxValue";
                }

                writer.WriteLine($"/// <unmanaged>{cppMacro.Name}</unmanaged>");
                writer.WriteLine($"public {modifier} {csDataType} {cppMacro.Name} = {macroValue};");
            }
        }
    }
}
