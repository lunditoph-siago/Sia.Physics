namespace Sia.Math.CodeGenerators;

public static class BaseTypeHelper
{
    public static string ToTypeName(this string baseType, int rows, int columns)
    {
        if (rows == 1 && columns > 1) return baseType + columns;

        return $"{baseType}{(rows > 1 ? rows.ToString() : "")}{(columns > 1 ? $"x{columns}" : "")}";
    }

    public static string ToTypeDescription(this string baseType, int rows, int columns, int num, bool addPrefix = false)
    {
        var name = baseType.ToTypeName(rows, columns);

        var numStr = num switch
        {
            1 => baseType == "int" ? "an " : "a ",
            2 => "two ",
            3 => "three ",
            4 => "four ",
            _ => ""
        };

        var vectorPrefix = addPrefix ? rows == 1 ? " row" : columns == 1 ? " column" : "" : "";

        return num > 1
            ? numStr + name + (rows == 1 && columns == 1 ? " values" :
                rows > 1 && columns > 1 ? " matrices" : $"{vectorPrefix} vectors")
            : numStr + name + (rows == 1 && columns == 1 ? " value" :
                rows > 1 && columns > 1 ? " matrix" : $"{vectorPrefix} vector");
    }

    public static string ToTypedLiteral(this string baseType, int value)
    {
        return baseType switch
        {
            "int" => $"{value}",
            "uint" => $"{value}u",
            "half" or "float" => $"{value}.0f",
            "double" => $"{value}.0",
            _ => ""
        };
    }   
}