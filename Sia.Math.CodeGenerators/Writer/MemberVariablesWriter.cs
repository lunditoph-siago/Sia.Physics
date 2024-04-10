using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Sia.Math.CodeGenerators.Writer;

public class MemberVariablesWriter(VectorType type) : ITypeSourceWriter
{
    private bool UseMarshal { get; } = type is { Columns: 1, BaseType: "bool" };

    public HashSet<string> Imports => UseMarshal ? ["System.Runtime.InteropServices"] : [];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        if(type.Columns > 1)
        {
            var columnType = type.BaseType.ToTypeName(type.Rows, 1);

            for (var i = 0; i < type.Columns; i++)
                source.WriteLine($"public {columnType} {VectorType.MatrixFields[i]};");
        }
        else
        {
            for (var i = 0; i < type.Rows; i++)
            {
                if (UseMarshal) source.WriteLine("[MarshalAs(UnmanagedType.U1)]");

                source.WriteLine($"public {type.BaseType} {VectorType.VectorFields[i]};");
            }
        }
    };
}