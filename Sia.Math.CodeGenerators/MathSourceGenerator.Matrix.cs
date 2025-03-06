using Microsoft.CodeAnalysis;
using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators;

public partial class MathSourceGenerator
{
    private static void AddMatrixCode(ref GeneratorExecutionContext context)
    {
        var source = Generator.CreateFileSource(out var builder);
        var mathWriter = new MathOnlyCompositeWriter();
        GenerateMulImplementations(mathWriter, BaseType.Int);
        GenerateMulImplementations(mathWriter, BaseType.UInt);
        GenerateMulImplementations(mathWriter, BaseType.Float);
        GenerateMulImplementations(mathWriter, BaseType.Double);

        source.WriteLine("using System;");

        foreach (var import in mathWriter.Imports)
            source.WriteLine("using {0};", import);

        source.WriteLine();
        source.WriteLine("#pragma warning disable 0660, 0661, 8981");
        source.WriteLine();

        using (Generator.GenerateInNamespace(source))
        {
            using (Generator.GenerateInMath(source))
            {
                mathWriter.MathSourceWriter.Invoke(source);
            }
        }

        context.AddSource("matrix.g.cs", builder.ToString());    
    }

    private static void GenerateMulImplementations(MathOnlyCompositeWriter writer, BaseType type)
    {
        // typenxk = mul(typenxm, typemxk)
        for (var n = 1; n <= 4; n++)
        {
            for (var m = 1; m <= 4; m++)
            {
                for (var k = 1; k <= 4; k++)
                {
                    // mul(a,b): if a is vector it is treated as a row vector. if b is a vector it is treaded as a column vector.

                    if (n > 1 && m == 1)
                        continue; // lhs cannot be column vector
                    if (m == 1 && k > 1)
                        continue; // rhs cannot be row vector

                    writer.Add(new MulWriter(type, (n, m), (m, k), GenerateMulType.Mul, false));
                }
            }
        }
    }
}