using System;

namespace Sia.Math.CodeGenerators;

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

public record struct VectorType(string BaseType, int Rows, int Columns, Features Operations)
{
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