using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators.Builder;

public class DebuggerTypeProxyBuilder(VectorType type): IBuilder
{
    private readonly TypeOnlyCompositeWriter m_TypeProxyWriter = new();

    public ICompositeWriter Build()
    {
        if (type.Columns > 1) return new EmptyWriter();

        m_TypeProxyWriter.Add(new DebuggerTypeProxyWriter(type));

        return m_TypeProxyWriter;
    }
}