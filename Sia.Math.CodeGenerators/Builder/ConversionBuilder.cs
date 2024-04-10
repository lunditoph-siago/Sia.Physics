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
            case "int":
                m_ConversionWriter.Add(new ConversionWriter(type, "bool", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "bool", true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "uint", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "uint", true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "float", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "float", true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "double", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "double", true, false));
                break;
            case "uint":
                m_ConversionWriter.Add(new ConversionWriter(type, "bool", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "bool", true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "int", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "int", true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "float", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "float", true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "double", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "double", true, false));
                break;
            case "float":
                m_ConversionWriter.Add(new ConversionWriter(type, "bool", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "bool", true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "int", false, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "int", false, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "uint", false, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "uint", false, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "double", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "double", true, false));
                break;
            case "double":
                m_ConversionWriter.Add(new ConversionWriter(type, "bool", true, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "bool", true, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "int", false, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "int", false, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "uint", false, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "uint", false, false));
                m_ConversionWriter.Add(new ConversionWriter(type, "float", false, true));
                m_ConversionWriter.Add(new ConversionWriter(type, "float", false, false));
                break;
        }

        return m_ConversionWriter;
    }
}