using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Sia.Math.CodeGenerators.Writer;

public interface IOpSourceWriter
{
    Action<IndentedTextWriter>? OpSourceWriter { get; }
}

public class ConversionCompositeWriter : CompositeWriter
{
    private readonly List<IOpSourceWriter> m_OpSourceWriters = [];

    public override Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        var opSource = Generator.CreateSource(out var opSourceBuilder);
        opSource.Indent = source.Indent;

        base.TypeSourceWriter(source);

        foreach (var sourceWriter in m_OpSourceWriters)
        {
            if (sourceWriter.OpSourceWriter != null)
            {
                source.WriteLineNoTabs(string.Empty);

                sourceWriter.OpSourceWriter.Invoke(source);
            }
        }

        source.WriteLine(opSourceBuilder);
    };

    public override void Add<T>(T writer) where T : class
    {
        base.Add(writer);

        if (writer is IOpSourceWriter opSourceWriter)
        {
            m_OpSourceWriters.Add(opSourceWriter);
        }
    }
}

public class ConversionWriter(VectorType type, string sourceBaseType, bool isExplicit, bool isScalar) : ICompositeWriter, IOpSourceWriter
{
    private string SourceType { get; } = isScalar ? sourceBaseType : sourceBaseType.ToTypeName(type.Rows, type.Columns);

    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        var fieldType = type.Columns > 1 ? type.BaseType.ToTypeName(type.Rows, 1) : type.BaseType;
        var fieldCount = type.Columns > 1 ? type.Columns : type.Rows;
        var fields = type.Columns > 1 ? VectorType.MatrixFields : VectorType.VectorFields;

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public {0}({1} v)", type.TypeName, SourceType);
        source.WriteLine("{");
        source.Indent++;
        {
            for (var i = 0; i < fieldCount; i++)
            {
                var rhs = "v";
                if (!isScalar) rhs = $"{rhs}.{fields[i]}";

                if (isExplicit)
                {
                    if (sourceBaseType == "bool")
                        rhs = type.Columns > 1
                            ? $"math.select(new {fieldType}({type.BaseType.ToTypedLiteral(0)}), new {fieldType}({type.BaseType.ToTypedLiteral(1)}), {rhs})"
                            : $"{rhs} ? {type.BaseType.ToTypedLiteral(1)} : {type.BaseType.ToTypedLiteral(0)}";
                    else
                        rhs = $"({fieldType}){rhs}";
                }


                source.WriteLine("this.{0} = {1};", fields[i], rhs);
            }
        }
        source.Indent--;
        source.WriteLine("}");
    };

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} {0}({1} v) => new {0}(v);", type.TypeName, SourceType);
    };

    public Action<IndentedTextWriter> OpSourceWriter => source =>
    {
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} operator {1}({2} v) => new {1}(v);",
            isExplicit ? "explicit" : "implicit", type.TypeName, SourceType);
    };
}