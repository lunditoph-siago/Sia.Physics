using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators.Builder;

public class ShuffleBuilder(VectorType type) : IBuilder
{
    private readonly MathOnlyCompositeWriter m_ShuffleWriter = new();

    public ICompositeWriter Build()
    {
        if (type.Columns == 1)
        {
            m_ShuffleWriter.Add(new SelectShuffleWriter(type));
            m_ShuffleWriter.Add(new ShuffleWriter(type));
        }

        return m_ShuffleWriter;
    }
}