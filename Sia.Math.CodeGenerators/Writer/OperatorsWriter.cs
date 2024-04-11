using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace Sia.Math.CodeGenerators.Writer;

public enum IndexerMode
{
    ByValue,
    ByRef
}

public class BinaryOperatorWriter(VectorType type, (int Rows, int Columns) lhs, (int Rows, int Columns) rhs,
    string op, string opDesc, BaseType resultType, (int Rows, int Columns) result) : ITypeSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Numerics", "System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        var fields = result.Columns > 1 ? VectorType.MatrixFields : VectorType.VectorFields;
        var resultCount = result.Columns > 1 ? result.Columns : result.Rows;

        var bodyStr = new StringBuilder();
        {
            for (var i = 0; i < resultCount; i++)
            {
                if (lhs.Rows == 1)
                {
                    var rhsIndex = i;
                    bodyStr.Append($"lhs {op} rhs.{fields[rhsIndex]}");
                    if (i != resultCount - 1) bodyStr.Append(", ");
                }
                else if (rhs.Rows == 1)
                {
                    var lhsIndex = i;
                    bodyStr.Append($"lhs.{fields[lhsIndex]} {op} rhs");
                    if (i != resultCount - 1) bodyStr.Append(", ");
                }
                else
                {
                    var lhsIndex = i;
                    var rhsIndex = i;

                    bodyStr.Append($"lhs.{fields[lhsIndex]} {op} rhs.{fields[rhsIndex]}");
                    if (i != resultCount - 1) bodyStr.Append(", ");
                }
            }
        }

        source.WriteLine("/// {0}", opDesc);
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} operator {1} ({2} lhs, {3} rhs) => new {0}({4});",
            resultType.ToTypeName(result.Rows, result.Columns), op, type.BaseType.ToTypeName(lhs.Rows, lhs.Columns),
            type.BaseType.ToTypeName(rhs.Rows, rhs.Columns), bodyStr);
    };
}

public class IndexOperatorWriter(VectorType type, IndexerMode mode) : ITypeSourceWriter
{
    public HashSet<string> Imports { get; } = [];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        var resultCount = type.Columns > 1 ? type.Columns : type.Rows;
        var returnType = type.BaseType.ToTypeName(type.Columns > 1 ? type.Rows : 1, 1);

        var refPrefix = mode == IndexerMode.ByRef ? "ref " : "";

        source.WriteLine("/// <summary>{0} at the specified index.</summary>", mode == IndexerMode.ByRef ? "Gets the reference of element": "Gets or Sets the element");
        source.WriteLine("/// <param name=\"index\">The index of the element to {0}.</param>", mode == IndexerMode.ByRef ? "get" : "get or set");
        source.WriteLine("/// <returns>The <see cref=\"{0}\" /> element at <paramref name=\"index\" />.</returns>", returnType);
        source.WriteLine("/// <exception cref=\"ArgumentOutOfRangeException\"><paramref name=\"index\" /> was less than zero or greater than the number of elements.</exception>");
        source.WriteLine("public unsafe {1}{0} this[int index]", returnType, refPrefix);
        source.WriteLine("{");
        source.Indent++;
        {
            source.WriteLine("get");
            source.WriteLine("{");
            source.Indent++;
            {
                source.WriteLineNoTabs("#if DEBUG");
                source.WriteLine("if ((uint)index >= {0})", resultCount);
                source.WriteLine("    throw new System.ArgumentException(\"index must be between[0...{0}]\");", resultCount - 1);
                source.WriteLineNoTabs("#endif");

                var bodyStr = $"return {refPrefix}(({returnType}*)array)[index];";
                source.WriteLine($"fixed ({type.TypeName}* array = &this) {{ {bodyStr} }}");
            }
            source.Indent--;
            source.WriteLine("}");

            if (mode == IndexerMode.ByValue)
            {
                source.WriteLine("set");
                source.WriteLine("{");
                source.Indent++;
                {
                    source.WriteLineNoTabs("#if DEBUG");
                    source.WriteLine("if ((uint)index >= {0})", resultCount);
                    source.WriteLine("    throw new System.ArgumentException(\"index must be between[0...{0}]\");", resultCount - 1);
                    source.WriteLineNoTabs("#endif");

                    var member = type.Columns > 1 ? "c0" : "x";
                    var bodyStr = "array[index] = value;";
                    source.WriteLine($"fixed ({returnType}* array = &{member}) {{ {bodyStr} }}");
                }
                source.Indent--;
                source.WriteLine("}");
            }
        }
        source.Indent--;
        source.WriteLine("}");
    };
}

public class ShiftOperatorWriter(VectorType type, string op, string opDesc) : ITypeSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        var fields = type.Columns > 1 ? VectorType.MatrixFields : VectorType.VectorFields;
        var resultCount = type.Columns > 1 ? type.Columns : type.Rows;

        var bodyStr = new StringBuilder();
        {
            for (var i = 0; i < resultCount; i++)
            {
                if (type.Rows == 1)
                {
                    bodyStr.Append($"x {op} n");
                    if (i != resultCount - 1) bodyStr.Append(", ");
                }
                else
                {
                    var lhsIndex = i;
                    bodyStr.Append($"x.{fields[lhsIndex]} {op} n");
                    if (i != resultCount - 1) bodyStr.Append(", ");
                }
            }
        }

        source.WriteLine("/// {0}", opDesc);
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} operator {1} ({0} x, int n) => new {0}({2});", type.TypeName, op, bodyStr);
    };
}

public class UnaryOperatorWriter(VectorType type, string op, string opDesc) : ITypeSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        var fields = type.Columns > 1 ? VectorType.MatrixFields : VectorType.VectorFields;
        var resultCount = type.Columns > 1 ? type.Columns : type.Rows;

        var bodyStr = new StringBuilder();
        {
            for (var i = 0; i < resultCount; i++)
            {
                if (op == "-" && type is { BaseType: BaseType.UInt, Columns: 1 })
                    bodyStr.Append($"(uint){op}val.{fields[i]}");
                else
                    bodyStr.Append($"{op}val.{fields[i]}");

                if (i != resultCount - 1)
                    bodyStr.Append(", ");
            }
        }

        source.WriteLine("/// {0}", opDesc);
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} operator {1} ({0} val) => new {0}({2});", type.TypeName, op, bodyStr);
    };
}