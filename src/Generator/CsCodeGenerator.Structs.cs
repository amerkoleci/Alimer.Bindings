// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using CppAst;

namespace Generator;

partial class CsCodeGenerator
{
    private void GenerateStructAndUnions(CppCompilation compilation)
    {
        string visibility = _options.PublicVisiblity ? "public" : "internal";

        // Generate Structures
        using var writer = new CodeWriter(Path.Combine(_options.OutputPath, "Structs.cs"),
            false,
            _options.Namespace,
            [
                "System.Runtime.InteropServices",
                "System.Runtime.CompilerServices",
                "System.Diagnostics.CodeAnalysis"
            ],
            "#pragma warning disable CS0649"
            );

        // Print All classes, structs
        foreach (CppClass? cppClass in compilation.Classes)
        {
            if (cppClass.ClassKind == CppClassKind.Class ||
                cppClass.SizeOf == 0 ||
                cppClass.Name.EndsWith("_T"))
            {
                continue;
            }

            // Handled manually.
            if (cppClass.Name == "VkClearColorValue"
                || cppClass.Name == "VkTransformMatrixKHR"
                || cppClass.Name == "VkAccelerationStructureInstanceKHR"
                || cppClass.Name == "VkAccelerationStructureSRTMotionInstanceNV"
                || cppClass.Name == "VkAccelerationStructureMatrixMotionInstanceNV"
                )
            {
                continue;
            }

            string structName = cppClass.Name;
            bool isUnion = cppClass.ClassKind == CppClassKind.Union;
            if (isUnion)
            {
                writer.WriteLine("[StructLayout(LayoutKind.Explicit)]");
            }

            bool isReadOnly = false;
            string modifier = "partial";

            if (!string.IsNullOrEmpty(_options.StructPrefixRemap)
                && structName.StartsWith(_options.StructPrefixRemap))
            {
                structName = structName.Replace(_options.StructPrefixRemap, string.Empty);
                structName = PrettyString(structName);
                AddCsMapping(cppClass.Name, structName);
            }

            using (writer.PushBlock($"{visibility} {modifier} struct {structName}"))
            {
                if (_options.GenerateSizeOfStructs && cppClass.SizeOf > 0)
                {
                    writer.WriteLine("/// <summary>");
                    writer.WriteLine($"/// The size of the <see cref=\"{structName}\"/> type, in bytes.");
                    writer.WriteLine("/// </summary>");
                    writer.WriteLine($"public static readonly int SizeInBytes = {cppClass.SizeOf};");
                    writer.WriteLine();
                }

                foreach (CppField cppField in cppClass.Fields)
                {
                    WriteField(writer, cppField, isUnion, isReadOnly);
                }
            }

            writer.WriteLine();
        }
    }

    private void WriteField(CodeWriter writer, CppField field, bool isUnion = false, bool isReadOnly = false)
    {
        string csFieldName = NormalizeFieldName(field.Name);

        if (isUnion)
        {
            writer.WriteLine("[FieldOffset(0)]");
        }

        if (field.Type is CppArrayType arrayType)
        {
            bool canUseFixed = false;
            if (arrayType.ElementType is CppPrimitiveType)
            {
                canUseFixed = true;
            }
            else if (arrayType.ElementType is CppTypedef typedef
                && typedef.ElementType is CppPrimitiveType)
            {
                canUseFixed = true;
            }

            if (canUseFixed)
            {
                string csFieldType = GetCsTypeName(arrayType.ElementType);
                writer.WriteLine($"public unsafe fixed {csFieldType} {csFieldName}[{arrayType.Size}];");
            }
            else
            {
                string csFieldType;
                if (arrayType.ElementType is CppArrayType elementArrayType)
                {
                    // vk-video madness
                    csFieldType = GetCsTypeName(elementArrayType.ElementType);
                    writer.WriteLine($"public unsafe fixed {csFieldType} {csFieldName}[{arrayType.Size} * {elementArrayType.Size}];");
                }
                else
                {
                    csFieldType = GetCsTypeName(arrayType.ElementType);

                    writer.WriteLine($"public {csFieldName}__FixedBuffer {csFieldName};");
                    writer.WriteLine();

                    using (writer.PushBlock($"public unsafe struct {csFieldName}__FixedBuffer"))
                    {
                        for (int i = 0; i < arrayType.Size; i++)
                        {
                            writer.WriteLine($"public {csFieldType} e{i};");
                        }
                        writer.WriteLine();

                        writer.WriteLine("[UnscopedRef]");
                        using (writer.PushBlock($"public ref {csFieldType} this[int index]"))
                        {
                            writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
                            using (writer.PushBlock("get"))
                            {
                                if (csFieldType.EndsWith('*'))
                                {
                                    using (writer.PushBlock($"fixed ({csFieldType}* pThis = &e0)"))
                                    {
                                        writer.WriteLine($"return ref pThis[index];");
                                    }
                                }
                                else
                                {
                                    writer.WriteLine($"return ref AsSpan()[index];");
                                }
                            }
                        }
                        writer.WriteLine();

                        if (!csFieldType.EndsWith('*'))
                        {
                            writer.WriteLine("[UnscopedRef]");
                            writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
                            using (writer.PushBlock($"public Span<{csFieldType}> AsSpan()"))
                            {
                                writer.WriteLine($"return MemoryMarshal.CreateSpan(ref e0, {arrayType.Size});");
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // VkAllocationCallbacks members
            string csFieldType = string.Empty;
            if (field.Type is CppTypedef typedef &&
                typedef.ElementType is CppPointerType pointerType &&
                pointerType.ElementType is CppFunctionType functionType)
            {
                csFieldType = GetCallbackMemberSignature(functionType);
                writer.WriteLine($"public unsafe {csFieldType} {csFieldName};");
                return;
            }

            csFieldType = GetCsTypeName(field.Type);
            if (csFieldName.Equals("specVersion", StringComparison.OrdinalIgnoreCase) ||
                csFieldName.Equals("applicationVersion", StringComparison.OrdinalIgnoreCase) ||
                csFieldName.Equals("engineVersion", StringComparison.OrdinalIgnoreCase) ||
                csFieldName.Equals("apiVersion", StringComparison.OrdinalIgnoreCase))
            {
                csFieldType = "VkVersion";
            }

            if (field.Type.ToString() == "ANativeWindow*")
            {
                csFieldType = "IntPtr";
            }
            else if (field.Type.ToString() == "CAMetalLayer*"
                || field.Type.ToString() == "const CAMetalLayer*")
            {
                csFieldType = "IntPtr";
            }

            string fieldPrefix = isReadOnly ? "readonly " : string.Empty;
            if (csFieldType.EndsWith('*'))
            {
                fieldPrefix += "unsafe ";
            }

            //if (field.Comment is not null && string.IsNullOrEmpty(field.Comment.ToString()) == false)
            //{
            //    writer.WriteLine($"/// <summary>{field.Comment}</summary>");
            //}

            writer.WriteLine($"public {fieldPrefix}{csFieldType} {csFieldName};");
        }
    }
}
