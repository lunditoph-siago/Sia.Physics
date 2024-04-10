using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sia.Math.CodeGenerators.Writer;

public class VectorConstructorWriter(VectorType type, int numParameters, int[] parameterComponents) : ICompositeWriter
{
    private string TypeParams { get; } = GenerateTypeParams(type.BaseType, numParameters, parameterComponents);

    public HashSet<string> Imports { get; } = [];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");

        using (Generator.GenerateInConstructor(source, type.TypeName, TypeParams))
        {
            var componentIndex = 0;
            for (var i = 0; i < numParameters; i++)
            {
                var paramComponents = parameterComponents[i];
                var componentString = string.Join("", VectorType.Components.Skip(componentIndex).Take(paramComponents));
                for (var j = 0; j < paramComponents; j++)
                {
                    source.Write($"this.{VectorType.Components[componentIndex]} = {componentString}");
                    if (paramComponents > 1)
                    {
                        source.Write(".");
                        source.Write(VectorType.Components[j]);
                    }

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
                var componentString = string.Join("", VectorType.Components.Skip(componentIndex).Take(paramComponents));

                if (i != 0) bodyBuilder.Append(", ");
                bodyBuilder.Append(componentString);
                componentIndex += paramComponents;
            }
        }

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} {0}({1}) => new {0}({2});", type.TypeName, TypeParams, bodyBuilder);
    };

    private static string GenerateTypeParams(string baseType, int numParameters, int[] parameterComponents)
    {
        var paramsBuilder = new StringBuilder();

        var componentIndex = 0;
        for (var i = 0; i < numParameters; i++)
        {
            if (i != 0) paramsBuilder.Append(", ");

            var paramComponents = parameterComponents[i];
            var paramType = baseType.ToTypeName(paramComponents, 1);
            var componentString = string.Join("", VectorType.Components.Skip(componentIndex).Take(paramComponents));

            paramsBuilder.Append($"{paramType} {componentString}");
            componentIndex += paramComponents;
        }

        return paramsBuilder.ToString();
    }
}

public class MatrixColumnConstructorWriter(VectorType type) : ICompositeWriter
{
    private string TypeParams { get; } = GenerateTypeParams(type.BaseType.ToTypeName(type.Rows, 1), type.Columns);

    public HashSet<string> Imports { get; } = [];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        using (Generator.GenerateInConstructor(source, type.TypeName, TypeParams))
        {
            for (var column = 0; column < type.Columns; column++)
                source.WriteLine("this.{0} = {0};", VectorType.MatrixFields[column]);
        }
    };

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        var bodyBuilder = new StringBuilder();
        {
            for (var column = 0; column < type.Columns; column++)
            {
                if (column != 0) bodyBuilder.Append(", ");
                bodyBuilder.Append(VectorType.MatrixFields[column]);
            }
        }

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} {0}({1}) => new {0}({2});", type.TypeName, TypeParams, bodyBuilder);
    };

    private static string GenerateTypeParams(string columnType, int columns)
    {
        var paramsBuilder = new StringBuilder();

        for (var column = 0; column < columns; column++)
        {
            if (column != 0) paramsBuilder.Append(", ");

            paramsBuilder.Append(columnType);
            paramsBuilder.Append(" ");
            paramsBuilder.Append(VectorType.MatrixFields[column]);
        }

        return paramsBuilder.ToString();
    }
}

public class MatrixRowConstructorWriter(VectorType type) : ICompositeWriter
{
    private string TypeParams { get; } = GenerateTypeParams(type.BaseType, type.Rows, type.Columns);

    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];

    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> TypeSourceWriter => source =>
    {
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        using (Generator.GenerateInConstructor(source, type.TypeName, TypeParams))
        {
            for (var column = 0; column < type.Columns; column++)
            {
                source.Write($"this.{VectorType.MatrixFields[column]} = new {type.BaseType.ToTypeName(type.Rows, 1)}(");

                for (var row = 0; row < type.Rows; row++)
                {
                    source.Write($"m{row}{column}");

                    if (row != type.Rows - 1) source.Write(", ");
                }

                source.WriteLine(");");
            }
        }
    };

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        var bodyBuilder = new StringBuilder();
        {
            for (var column = 0; column < type.Columns; column++)
            {
                for (var row = 0; row < type.Rows; row++)
                {
                    if (row != 0 || column != 0) bodyBuilder.Append(", ");
                    bodyBuilder.Append($"m{row}{column}");
                }
            }
        }

        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("public static {0} {0}({1}) => new {0}({2});", type.TypeName, TypeParams, bodyBuilder);
    };

    private static string GenerateTypeParams(string baseType, int rows, int columns)
    {
        var paramsBuilder = new StringBuilder();

        for (var row = 0; row < rows; row++)
        {
            if (row != 0) paramsBuilder.Append(", ");

            var columnStr = new string[columns];

            for (var column = 0; column < columns; column++)
                columnStr[column] = $"{baseType} m{row}{column}";

            paramsBuilder.Append(string.Join(", ", columnStr));
        }

        return paramsBuilder.ToString();
    }
}
