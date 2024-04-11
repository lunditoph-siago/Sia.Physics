using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Sia.Math.CodeGenerators.Writer;

public class DebuggerTypeProxyWriter(VectorType type) : ITypeSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Diagnostics"];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        if (type.Columns > 1) return;

        source.WriteLine("internal sealed class DebuggerProxy");
        source.WriteLine("{");

        source.Indent++;
        {
            for (var i = 0; i < type.Rows; i++)
                source.WriteLine($"public {type.BaseTypeName} {VectorType.VectorFields[i]};");

            source.WriteLine($"public DebuggerProxy({type.TypeName} v)");
            source.WriteLine("{");

            source.Indent++;
            {
                for (var i = 0; i < type.Rows; i++)
                    source.WriteLine($"{VectorType.VectorFields[i]} = v.{VectorType.VectorFields[i]};");
            }
            source.Indent--;

            source.WriteLine("}");
        }
        source.Indent--;

        source.WriteLine("}");
    };
}