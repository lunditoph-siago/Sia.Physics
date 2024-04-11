using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Sia.Math.CodeGenerators.Writer;

public class MemberVariablesWriter(VectorType type) : ITypeSourceWriter
{
    private readonly bool m_UseMarshal = type is { Columns: 1, BaseType: BaseType.Bool };

    public HashSet<string> Imports => m_UseMarshal ? ["System.Runtime.InteropServices"] : [];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        if(type.Columns > 1)
        {
            var columnType = type.BaseType.ToTypeName(type.Rows, 1);

            var columnDeclarations = Enumerable.Range(0, type.Columns)
                .Select(i => $"public {columnType} {VectorType.MatrixFields[i]};");

            foreach (var declaration in columnDeclarations) source.WriteLine(declaration);
        }
        else
        {
            var rowDeclarations = Enumerable.Range(0, type.Rows)
                .SelectMany<int, string>(i => m_UseMarshal
                    ? ["[MarshalAs(UnmanagedType.U1)]", $"public {type.BaseTypeName} {VectorType.VectorFields[i]};"]
                    : [$"public {type.BaseTypeName} {VectorType.VectorFields[i]};"]);

            foreach (var declaration in rowDeclarations) source.WriteLine(declaration);
        }
    };
}