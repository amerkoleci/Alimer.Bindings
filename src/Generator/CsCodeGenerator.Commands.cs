// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Text;
using CppAst;

namespace Generator;

public static partial class CsCodeGenerator
{
    private static string GetFunctionPointerSignature(CppFunction function)
    {
        StringBuilder builder = new();
        foreach (CppParameter parameter in function.Parameters)
        {
            string paramCsType = GetCsTypeName(parameter.Type, false);

            //if (CanBeUsedAsOutput(parameter.Type, out CppTypeDeclaration? cppTypeDeclaration))
            //{
            //    builder.Append("out ");
            //    paramCsType = GetCsTypeName(cppTypeDeclaration, false);
            //}

            builder.Append(paramCsType).Append(", ");
        }

        string returnCsName = GetCsTypeName(function.ReturnType, false);
        builder.Append(returnCsName);

        return $"delegate* unmanaged<{builder}>";
    }

    private static void GenerateCommands(CppCompilation compilation, string outputPath)
    {
        // Generate Functions
        using var writer = new CodeWriter(Path.Combine(outputPath, "Commands.cs"),
            false,
            "System"
            );

        Dictionary<string, CppFunction> commands = new();
        foreach (CppFunction? cppFunction in compilation.Functions)
        {
            string? returnType = GetCsTypeName(cppFunction.ReturnType, false);
            string? csName = cppFunction.Name;
            string argumentsString = GetParameterSignature(cppFunction);

            commands.Add(csName, cppFunction);
        }

        using (writer.PushBlock($"unsafe partial class WebGPU"))
        {
            foreach (KeyValuePair<string, CppFunction> command in commands)
            {
                CppFunction cppFunction = command.Value;

                string functionPointerSignature = GetFunctionPointerSignature(cppFunction);
                writer.WriteLine($"private static {functionPointerSignature} {command.Key}_ptr;");
                WriteFunctionInvocation(writer, cppFunction);
            }

            WriteCommands(writer, "GenLoadCommands", commands);
        }
    }

    private static void WriteCommands(CodeWriter writer, string name, Dictionary<string, CppFunction> commands)
    {
        using (writer.PushBlock($"private static void {name}()"))
        {
            foreach (KeyValuePair<string, CppFunction> instanceCommand in commands)
            {
                string commandName = instanceCommand.Key;
                string functionPointerSignature = GetFunctionPointerSignature(instanceCommand.Value);
                writer.WriteLine($"{commandName}_ptr = ({functionPointerSignature}) LoadFunctionPointer(nameof({commandName}));");
            }
        }
    }

    private static void WriteFunctionInvocation(CodeWriter writer, CppFunction cppFunction)
    {
        string returnCsName = GetCsTypeName(cppFunction.ReturnType, false);
        string argumentsString = GetParameterSignature(cppFunction);

        using (writer.PushBlock($"public static {returnCsName} {cppFunction.Name}({argumentsString})"))
        {
            if (returnCsName != "void")
            {
                writer.Write("return ");
            }

            writer.Write($"{cppFunction.Name}_ptr(");

            int index = 0;
            foreach (CppParameter cppParameter in cppFunction.Parameters)
            {
                string paramCsName = GetParameterName(cppParameter.Name);

                //if (CanBeUsedAsOutput(cppParameter.Type, out CppTypeDeclaration? cppTypeDeclaration))
                //{
                //    writer.Write("out ");
                //}

                writer.Write($"{paramCsName}");

                if (index < cppFunction.Parameters.Count - 1)
                {
                    writer.Write(", ");
                }

                index++;
            }

            writer.WriteLine(");");
        }

        writer.WriteLine();
    }


    private static void EmitInvoke(CodeWriter writer, CppFunction function, List<string> parameters, bool handleCheckResult = true)
    {
        var postCall = string.Empty;
        if (handleCheckResult)
        {
            var hasResultReturn = GetCsTypeName(function.ReturnType) == "VkResult";
            if (hasResultReturn)
            {
                postCall = ".CheckResult()";
            }
        }

        int index = 0;
        var callArgumentStringBuilder = new StringBuilder();
        foreach (string? parameterName in parameters)
        {
            callArgumentStringBuilder.Append(parameterName);

            if (index < parameters.Count - 1)
            {
                callArgumentStringBuilder.Append(", ");
            }

            index++;
        }

        string callArgumentString = callArgumentStringBuilder.ToString();
        writer.WriteLine($"{function.Name}_ptr({callArgumentString}){postCall};");
    }

    public static string GetParameterSignature(CppFunction cppFunction)
    {
        return GetParameterSignature(cppFunction.Parameters, cppFunction.Name);
    }

    private static string GetParameterSignature(IList<CppParameter> parameters, string functionName)
    {
        var argumentBuilder = new StringBuilder();
        int index = 0;

        foreach (CppParameter cppParameter in parameters)
        {
            string direction = string.Empty;
            string paramCsTypeName = GetCsTypeName(cppParameter.Type, false);
            string paramCsName = GetParameterName(cppParameter.Name);

            //if (cppParameter.Name.EndsWith("Count"))
            //{
            //    if (functionName.StartsWith("vkEnumerate") ||
            //        functionName.StartsWith("vkGet"))
            //    {
            //        paramCsTypeName = "int*";
            //    }
            //    else
            //    {
            //        paramCsTypeName = "int";
            //    }
            //}

            //if (CanBeUsedAsOutput(cppParameter.Type, out CppTypeDeclaration? cppTypeDeclaration))
            //{
            //    argumentBuilder.Append("out ");
            //    paramCsTypeName = GetCsTypeName(cppTypeDeclaration, false);
            //}

            argumentBuilder.Append(paramCsTypeName).Append(' ').Append(paramCsName);
            if (index < parameters.Count - 1)
            {
                argumentBuilder.Append(", ");
            }
            else
            {
                if (paramCsTypeName == "VkAllocationCallbacks*")
                {
                    argumentBuilder.Append(" = default");
                }
            }

            index++;
        }

        return argumentBuilder.ToString();
    }

    private static string GetParameterName(string name)
    {
        if (name == "event")
            return "@event";

        if (name == "object")
            return "@object";

        if (name.StartsWith('p')
            && char.IsUpper(name[1]))
        {
            name = char.ToLower(name[1]) + name.Substring(2);
            return GetParameterName(name);
        }

        return name;
    }

    private static bool CanBeUsedAsOutput(CppType type, out CppTypeDeclaration? elementTypeDeclaration)
    {
        if (type is CppPointerType pointerType)
        {
            if (pointerType.ElementType is CppTypedef typedef)
            {
                elementTypeDeclaration = typedef;
                return true;
            }
            else if (pointerType.ElementType is CppClass @class
                && @class.ClassKind != CppClassKind.Class
                && @class.SizeOf > 0)
            {
                elementTypeDeclaration = @class;
                return true;
            }
            else if (pointerType.ElementType is CppEnum @enum
                && @enum.SizeOf > 0)
            {
                elementTypeDeclaration = @enum;
                return true;
            }
        }

        elementTypeDeclaration = null;
        return false;
    }
}
