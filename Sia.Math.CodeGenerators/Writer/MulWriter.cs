using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Sia.Math.CodeGenerators.Writer;

public enum GenerateMulType
{
    Mul,
    Rotate,
    Transform
}

public class MulWriter(BaseType type, (int Rows, int Columns) lhs, (int Rows, int Columns) rhs,
    GenerateMulType mulType, bool doTranslation) : IMathSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];
    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        var resultRows = lhs.Columns != rhs.Rows ? rhs.Rows : lhs.Rows;
        var resultType = type.ToTypeName(resultRows, rhs.Columns);
        var lhsType = type.ToTypeName(lhs.Rows, lhs.Columns);
        var rhsType = type.ToTypeName(rhs.Rows, rhs.Columns);

        var needsSwizzle = resultRows != lhs.Rows;
        var name = mulType switch
        {
            GenerateMulType.Mul => "mul",
            GenerateMulType.Rotate => "rotate",
            GenerateMulType.Transform => "transform",
            _ => throw new ArgumentOutOfRangeException(nameof(mulType), mulType, null)
        };

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {1} {0}({2} a, {3} b)", name, resultType, lhsType, rhsType);
        source.WriteLine("{");
        source.Indent++;
        {
            var bodyStr = string.Join(", ", Enumerable.Range(0, rhs.Columns)
                .Select(col =>
                {
                    var terms = Enumerable.Range(0, lhs.Columns)
                        .Where(row => row < rhs.Rows || doTranslation)
                        .Select(row =>
                        {
                            var lhsElement = lhs switch
                            {
                                { Rows: 1, Columns: 1 } => "a",
                                { Rows: 1 } or { Columns: 1 } => $"a.{VectorType.VectorFields[row]}",
                                _ => $"a.{VectorType.MatrixFields[row]}"
                            };

                            var rhsElement = rhs switch
                            {
                                { Rows: 1, Columns: 1 } => "b",
                                { Rows: 1 } or { Columns: 1 } => $"b.{VectorType.VectorFields[row]}",
                                _ => $"b.{VectorType.MatrixFields[col]}.{VectorType.VectorFields[row]}"
                            };

                            return col < rhs.Rows ? $"{lhsElement} * {rhsElement}" : lhsElement;
                        });

                    return string.Join(" + ", terms);
                }));

            var needsParen = rhs.Columns > 1 || needsSwizzle;

            if (needsParen)
                bodyStr = $"new {resultType}({bodyStr})";
            if (needsSwizzle)
                bodyStr = $"{bodyStr}.{string.Join(string.Empty, Enumerable.Range(0, resultRows)
                    .Select(i => VectorType.Components[i]))}";

            source.WriteLine("return {0};", bodyStr);
        }
        source.Indent--;
        source.WriteLine("}");
    };
}