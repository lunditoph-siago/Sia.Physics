using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Sia.Math.CodeGenerators;

[Generator]
public partial class MathSourceGenerator : ISourceGenerator
{
    private readonly List<Task<(string fileName, string sourceCode)>> m_Tasks = [];

    public void Initialize(GeneratorInitializationContext context)
    {
        for (var rows = 2; rows <= 4; rows++)
        {
            for (var columns = 1; columns <= 4; columns++)
            {
                var localRows = rows;
                var localColumns = columns;

                m_Tasks.Add(Task.Run(() => AddTypeCode(BaseType.Bool, localRows, localColumns, Features.BitwiseLogic)));
                m_Tasks.Add(Task.Run(() => AddTypeCode(BaseType.Int, localRows, localColumns, Features.All)));
                m_Tasks.Add(Task.Run(() => AddTypeCode(BaseType.UInt, localRows, localColumns, Features.All)));
                m_Tasks.Add(Task.Run(() => AddTypeCode(BaseType.Float, localRows, localColumns, Features.Arithmetic | Features.UnaryNegation)));
                m_Tasks.Add(Task.Run(() => AddTypeCode(BaseType.Double, localRows, localColumns, Features.Arithmetic | Features.UnaryNegation)));
            }
        }
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var taskResults = Task.WhenAll(m_Tasks).Result;

        foreach (var (fileName, sourceCode) in taskResults)
        {
            context.AddSource(fileName, sourceCode);
        }

        AddMathCode(ref context);
        AddMatrixCode(ref context);
    }
}