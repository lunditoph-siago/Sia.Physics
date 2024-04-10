using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Sia.Math.CodeGenerators.Writer;

public class TypeDividerWriter : ITypeSourceWriter
{
    public HashSet<string> Imports { get; } = [];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        source.WriteLineNoTabs(string.Empty);
    };
}

public class MathDividerWriter : IMathSourceWriter
{
    public HashSet<string> Imports { get; } = [];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        source.WriteLineNoTabs(string.Empty);
    };
}