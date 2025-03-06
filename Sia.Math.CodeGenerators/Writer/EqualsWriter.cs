using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sia.Math.CodeGenerators.Writer;

public class EqualsWriter(VectorType type) : ITypeSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Diagnostics.CodeAnalysis", "System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [$"System.IEquatable<{type.TypeName}>"];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        var fields = type.Columns > 1 ? VectorType.MatrixFields : VectorType.VectorFields;
        var resultCount = type.Columns > 1 ? type.Columns : type.Rows;
        var resultType = type.Columns > 1 ? ("matrix", "matrices") : ("vector", "vectors");

        source.WriteLine("/// <summary>Returns a value that indicates whether this <see cref=\"{0}\" /> instance and another <see cref=\"{0}\" /> {1} are equal.</summary>", type.TypeName, resultType.Item1);
        source.WriteLine("/// <param name=\"other\">The other <see cref=\"{0}\" /> {1}.</param>", type.TypeName, resultType.Item1);
        source.WriteLine("/// <returns><see langword=\"true\" /> if the two <see cref=\"{0}\" /> {1} are equal; otherwise, <see langword=\"false\" />.</returns>", type.TypeName, resultType.Item2);
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public readonly bool Equals({0} other)", type.TypeName);
        source.WriteLine("{");
        source.Indent++;
        {
            var compareConditions = Enumerable.Range(0, resultCount)
                .Select(i => type.Columns == 1 
                    ? $"{fields[i]} == other.{fields[i]}" 
                    : $"{fields[i]}.Equals(other.{fields[i]})");

            source.WriteLine("return {0};", string.Join(" && ", compareConditions));
        }
        source.Indent--;
        source.WriteLine("}");

        source.WriteLineNoTabs(string.Empty);

        source.WriteLine("/// <summary>Returns a value that indicates whether this <see cref=\"{0}\" /> instance and a specified object are equal.</summary>", type.TypeName);
        source.WriteLine("/// <param name=\"obj\">The object to compare with this <see cref=\"{0}\" /> instance.</param>", type.TypeName);
        source.WriteLine("/// <returns><see langword=\"true\" /> if this <see cref=\"{0}\" /> and <paramref name=\"obj\" /> are equal; otherwise, <see langword=\"false\" />. If <paramref name=\"obj\" /> is <see langword=\"null\" />, the method returns <see langword=\"false\" />.</returns>", type.TypeName);
        source.WriteLine("public override readonly bool Equals([NotNullWhen(true)] object? obj)");
        source.WriteLine("{");
        source.Indent++;
        {
            source.WriteLine("return (obj is {0} other) && Equals(other);", type.TypeName);
        }
        source.Indent--;
        source.WriteLine("}");
    };
}