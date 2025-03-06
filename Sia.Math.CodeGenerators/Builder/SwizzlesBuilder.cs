using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators.Builder;

public class SwizzlesBuilder(VectorType type) : IBuilder
{
    private readonly TypeOnlyCompositeWriter m_SwizzlesWriter = new();

    public ICompositeWriter Build()
    {
        if (type.Columns == 1)
        {
            var count = type.Rows;

            for (var x = 0; x < count; x++)
            {
                for (var y = 0; y < count; y++)
                {
                    for (var z = 0; z < count; z++)
                    {
                        for (var w = 0; w < count; w++)
                        {
                            m_SwizzlesWriter.Add(new SwizzlesWriter(type, [x, y, z, w]));
                        }
                    }
                }
            }

            for (var x = 0; x < count; x++)
            {
                for (var y = 0; y < count; y++)
                {
                    for (var z = 0; z < count; z++)
                    {
                        m_SwizzlesWriter.Add(new SwizzlesWriter(type, [x, y, z]));
                    }
                }
            }

            for (var x = 0; x < count; x++)
            {
                for (var y = 0; y < count; y++)
                {
                    m_SwizzlesWriter.Add(new SwizzlesWriter(type, [x, y]));
                }
            }
        }

        return m_SwizzlesWriter;
    }
}