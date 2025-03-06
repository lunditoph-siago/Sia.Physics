using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Sia.Math.CodeGenerators.Writer;

public interface IGlobalSourceWriter
{
    HashSet<string> Imports { get; }

    HashSet<string> Inherits { get; }
}

public interface ITypeSourceWriter : IGlobalSourceWriter
{
    Action<IndentedTextWriter>? TypeSourceWriter { get; }
}

public interface IMathSourceWriter : IGlobalSourceWriter
{
    Action<IndentedTextWriter>? MathSourceWriter { get; }
}

public interface ICompositeWriter: ITypeSourceWriter, IMathSourceWriter;

public class EmptyWriter : ICompositeWriter
{
    public HashSet<string> Imports { get; } = [];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter>? TypeSourceWriter => null;

    public Action<IndentedTextWriter>? MathSourceWriter => null;
}

public class TypeOnlyCompositeWriter : ICompositeWriter
{
    private readonly List<ITypeSourceWriter> m_TypeSourceWriters = [];

    public HashSet<string> Imports { get; } = [];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        for (var i = 0; i < m_TypeSourceWriters.Count; i++)
        {
            var sourceWriter = m_TypeSourceWriters[i];
            if (sourceWriter.TypeSourceWriter != null)
            {
                if (i != 0 && i < m_TypeSourceWriters.Count)
                {
                    source.WriteLineNoTabs(string.Empty);
                }

                sourceWriter.TypeSourceWriter.Invoke(source);
            }
        }
    };

    public Action<IndentedTextWriter>? MathSourceWriter => null;

    public void Add<T>(T writer) where T : class
    {
        if (writer is ITypeSourceWriter typeSourceWriter)
        {
            Imports.UnionWith(typeSourceWriter.Imports);
            Inherits.UnionWith(typeSourceWriter.Inherits);

            m_TypeSourceWriters.Add(typeSourceWriter);
        }
    }
}

public class MathOnlyCompositeWriter : ICompositeWriter
{
    private readonly List<IMathSourceWriter> m_MathSourceWriters = [];

    public HashSet<string> Imports { get; } = [];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter>? TypeSourceWriter => null;

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        for (var i = 0; i < m_MathSourceWriters.Count; i++)
        {
            var sourceWriter = m_MathSourceWriters[i];
            if (sourceWriter.MathSourceWriter != null)
            {
                if (i != 0 && i < m_MathSourceWriters.Count)
                {
                    source.WriteLineNoTabs(string.Empty);
                }

                sourceWriter.MathSourceWriter.Invoke(source);
            }
        }
    };

    public void Add<T>(T writer) where T : class
    {
        if (writer is IMathSourceWriter typeSourceWriter)
        {
            Imports.UnionWith(typeSourceWriter.Imports);
            Inherits.UnionWith(typeSourceWriter.Inherits);

            m_MathSourceWriters.Add(typeSourceWriter);
        }
    }
}

public class CompositeWriter : ICompositeWriter
{
    private readonly List<ITypeSourceWriter> m_TypeSourceWriters = [];
    private readonly List<IMathSourceWriter> m_MathSourceWriters = [];

    public HashSet<string> Imports { get; } = [];

    public HashSet<string> Inherits { get; } = [];

    public virtual Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        for (var i = 0; i < m_TypeSourceWriters.Count; i++)
        {
            var sourceWriter = m_TypeSourceWriters[i];
            if (sourceWriter.TypeSourceWriter != null)
            {
                if (i != 0 && i < m_TypeSourceWriters.Count)
                {
                    source.WriteLineNoTabs(string.Empty);
                }

                sourceWriter.TypeSourceWriter.Invoke(source);
            }
        }
    };

    public virtual Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        for (var i = 0; i < m_MathSourceWriters.Count; i++)
        {
            var sourceWriter = m_MathSourceWriters[i];
            if (sourceWriter.MathSourceWriter != null)
            {
                if (i != 0 && i < m_MathSourceWriters.Count)
                {
                    source.WriteLineNoTabs(string.Empty);
                }

                sourceWriter.MathSourceWriter.Invoke(source);
            }
        }
    };

    public virtual void Add<T>(T writer) where T : class
    {
        if (writer is ITypeSourceWriter typeSourceWriter)
        {
            Imports.UnionWith(typeSourceWriter.Imports);
            Inherits.UnionWith(typeSourceWriter.Inherits);

            m_TypeSourceWriters.Add(typeSourceWriter);
        }

        if (writer is IMathSourceWriter mathSourceWriter)
        {
            Imports.UnionWith(mathSourceWriter.Imports);
            Inherits.UnionWith(mathSourceWriter.Inherits);

            m_MathSourceWriters.Add(mathSourceWriter);
        }
    }
}