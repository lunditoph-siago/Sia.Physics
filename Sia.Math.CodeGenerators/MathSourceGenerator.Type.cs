using System.Linq;
using Microsoft.CodeAnalysis;
using Sia.Math.CodeGenerators.Builder;
using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators;

public partial class MathSourceGenerator
{
    private static void AddTypeCode(ref GeneratorExecutionContext context, BaseType baseType, int rows, int columns, Features operations)
    {
        var vectorType = new VectorType(baseType, rows, columns, operations);
        var compositeWriter = new CompositeWriter();

        compositeWriter.Add(new MemberVariablesWriter(vectorType));
        compositeWriter.Add(new ConstructorsBuilder(vectorType).Build());
        compositeWriter.Add(new ConversionBuilder(vectorType).Build());
        compositeWriter.Add(new OperatorsBuilder(vectorType).Build());
        compositeWriter.Add(new EqualsWriter(vectorType));
        compositeWriter.Add(new HashBuilder(vectorType).Build());
        compositeWriter.Add(new ToStringWriter(vectorType));
        compositeWriter.Add(new DebuggerTypeProxyBuilder(vectorType).Build());

        var typeSource = Generator.CreateFileSource(out var sourceBuilder);

        typeSource.WriteLine("using System;");

        foreach (var import in compositeWriter.Imports)
            typeSource.WriteLine("using {0};", import);

        typeSource.WriteLine();
        typeSource.WriteLine("#pragma warning disable 0660, 0661, 8981");
        typeSource.WriteLine();

        using (Generator.GenerateInNamespace(typeSource))
        {
            var mathSource = Generator.CreateSource(out var mathSourceBuilder);

            using (Generator.GenerateInTypeStruct(typeSource, vectorType, compositeWriter.Inherits.ToArray()))
            {
                compositeWriter.TypeSourceWriter.Invoke(typeSource);
            }

            typeSource.WriteLineNoTabs(string.Empty);

            using (Generator.GenerateInMath(typeSource))
            {
                mathSource.Indent = typeSource.Indent;

                compositeWriter.MathSourceWriter.Invoke(mathSource);

                typeSource.Write(mathSourceBuilder);
            }
        }

        context.AddSource($"{vectorType.TypeName}.g.cs", sourceBuilder.ToString());        
    }
}