using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators.Builder;

public interface IBuilder
{
    ICompositeWriter Build();
}