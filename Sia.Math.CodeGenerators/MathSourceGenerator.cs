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
                AddTypeCode(ref context, "bool", rows, columns, Features.BitwiseLogic);
                AddTypeCode(ref context, "int", rows, columns, Features.All);
                AddTypeCode(ref context, "uint", rows, columns, Features.All);
                AddTypeCode(ref context, "float", rows, columns, Features.Arithmetic | Features.UnaryNegation);
                AddTypeCode(ref context, "double", rows, columns, Features.Arithmetic | Features.UnaryNegation);
            }
        }

        AddMatrixCode(ref context);
    }
}