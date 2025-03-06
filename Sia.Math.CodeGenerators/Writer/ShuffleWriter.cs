using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Sia.Math.CodeGenerators.Writer;

public class SelectShuffleWriter(VectorType type) : IMathSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];
    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        source.WriteLine("internal static {0} select_shuffle_component({1} a, {1} b, ShuffleComponent component) => component switch {{", type.BaseTypeName, type.TypeName);
        source.Indent++;
        {
            foreach (var component in VectorType.ShuffleComponents.Take(type.Rows * 2))
            {
                var index = Array.IndexOf(VectorType.ShuffleComponents, component);
                var target = index >= type.Rows ? "b" : "a";
                var field = type.Rows > 1 ? $".{VectorType.VectorFields[index % type.Rows]}" : string.Empty;
                source.WriteLine($"ShuffleComponent.{component} => {target}{field},");
            }
            source.WriteLine("_ => throw new System.ArgumentException($\"Invalid shuffle component: {component}\")");
        }
        source.Indent--;
        source.WriteLine("};");
    };
}

public class ShuffleWriter(VectorType type) : IMathSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];
    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        for (var comp = 1; comp <= 4; comp++)
        {
            var resultType = type.BaseType.ToTypeName(comp, 1);
            var fromTypeDesc = type.BaseType.ToTypeDescription(type.Rows, type.Columns, 2);
            var toTypeDesc = type.BaseType.ToTypeDescription(comp, type.Columns, 1);

            source.WriteLine("/// <summary>Returns the result of specified shuffling of the components from {0} into {1}.</summary>", fromTypeDesc, toTypeDesc);
            source.WriteLine("/// <param name=\"left\">{0} to use as the left argument</param>", type.TypeName);
            source.WriteLine("/// <param name=\"right\">{0} to use as the right argument</param>", type.TypeName);

            foreach (var field in VectorType.VectorFields.Take(comp))
            {
                source.WriteLine("/// <param name=\"{0}\">{1} component selector</param>", field, comp > 1 ? $"{resultType} {field}" : string.Empty);
            }

            source.WriteLine("/// <returns>{0} shuffled value</returns>", resultType);

            var components = string.Join(", ", Enumerable.Range(0, comp)
                .Select(i => $"ShuffleComponent {VectorType.VectorFields[i]}"));

            source.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            source.WriteLine("public static {0} shuffle({1} left, {1} right, {2}) =>", resultType, type.TypeName, components);
            source.Indent++;
            if (comp == 1)
            {
                source.WriteLine("select_shuffle_component(left, right, {0});", VectorType.VectorFields[0]);
            }
            else
            {
                var bodyStr = string.Join(", ", Enumerable.Range(0, comp)
                    .Select(i => $"select_shuffle_component(left, right, {VectorType.VectorFields[i]})"));
                source.WriteLine("new {0}({1});", resultType, bodyStr);
            }
            source.Indent--;

            if (comp < 4) source.WriteLineNoTabs(string.Empty);
        }
    };
}