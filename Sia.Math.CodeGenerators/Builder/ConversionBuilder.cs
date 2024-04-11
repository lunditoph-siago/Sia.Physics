using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators.Builder;

public class ConversionBuilder(VectorType type): IBuilder
{
    private readonly ConversionCompositeWriter m_ConversionWriter = new();

    public ICompositeWriter Build()
    {
        m_ConversionWriter.Add(new ConversionWriter(type, type.BaseType, false, true));

        switch (type.BaseType)
        {
            case BaseType.Int:
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Bool, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Bool, true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.UInt, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.UInt, true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Float, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Float, true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Double, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Double, true, false));
                break;
            case BaseType.UInt:
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Bool, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Bool, true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Int, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Int, true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Float, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Float, true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Double, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Double, true, false));
                break;
            case BaseType.Float:
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Bool, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Bool, true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Int, false, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Int, false, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.UInt, false, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.UInt, false, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Double, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Double, true, false));
                break;
            case BaseType.Double:
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Bool, true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Bool, true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Int, false, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Int, false, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.UInt, false, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.UInt, false, false));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Float, false, true));
                m_ConversionWriter.Add(new ConversionWriter(type, BaseType.Float, false, false));
                break;
        }

        return m_ConversionWriter;
    }
}