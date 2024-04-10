using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace Sia.Math.CodeGenerators.Writer;

public class EqualsWriter(VectorType type) : ITypeSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [$"System.IEquatable<{type.TypeName}>"];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        var fields = type.Columns > 1 ? VectorType.MatrixFields : VectorType.VectorFields;
        var resultCount = type.Columns > 1 ? type.Columns : type.Rows;

        var compareStr = new StringBuilder();
        {
            for (var i = 0; i < resultCount; i++)
            {
                compareStr.Append(type.Columns == 1 ? $"{fields[i]} == rhs.{fields[i]}" : $"{fields[i]}.Equals(rhs.{fields[i]})");

                if (i != resultCount - 1) compareStr.Append(" && ");
            }
        }

        source.WriteLine("/// <summary>Returns true if the {0} is equal to a given {0}, false otherwise.</summary>", type.TypeName);
        source.WriteLine("/// <param name=\"rhs\">Right hand side argument to compare equality with.</param>");
        source.WriteLine("/// <returns>The result of the equality comparison.</returns>");
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public bool Equals({0} rhs) => {1};", type.TypeName, compareStr);

        source.WriteLineNoTabs(string.Empty);

        source.WriteLine("public override bool Equals(object? o) => o is {0} converted && Equals(converted);", type.TypeName);
    };
}