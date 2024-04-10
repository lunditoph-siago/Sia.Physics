using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators.Builder;

public class ConstructorsBuilder(VectorType type) : IBuilder
{
    private readonly CompositeWriter m_ConstructorsWriter = new();

    public ICompositeWriter Build()
    {
        if (type.Columns == 1)
        {
            var parameterComponenets = new int[4];
            GenerateVectorConstructors(type.Rows, 0, parameterComponenets);
        }
        else
        {
            GenerateMatrixConstructors();
        }

        return m_ConstructorsWriter;
    }

    private void GenerateVectorConstructors(int numRemainingComponents, int numParameters, int[] parameterComponents)
    {
        if (numRemainingComponents == 0)
        {
            m_ConstructorsWriter.Add(new VectorConstructorWriter(type, numParameters, [..parameterComponents]));
        }

        for (var i = 1; i <= numRemainingComponents; i++)
        {
            parameterComponents[numParameters] = i;

            GenerateVectorConstructors(numRemainingComponents - i, numParameters + 1, parameterComponents);
        }
    }

    private void GenerateMatrixConstructors()
    {
        m_ConstructorsWriter.Add(new MatrixColumnConstructorWriter(type));
        m_ConstructorsWriter.Add(new MatrixRowConstructorWriter(type));
    }
}