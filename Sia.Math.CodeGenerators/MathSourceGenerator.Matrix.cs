using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;
using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators;

public partial class MathSourceGenerator
{
    private void AddMatrixCode(ref GeneratorExecutionContext context)
    {
        var compositeWriter = new CompositeWriter();

        var source = Generator.CreateFileSource(out var builder);

        source.WriteLine("using System;");
        source.WriteLine("using System.Runtime.CompilerServices;");
        source.WriteLine();
        source.WriteLine("#pragma warning disable 0660, 0661, 8981");
        source.WriteLine();

        using (Generator.GenerateInNamespace(source))
        {
            using (Generator.GenerateInMath(source))
            {
                compositeWriter.MathSourceWriter.Invoke(source);
            }
        }

        context.AddSource("matrix.g.cs", builder.ToString());    
    }
}