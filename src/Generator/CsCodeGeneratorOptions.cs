// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Generator;

public sealed class CsCodeGeneratorOptions
{
    public string OutputPath { get; set; } = null!;
    public string ClassName { get; set; } = null!;
    public string? Namespace { get; set; }
    public bool PublicVisiblity { get; set; } = true;
    public bool GenerateSizeOfStructs { get; set; }

    /// <summary>
    /// List of the excluded constants.
    /// </summary>
    public HashSet<string> ExcludeConstants { get; private set; } = [];

    /// <summary>
    /// List of the excluded functions.
    /// </summary>
    public HashSet<string> ExcludeFunctions { get; private set; } = [];


    /// <summary>
    /// List of the excluded structures.
    /// </summary>
    public HashSet<string> ExcludeStructs { get; private set; } = [];

    public HashSet<string> OutReturnFunctions { get; private set; } = [];
    public Dictionary<string, string> TypeNameMappings { get; private set; } = [];

    public string? EnumPrefixRemap { get; set; }
    public string? StructPrefixRemap { get; set; }
    public string? FunctionPrefixRemap { get; set; }
    public bool SkipEnumItemRemap { get; set; }

    public Dictionary<string, string> FunctionParametersRemap { get; set; } = [];
}
