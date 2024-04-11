using Microsoft.CodeAnalysis;

namespace Sia.Math.CodeGenerators;

[Generator]
public partial class MathSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        
    }

    public void Execute(GeneratorExecutionContext context)
    {
        for (var rows = 2; rows <= 4; rows++)
        {
            for (var columns = 1; columns <= 4; columns++)
            {
                AddTypeCode(ref context, BaseType.Bool, rows, columns, Features.BitwiseLogic);
                AddTypeCode(ref context, BaseType.Int, rows, columns, Features.All);
                AddTypeCode(ref context, BaseType.UInt, rows, columns, Features.All);
                AddTypeCode(ref context, BaseType.Float, rows, columns, Features.Arithmetic | Features.UnaryNegation);
                AddTypeCode(ref context, BaseType.Double, rows, columns, Features.Arithmetic | Features.UnaryNegation);
            }
        }

        AddMatrixCode(ref context);
    }
}