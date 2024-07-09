// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using CppAst;

namespace Generator;

public static class Program
{
    public static void Main(string[] args)
    {
        string outputPath = AppContext.BaseDirectory;
        if (args.Length > 0)
        {
            outputPath = args[0];
        }

        if (!Path.IsPathRooted(outputPath))
        {
            outputPath = Path.Combine(AppContext.BaseDirectory, outputPath);
        }

        if (!outputPath.EndsWith("Generated"))
        {
            outputPath = Path.Combine(outputPath, "Generated");
        }

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string? headerFile = default;
        CppParserOptions? parseOptions = default;
        CsCodeGeneratorOptions? generateOptions = default;

        if (outputPath.Contains("Alimer.Bindings.MeshOptimizer"))
        {
            headerFile = Path.Combine(AppContext.BaseDirectory, "headers", "meshoptimizer.h");
            parseOptions = new()
            {
                ParseMacros = true
            };

            generateOptions = new()
            {
                ClassName = "Meshopt",
                Namespace = "Meshopt",
                PublicVisiblity = true,
                EnumPrefixRemap = "meshopt_",
                StructPrefixRemap = "meshopt_",
                FunctionPrefixRemap = "meshopt_",
                ExcludeConstants =
                {
                    "MESHOPTIMIZER_ALLOC_CALLCONV",
                    "MESHOPTIMIZER_EXPERIMENTAL",
                },
                ExcludeFunctions =
                {
                    "meshopt_setAllocator"
                },
                FunctionParametersRemap =
                {
                    { "meshopt_simplify::options", "SimplificationOptions" },
                    { "meshopt_simplifyWithAttributes::options", "SimplificationOptions" },
                }
            };
        }
        else if (outputPath.Contains("Alimer.Bindings.WebGPU"))
        {
            headerFile = Path.Combine(AppContext.BaseDirectory, "webgpu", "wgpu.h");
            parseOptions = new()
            {
                ParseMacros = true
            };

            generateOptions = new()
            {
                ClassName = "WebGPU",
                Namespace = "WebGPU",
                PublicVisiblity = true,
            };
        }
        else
        {
            Console.WriteLine("No generator configured");
            return;
        }

        CppCompilation compilation = CppParser.ParseFile(headerFile, parseOptions!);

        // Print diagnostic messages
        if (compilation.HasErrors)
        {
            foreach (CppDiagnosticMessage message in compilation.Diagnostics.Messages)
            {
                if (message.Type == CppLogMessageType.Error)
                {
                    var currentColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(message);
                    Console.ForegroundColor = currentColor;
                }
            }

            return;
        }

        generateOptions.OutputPath = outputPath;
        CsCodeGenerator generator = new(generateOptions!);
        generator.Generate(compilation);
    }
}
