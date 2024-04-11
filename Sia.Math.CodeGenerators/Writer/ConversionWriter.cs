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

public class ConversionWriter(VectorType type, BaseType sourceBaseType, bool isExplicit, bool isScalar) : ICompositeWriter, IOpSourceWriter
{
    private readonly string m_SourceType = isScalar ? sourceBaseType.ToBaseTypeName() : sourceBaseType.ToTypeName(type.Rows, type.Columns);

    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        var fieldType = type.Columns > 1 ? type.BaseType.ToTypeName(type.Rows, 1) : type.BaseType.ToBaseTypeName();
        var fieldCount = type.Columns > 1 ? type.Columns : type.Rows;
        var fields = type.Columns > 1 ? VectorType.MatrixFields : VectorType.VectorFields;
        var typeCategory = type.Columns > 1 ? "matrix" : "vector";

        if (isScalar)
        {
            if(sourceBaseType != type.BaseType)
            {
                source.WriteLine("/// <summary>Constructs a <see cref=\"{0}\" /> {1} from a single <see cref=\"{2}\" /> value by converting it to <see cref=\"{3}\" /> and assigning it to every component.</summary>", type.TypeName, typeCategory, m_SourceType, type.BaseTypeName);
                source.WriteLine("/// <param name=\"v\">The <see cref=\"{0}\" /> to convert to <see cref=\"{1}\" />.</param>", m_SourceType, type.TypeName);
            }
            else
            {
                source.WriteLine("/// <summary>Constructs a <see cref=\"{0}\" /> {1} from a single <see cref=\"{2}\" /> value by assigning it to every component.</summary>", type.TypeName, typeCategory, m_SourceType);
                source.WriteLine("/// <param name=\"v\">The <see cref=\"{0}\" /> to convert to <see cref=\"{1}\" />.</param>", m_SourceType, type.TypeName);
            }
        }
        else
        {
            if (sourceBaseType != type.BaseType)
            {
                source.WriteLine("/// <summary>Constructs a <see cref=\"{0}\" /> {1} from a <see cref=\"{2}\" /> {1} by component-wise conversion.</summary>", type.TypeName, typeCategory, m_SourceType);
                source.WriteLine("/// <param name=\"v\">The <see cref=\"{0}\" /> to convert to <see cref=\"{1}\" />.</param>", m_SourceType, type.TypeName);
            }
        }

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public {0}({1} v)", type.TypeName, m_SourceType);
        source.WriteLine("{");
        source.Indent++;
        {
            for (var i = 0; i < fieldCount; i++)
            {
                var rhs = "v";
                if (!isScalar) rhs = $"{rhs}.{fields[i]}";

                if (isExplicit)
                {
                    if (sourceBaseType is BaseType.Bool)
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
        var typeCategory = type.Columns > 1 ? "matrix" : "vector";

        if (isScalar)
        {
            if(sourceBaseType != type.BaseType)
            {
                source.WriteLine("/// <summary>Returns a <see cref=\"{0}\" /> {1} constructed from a single <see cref=\"{2}\" /> value by converting it to <see cref=\"{3}\" /> and assigning it to every component.</summary>", type.TypeName, typeCategory, m_SourceType, type.BaseTypeName);
                source.WriteLine("/// <param name=\"v\">The <see cref=\"{0}\" /> to convert to <see cref=\"{1}\" />.</param>", m_SourceType, type.TypeName);
                source.WriteLine("/// <returns>The <see cref=\"{0}\" /> constructed from arguments.</returns>", type.TypeName);
            }
            else
            {
                source.WriteLine("/// <summary>Returns a <see cref=\"{0}\" /> {1} constructed from a single <see cref=\"{2}\" /> value by assigning it to every component.</summary>", type.TypeName, typeCategory, m_SourceType);
                source.WriteLine("/// <param name=\"v\">The <see cref=\"{0}\" /> to convert to <see cref=\"{1}\" />.</param>", m_SourceType, type.TypeName);
                source.WriteLine("/// <returns>The <see cref=\"{0}\" /> constructed from arguments.</returns>", type.TypeName);
            }
        }
        else
        {
            if (sourceBaseType != type.BaseType)
            {
                source.WriteLine("/// <summary>Return a <see cref=\"{0}\" /> {1} constructed from a <see cref=\"{2}\" /> {1} by component-wise conversion.</summary>", type.TypeName, typeCategory, m_SourceType);
                source.WriteLine("/// <param name=\"v\">The <see cref=\"{0}\" /> to convert to <see cref=\"{1}\" />.</param>", m_SourceType, type.TypeName);
                source.WriteLine("/// <returns>The <see cref=\"{0}\" /> constructed from arguments.</returns>", type.TypeName);
            }
        }

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} {0}({1} v) => new {0}(v);", type.TypeName, m_SourceType);
    };

    public Action<IndentedTextWriter> OpSourceWriter => source =>
    {
        var typeCategory = type.Columns > 1 ? "matrix" : "vector";
        var plicitlyString = isExplicit ? "Explicitly" : "Implicitly";

        if (isScalar)
        {
            if(sourceBaseType != type.BaseType)
            {
                source.WriteLine("/// <summary>{0} converts a single <see cref=\"{1}\" /> value to a <see cref=\"{2}\" /> {3} by converting it to <see cref=\"{4}\" /> and assigning it to every component.</summary>", plicitlyString, m_SourceType, type.TypeName, typeCategory, type.BaseTypeName);
                source.WriteLine("/// <param name=\"v\">The <see cref=\"{0}\" /> to convert to <see cref=\"{1}\" />.</param>", m_SourceType, type.TypeName);
                source.WriteLine("/// <returns>The <see cref=\"{0}\" /> converted from arguments.</returns>", type.TypeName);
            }
            else
            {
                source.WriteLine("/// <summary>{0} converts a single <see cref=\"{1}\" /> value to a <see cref=\"{2}\" /> {3} by assigning it to every component.</summary>", plicitlyString, m_SourceType, type.TypeName, typeCategory);
                source.WriteLine("/// <param name=\"v\">The <see cref=\"{0}\" /> to convert to <see cref=\"{1}\" />.</param>", m_SourceType, type.TypeName);
                source.WriteLine("/// <returns>The <see cref=\"{0}\" /> converted from arguments.</returns>", type.TypeName);
            }
        }
        else
        {
            if (sourceBaseType != type.BaseType)
            {
                source.WriteLine("/// <summary>{0} converts a <see cref=\"{1}\" /> {2} to a <see cref=\"{3}\" /> {2} by component-wise conversion.</summary>", plicitlyString, m_SourceType, typeCategory, type.TypeName);
                source.WriteLine("/// <param name=\"v\">The <see cref=\"{0}\" /> to convert to <see cref=\"{1}\" />.</param>", m_SourceType, type.TypeName);
                source.WriteLine("/// <returns>The <see cref=\"{0}\" /> converted from arguments.</returns>", type.TypeName);
            }
        }

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} operator {1}({2} v) => new {1}(v);", isExplicit ? "explicit" : "implicit", type.TypeName, m_SourceType);
    };
}