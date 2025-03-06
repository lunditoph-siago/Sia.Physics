using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sia.Math.CodeGenerators.Writer;

public class VectorConstructorWriter(VectorType type, int numParameters, int[] parameterComponents) : ICompositeWriter
{
    private readonly string m_TypeParams = GenerateTypeParams(type.BaseType, numParameters, parameterComponents);
    private readonly IEnumerable<string> m_Descriptions = GenerateDescriptions(type, numParameters, parameterComponents);

    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        foreach (var description in m_Descriptions.Take(m_Descriptions.Count() - 1)) source.WriteLine(description);

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        using (Generator.GenerateInConstructor(source, type.TypeName, m_TypeParams))
        {
            var componentIndex = 0;
            foreach (var components in parameterComponents.Take(numParameters))
            {
                var componentString = string.Concat(VectorType.Components.Skip(componentIndex).Take(components));
                for (var j = 0; j < components; j++)
                {
                    source.Write($"this.{VectorType.Components[componentIndex]} = {componentString}");
                    if (components > 1) source.Write($".{VectorType.Components[j]}");

                    source.WriteLine(";");
                    componentIndex++;
                }
            }
        }
    };

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        var bodyBuilder = new StringBuilder();
        {
            var componentIndex = 0;
            for (var i = 0; i < numParameters; i++)
            {
                var paramComponents = parameterComponents[i];
                var componentString = string.Concat(VectorType.Components.Skip(componentIndex).Take(paramComponents));

                if (i != 0) bodyBuilder.Append(", ");
                bodyBuilder.Append(componentString);
                componentIndex += paramComponents;
            }
        }

        foreach (var description in m_Descriptions) source.WriteLine(description);

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} {0}({1}) => new {0}({2});", type.TypeName, m_TypeParams, bodyBuilder);
    };

    private static IEnumerable<string> GenerateDescriptions(VectorType type, int numParameters, int[] parameterComponents)
    {
        var typeCategory = type.Columns > 1 ? "matrix" : "vector";
        var parameterParts = Enumerable.Range(0, numParameters)
            .GroupBy(i => parameterComponents[i])
            .Select(g => type.BaseType.ToTypeDescription(g.Key, 1, g.Count()))
            .ToList();

        var parameterDesc = parameterParts.Count > 1
            ? $"{string.Join(", ", parameterParts.Take(parameterParts.Count - 1))} and {parameterParts.Last()}"
            : parameterParts.Last();
        var descriptions = new List<string>
        {
            $"/// <summary>Constructs a <see cref=\"{type.TypeName}\" /> {typeCategory} from {parameterDesc}.</summary>"
        };

        descriptions.AddRange(parameterComponents.Take(numParameters)
            .Select((components, i) =>
            {
                var componentString = string.Concat(VectorType.Components.Skip(parameterComponents.Take(i).Sum()).Take(components));
                var componentPluralOrSingular = components > 1 ? "fields" : "field";
                return $"/// <param name=\"{componentString}\">The value to assign to the <see cref=\"{componentString}\" /> {componentPluralOrSingular}.</param>";
            }));

        descriptions.Add($"/// <returns>The <see cref=\"{type.TypeName}\" /> constructed from arguments.</returns>");

        return descriptions;
        
    }

    private static string GenerateTypeParams(BaseType baseType, int numParameters, int[] parameterComponents)
    {
        var componentIndex = 0;
        return string.Join(", ", parameterComponents.Take(numParameters)
            .Select(components =>
            {
                var paramType = baseType.ToTypeName(components, 1);
                var componentString = string.Join("", VectorType.Components.Skip(componentIndex).Take(components));
                componentIndex += components;
                return $"{paramType} {componentString}";
            }));
    }
}

