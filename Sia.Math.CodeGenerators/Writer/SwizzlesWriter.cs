using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Sia.Math.CodeGenerators.Writer;

public class SwizzlesWriter(VectorType type, int[] swizzle) : ITypeSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];
    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        var bits = 0;
        var allowSetter = true;
        foreach (var sw in swizzle)
        {
            var bit = 1 << sw;
            if ((bits & bit) != 0)
                allowSetter = false;

            bits |= 1 << sw;
        }

        var swizzleOrder = swizzle.Select(s => VectorType.Components[s]).ToArray();
        var fieldType = type.BaseType.ToTypeName(swizzle.Length, 1);

        source.WriteLine("public {0} {1}", fieldType, string.Join(string.Empty, swizzleOrder));
        source.WriteLine("{");
        source.Indent++;
        {
            source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            source.WriteLine("get => new {0}({1});", fieldType, string.Join(", ", swizzleOrder));
            if (allowSetter)
            {
                source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
                source.WriteLine("set {");
                source.Indent++;
                {
                    for (var i = 0; i < swizzle.Length; i++)
                    {
                        var value = swizzle.Length != 1 ? $"value.{VectorType.Components[i]}" : "value";
                        source.WriteLine("{0} = {1};", VectorType.Components[swizzle[i]], value);
                    }
                }
                source.Indent--;
                source.WriteLine("}");
            }
        }
        source.Indent--;
        source.WriteLine("}");
    };
}