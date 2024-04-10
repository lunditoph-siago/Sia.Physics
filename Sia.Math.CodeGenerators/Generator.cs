﻿using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Text;

namespace Sia.Math.CodeGenerators;

public static class Generator
{
    public static IndentedTextWriter CreateSource(out StringBuilder builder)
    {
        builder = new StringBuilder();
        var writer = new StringWriter(builder, CultureInfo.InvariantCulture);
        var source = new IndentedTextWriter(writer, "    ");
        return source;
    }

    public static IndentedTextWriter CreateFileSource(out StringBuilder builder)
    {
        var source = CreateSource(out builder);
        source.WriteLine("// <auto-generated/>");
        source.WriteLine("#nullable enable");
        source.WriteLine();
        return source;
    }

    public class EnclosingDisposable(IndentedTextWriter source, int count) : IDisposable
    {
        public void Dispose()
        {
            for (var i = 0; i < count; ++i)
            {
                source.Indent--;
                source.WriteLine("}");
            }
        }
    }

    public static IDisposable GenerateInNamespace(IndentedTextWriter source, string @namespace = "Sia.Math", bool contain = false)
    {
        if (contain)
        {
            source.WriteLine($"namespace {@namespace}");
            source.WriteLine("{");

            source.Indent++;

            return new EnclosingDisposable(source, 1);
        }

        source.WriteLine($"namespace {@namespace};");
        source.WriteLine();

        return new EnclosingDisposable(source, 0);
    }

    public static IDisposable GenerateInTypeStruct(IndentedTextWriter source, VectorType type, string[] inherts)
    {
        if (type.Columns == 1)
            source.WriteLine($"[DebuggerTypeProxy(typeof({type.TypeName}.DebuggerProxy))]");

        source.WriteLine("[System.Serializable]");

        source.WriteLine("public struct {0} : {1}", type.TypeName, string.Join(", ", inherts));

        source.WriteLine("{");

        source.Indent++;

        return new EnclosingDisposable(source, 1);
    }

    public static IDisposable GenerateInMath(IndentedTextWriter source)
    {
        source.WriteLine("public static partial class math");
        source.WriteLine("{");
        source.Indent++;

        return new EnclosingDisposable(source, 1);
    }

    public static IDisposable GenerateInConstructor(IndentedTextWriter source, string type, string @params)
    {
        source.WriteLine($"public {type}({@params})");
        source.WriteLine("{");

        source.Indent++;

        return new EnclosingDisposable(source, 1);
    }
}