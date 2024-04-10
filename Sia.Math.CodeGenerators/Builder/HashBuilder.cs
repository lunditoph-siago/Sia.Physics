using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators.Builder;

public class HashBuilder(VectorType type) : IBuilder
{
    private readonly CompositeWriter m_HashWriter = new();

    public ICompositeWriter Build()
    {
        m_HashWriter.Add(new GetHashCodeWriter());

        m_HashWriter.Add(new HashWriter(type, true));
        m_HashWriter.Add(new MathDividerWriter());
        m_HashWriter.Add(new HashWriter(type, false));

        return m_HashWriter;
    }
}