public class MatrixColumnConstructorWriter(VectorType type) : ICompositeWriter
{
    private readonly string m_TypeParams = GenerateTypeParams(type);
    private readonly IEnumerable<string> m_Descriptions = GenerateDescriptions(type);

    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        foreach (var description in m_Descriptions.Take(m_Descriptions.Count() - 1)) source.WriteLine(description);

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        using (Generator.GenerateInConstructor(source, type.TypeName, m_TypeParams))
        {
            foreach (var field in VectorType.MatrixFields.Take(type.Columns))
            {
                source.WriteLine("this.{0} = {0};", field);
            }
        }
    };

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        var bodyBuilder = string.Join(", ", VectorType.MatrixFields.Take(type.Columns));

        foreach (var description in m_Descriptions) source.WriteLine(description);

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} {0}({1}) => new {0}({2});", type.TypeName, m_TypeParams, bodyBuilder);
    };

    private static IEnumerable<string> GenerateDescriptions(VectorType type)
    {
        var descriptions = new List<string>
        {
            $"/// <summary>Constructs a <see cref=\"{type.TypeName}\" /> matrix from {type.BaseType.ToTypeDescription(type.Rows, 1, type.Columns)}.</summary>"
        };

        descriptions.AddRange(Enumerable.Range(0, type.Columns)
            .Select(i => $"/// <param name=\"{VectorType.MatrixFields[i]}\">The value to assign to the <see cref=\"{VectorType.MatrixFields[i]}\" /> field.</param>"));

        descriptions.Add($"/// <returns>The <see cref=\"{type.TypeName}\" /> constructed from arguments.</returns>");

        return descriptions;
    }

    private static string GenerateTypeParams(VectorType type)
    {
        return string.Join(", ", VectorType.MatrixFields.Take(type.Columns)
            .Select(field => $"{type.BaseType.ToTypeName(type.Rows, 1)} {field}"));
    }
}

public class MatrixRowConstructorWriter(VectorType type) : ICompositeWriter
{
    private readonly string m_TypeParams = GenerateTypeParams(type);
    private readonly IEnumerable<string> m_Descriptions = GenerateDescriptions(type);

    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        foreach (var description in m_Descriptions.Take(m_Descriptions.Count() - 1)) source.WriteLine(description);

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        using (Generator.GenerateInConstructor(source, type.TypeName, m_TypeParams))
        {
            var assignments = VectorType.MatrixFields.Take(type.Columns)
                .Select((column, i) =>
                    $"this.{column} = new {type.BaseType.ToTypeName(type.Rows, 1)}({
                        string.Join(", ", Enumerable.Range(0, type.Rows).Select(row => $"m{row}{i}"))
                    });");

            foreach (var assignment in assignments) source.WriteLine(assignment);
        }
    };

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        var bodyBuilder = string.Join(", ", Enumerable.Range(0, type.Rows * type.Columns)
            .Select(i => $"m{i / type.Columns}{i % type.Columns}"));

        foreach (var description in m_Descriptions) source.WriteLine(description);

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} {0}({1}) => new {0}({2});", type.TypeName, m_TypeParams, bodyBuilder);
    };

    private static IEnumerable<string> GenerateDescriptions(VectorType type)
    {
        var descriptions = new List<string>
        {
            $"/// <summary>Constructs a <see cref=\"{type.TypeName}\" /> matrix from {type.Rows * type.Columns} <see cref=\"{type.BaseTypeName}\" /> values given in row-major order.</summary>"
        };

        descriptions.AddRange(Enumerable.Range(0, type.Rows * type.Columns)
            .Select(i =>
            {
                var (row, column) = (i / type.Columns, i % type.Columns);
                return $"/// <param name=\"m{row}{column}\">The value to assign to the {(column + 1).FormatOrdinals()} element in the {(row + 1).FormatOrdinals()} row.</param>";
            }));

        descriptions.Add($"/// <returns>The <see cref=\"{type.TypeName}\" /> constructed from arguments.</returns>");

        return descriptions;
    }

    private static string GenerateTypeParams(VectorType type)
    {
        return string.Join(", ", Enumerable.Range(0, type.Rows * type.Columns)
            .Select(i => $"{type.BaseTypeName} m{i / type.Columns}{i % type.Columns}"));
    }
}
