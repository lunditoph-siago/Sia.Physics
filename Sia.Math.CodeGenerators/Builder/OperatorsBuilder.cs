using Sia.Math.CodeGenerators.Writer;

namespace Sia.Math.CodeGenerators.Builder;

public class OperatorsBuilder(VectorType type) : IBuilder
{
    private readonly TypeOnlyCompositeWriter m_OperatorsWriter = new();

    public ICompositeWriter Build()
    {
        var resultType = type.BaseType;
        var resultBoolType = "bool";

        var isMatrix = type is { Rows: > 1, Columns: > 1 };
        m_OperatorsWriter.Add(new IndexOperatorWriter(type, isMatrix ? IndexerMode.ByRef : IndexerMode.ByValue));

        if (0 != (type.Operations & Features.Arithmetic))
        {
            GenerateBinaryOperator(type.Rows, type.Columns, "*", "<inheritdoc cref=\"IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)\" />", resultType);

            GenerateBinaryOperator(type.Rows, type.Columns, "+", "<inheritdoc cref=\"IAdditionOperators{TSelf, TOther, TResult}.op_Addition(TSelf, TOther)\" />", resultType);

            GenerateBinaryOperator(type.Rows, type.Columns, "-", "<inheritdoc cref=\"IDecrementOperators{TSelf}.op_Decrement(TSelf)\" />", resultType);

            GenerateBinaryOperator(type.Rows, type.Columns, "/", "<inheritdoc cref=\"IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)\" />", resultType);

            GenerateBinaryOperator(type.Rows, type.Columns, "%", "<inheritdoc cref=\"IModulusOperators{TSelf, TOther, TResult}.op_Modulus(TSelf, TOther)\" />", resultType);

            m_OperatorsWriter.Add(new UnaryOperatorWriter(type, "++", "<inheritdoc cref=\"IIncrementOperators{TSelf}.op_Increment(TSelf)\" />"));

            m_OperatorsWriter.Add(new UnaryOperatorWriter(type, "--", "<inheritdoc cref=\"IDecrementOperators{TSelf}.op_Decrement(TSelf)\" />"));

            GenerateBinaryOperator(type.Rows, type.Columns, "<", "<inheritdoc cref=\"IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)\" />", resultBoolType);
            GenerateBinaryOperator(type.Rows, type.Columns, "<=", "<inheritdoc cref=\"IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)\" />", resultBoolType);

            GenerateBinaryOperator(type.Rows, type.Columns, ">", "<inheritdoc cref=\"IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)\" />", resultBoolType);
            GenerateBinaryOperator(type.Rows, type.Columns, ">=", "<inheritdoc cref=\"IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)\" />", resultBoolType);
        }

        if (0 != (type.Operations & Features.UnaryNegation))
        {
            m_OperatorsWriter.Add(new UnaryOperatorWriter(type, "-", "<inheritdoc cref=\"IUnaryNegationOperators{TSelf, TResult}.op_UnaryNegation(TSelf)\" />"));
            m_OperatorsWriter.Add(new UnaryOperatorWriter(type, "+", "<inheritdoc cref=\"IUnaryPlusOperators{TSelf, TResult}.op_UnaryPlus(TSelf)\" />"));
        }

        if (0 != (type.Operations & Features.Shifts))
        {
            m_OperatorsWriter.Add(new ShiftOperatorWriter(type, "<<", "<inheritdoc cref=\"IShiftOperators{TSelf, TOther, TResult}.op_LeftShift(TSelf, TOther)\" />"));
            m_OperatorsWriter.Add(new ShiftOperatorWriter(type, ">>", "<inheritdoc cref=\"IShiftOperators{TSelf, TOther, TResult}.op_RightShift(TSelf, TOther)\" />"));
        }

        GenerateBinaryOperator(type.Rows, type.Columns, "==", "<inheritdoc cref=\"IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)\" />", resultBoolType);
        GenerateBinaryOperator(type.Rows, type.Columns, "!=", "<inheritdoc cref=\"IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)\" />", resultBoolType);

        if (0 != (type.Operations & Features.BitwiseComplement))
        {
            m_OperatorsWriter.Add(new UnaryOperatorWriter(type, "~", "<inheritdoc cref=\"IBitwiseOperators{TSelf, TOther, TResult}.op_OnesComplement(TSelf)\" />"));
        }

        if (type.BaseType == "bool")
        {
            m_OperatorsWriter.Add(new UnaryOperatorWriter(type, "!", "<inheritdoc cref=\"IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)\" />"));
        }

        if (0 != (type.Operations & Features.BitwiseLogic))
        {
            GenerateBinaryOperator(type.Rows, type.Columns, "&", "<inheritdoc cref=\"IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseAnd(TSelf, TOther)\" />", resultType);
            GenerateBinaryOperator(type.Rows, type.Columns, "|", "<inheritdoc cref=\"IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseOr(TSelf, TOther)\" />", resultType);
            GenerateBinaryOperator(type.Rows, type.Columns, "^", "<inheritdoc cref=\"IBitwiseOperators{TSelf, TOther, TResult}.op_ExclusiveOr(TSelf, TOther)\" />", resultType);
        }

        return m_OperatorsWriter;
    }

    private void GenerateBinaryOperator(int rows, int columns, string op, string opDesc, string resultType)
    {
        m_OperatorsWriter.Add(new BinaryOperatorWriter(type, (rows, columns), (rows, columns), op, opDesc, resultType, (rows, columns)));
        m_OperatorsWriter.Add(new BinaryOperatorWriter(type, (rows, columns), (1, 1), op, opDesc, resultType, (rows, columns)));
        m_OperatorsWriter.Add(new BinaryOperatorWriter(type, (1, 1), (rows, columns), op, opDesc, resultType, (rows, columns)));
    }
}