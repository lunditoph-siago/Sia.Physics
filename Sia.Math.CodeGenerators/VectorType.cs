using System;
using System.Runtime.CompilerServices;

namespace Sia.Math.CodeGenerators;

public enum BaseType
{
    Bool,
    Int,
    UInt,
    Float,
    Double,
}

[Flags]
public enum Features
{
    Arithmetic = 1 << 0,
    Shifts = 1 << 1,
    BitwiseLogic = 1 << 2,
    BitwiseComplement = 1 << 3,
    UnaryNegation = 1 << 4,
    All = Arithmetic | Shifts | BitwiseLogic | BitwiseComplement | UnaryNegation
}

public record struct VectorType(BaseType BaseType, int Rows, int Columns, Features Operations)
{
    public string BaseTypeName { get; } = BaseType.ToBaseTypeName();
    public string TypeName { get; } = BaseType.ToTypeName(Rows, Columns);

    public static readonly string[] Components = ["x", "y", "z", "w"];

    public static readonly string[] VectorFields = ["x", "y", "z", "w"];

    public static readonly string[] MatrixFields = ["c0", "c1", "c2", "c3"];

    public static readonly string[] ShuffleComponents =
    [
        "LeftX", "LeftY", "LeftZ", "LeftW",
        "RightX", "RightY", "RightZ", "RightW"
    ];
}

public static class BaseTypeHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToBaseTypeName(this BaseType baseType)
    {
        return baseType switch
        {
            BaseType.Bool => "bool",
            BaseType.Int => "int",
            BaseType.UInt => "uint",
            BaseType.Float => "float",
            BaseType.Double => "double",
            _ => throw new ArgumentOutOfRangeException(nameof(baseType), baseType, "Unsupported value type.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTypeName(this BaseType baseType, int rows, int columns)
    {
        if (rows == 1 && columns > 1) return baseType.ToBaseTypeName() + columns;

        return $"{baseType.ToBaseTypeName()}{(rows > 1 ? rows.ToString() : string.Empty)}{(columns > 1 ? $"x{columns}" : string.Empty)}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTypeDescription(this BaseType baseType, int rows, int columns, int num, bool addPrefix = false)
    {
        var name = $"<see cref=\"{baseType.ToTypeName(rows, columns)}\" />";

        var numStr = num.FormatQuantifiers(baseType is BaseType.Int);

        var vectorPrefix = addPrefix ? rows == 1 ? " row" : columns == 1 ? " column" : string.Empty : string.Empty;

        return num > 1
            ? numStr + name + (rows == 1 && columns == 1 ? " values" :
                rows > 1 && columns > 1 ? " matrices" : $"{vectorPrefix} vectors")
            : numStr + name + (rows == 1 && columns == 1 ? " value" :
                rows > 1 && columns > 1 ? " matrix" : $"{vectorPrefix} vector");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToTypedLiteral(this BaseType baseType, int value)
    {
        return baseType switch
        {
            BaseType.Int => $"{value}",
            BaseType.UInt => $"{value}u",
            BaseType.Float => $"{value}.0f",
            BaseType.Double => $"{value}.0",
            _ => string.Empty
        };
    }
}

public static class NumberHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatOrdinals(this int number)
    {
        return number switch
        {
            1 => "first",
            2 => "second",
            3 => "third",
            4 => "fourth",
            _ => string.Empty
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string FormatQuantifiers(this int number, bool isVowel)
    {
        return number switch
        {
            1 => isVowel ? "an " : "a ",
            2 => "two ",
            3 => "three ",
            4 => "four ",
            _ => string.Empty
        };
    }
}