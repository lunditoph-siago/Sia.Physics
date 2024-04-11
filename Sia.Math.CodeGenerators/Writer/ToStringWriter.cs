using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;

namespace Sia.Math.CodeGenerators.Writer;

public class ToStringWriter(VectorType type) : ITypeSourceWriter
{
    private readonly string m_Template = GenerateStringTemplate(type.BaseType, type.Rows, type.Columns);

    public HashSet<string> Imports { get; } = type.BaseType != BaseType.Bool
        ? ["System.Diagnostics.CodeAnalysis", "System.Globalization"]
        : ["System.Globalization"];

    public HashSet<string> Inherits { get; } = type.BaseType != BaseType.Bool ? ["IFormattable"] : [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        source.WriteLine("/// <summary>Returns the string representation of the <see cref=\"{0}\" /> using default formatting.</summary>", type.TypeName);
        source.WriteLine("/// <returns>The string representation of the <see cref=\"{0}\" />.</returns>", type.TypeName);
        source.WriteLine("public readonly override string ToString()");
        source.WriteLine("{");
        source.Indent++;
        {
            if (type.BaseType is BaseType.Bool)
            {
                source.WriteLine("var separator = NumberFormatInfo.GetInstance(CultureInfo.CurrentCulture).NumberGroupSeparator;");
                source.WriteLine("return $\"{0}({1})\";", type.TypeName, m_Template);
            }
            else
            {
                source.WriteLine("return ToString(\"G\", CultureInfo.CurrentCulture);");
            }
        }
        source.Indent--;
        source.WriteLine("}");

        if (type.BaseType is not BaseType.Bool)
        {
            source.WriteLineNoTabs(string.Empty);

            source.WriteLine("/// <summary>Returns a string representation of the <see cref=\"{0}\" /> using the specified format string to format individual elements and the specified format provider to define culture-specific formatting.</summary>", type.TypeName);
            source.WriteLine("/// <param name=\"format\">A standard or custom numeric format string that defines the format of individual elements.</param>");
            source.WriteLine("/// <param name=\"formatProvider\">A format provider that supplies culture-specific formatting information.</param>");
            source.WriteLine("/// <returns>The string representation of the <see cref=\"{0}\" />.</returns>", type.TypeName);
            source.WriteLine("public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)");
            source.WriteLine("{");
            source.Indent++;
            {
                source.WriteLine("var separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;");
                source.WriteLine("return $\"{0}({1})\";", type.TypeName, m_Template);
            }
            source.Indent--;
            source.WriteLine("}");
        }
    };

    private static string GenerateStringTemplate(BaseType baseType, int rows, int columns)
    {
        var templateBuilder = new StringBuilder();

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var idx = row * columns + column;
                if (idx > 0) templateBuilder.Append("{separator} ");

                templateBuilder.Append("{");

                templateBuilder.Append(columns > 1
                    ? VectorType.MatrixFields[column] + "." + VectorType.VectorFields[row]
                    : VectorType.VectorFields[row]);

                if (baseType is not BaseType.Bool) templateBuilder.Append(".ToString(format, formatProvider)");

                templateBuilder.Append("}");

                if (baseType is BaseType.Float) templateBuilder.Append("f");
            }
        }

        return templateBuilder.ToString();
    }
}