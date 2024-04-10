using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators.Builder;

public class ToStringBuilder(VectorType type) : IBuilder
{
    private readonly TypeOnlyCompositeWriter m_ToStringWriter = new();

    public ICompositeWriter Build()
    {
        m_ToStringWriter.Add(new ToStringWriter(type, false));

        if (type.BaseType != "bool")
        {
            m_ToStringWriter.Add(new ToStringWriter(type, true));
        }

        return m_ToStringWriter;
    }
}