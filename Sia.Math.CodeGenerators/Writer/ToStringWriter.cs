using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace Sia.Math.CodeGenerators.Writer;

public class ToStringWriter(VectorType type, bool useFormat) : ITypeSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];
    public HashSet<string> Inherits => useFormat ? ["IFormattable"] : [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");

        source.WriteLine(useFormat
            ? "public string ToString(string? format, IFormatProvider? formatProvider) => "
            : "public override string ToString() =>");

        source.Indent++;

        var templateStr = new StringBuilder();
        {
            for (var row = 0; row < type.Rows; row++)
            {
                for (var column = 0; column < type.Columns; column++)
                {
                    var idx = row * type.Columns + column;
                    if (idx > 0)
                    {
                        templateStr.Append(", ");
                        if (type.Columns > 1 && column == 0)
                            templateStr.Append(" ");
                    }

                    templateStr.Append($"{{{idx}}}");
                    if (type.BaseType == "float") templateStr.Append("f");
                }
            }
        }

        var paramsStr = new StringBuilder();
        {
            for (var row = 0; row < type.Rows; row++)
            {
                for (var column = 0; column < type.Columns; column++)
                {
                    var idx = row * type.Columns + column;
                    if (idx > 0)
                        paramsStr.Append(", ");

                    if (type.Columns > 1)
                        paramsStr.Append(VectorType.MatrixFields[column] + "." + VectorType.VectorFields[row]);
                    else
                        paramsStr.Append(VectorType.VectorFields[row]);

                    if (useFormat) paramsStr.Append(".ToString(format, formatProvider)");
                }
            }
        }

        source.WriteLine($"string.Format(\"{type.TypeName}({templateStr})\", {paramsStr});");

        source.Indent--;
    };
}