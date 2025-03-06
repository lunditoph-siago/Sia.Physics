using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Sia.Math.CodeGenerators;

public record struct FunctionInfo((int, int) Scalar, Func<BaseType, int, string[]> Template);

public record struct Function(string Name, Dictionary<BaseType, FunctionInfo> Infos);

public partial class MathSourceGenerator
{
    private static void AddMathCode(ref GeneratorExecutionContext context)
    {
        var source = Generator.CreateFileSource(out var builder);

        source.WriteLine("using System;");
        source.WriteLine("using System.Runtime.CompilerServices;");
        source.WriteLine();
        source.WriteLine("#pragma warning disable 0660, 0661, 8981");
        source.WriteLine();

        using (Generator.GenerateInNamespace(source))
        {
            using (Generator.GenerateInMath(source))
            {
                var minFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        result.Add($"public static {name} min({name} x, {name} y) => x < y ? x : y;");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i =>
                            {
                                var member = VectorType.Components[i];
                                return $"min(x.{member}, y.{member})";
                            }));
                        result.Add($"public static {name} min({name} x, {name} y) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "min",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Int, minFunc },
                        { BaseType.UInt, minFunc },
                        { BaseType.Float, minFunc },
                        { BaseType.Double, minFunc }
                    }
                ));

                var maxFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        result.Add($"public static {name} max({name} x, {name} y) => x > y ? x : y;");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i =>
                            {
                                var member = VectorType.Components[i];
                                return $"max(x.{member}, y.{member})";
                            }));
                        result.Add($"public static {name} max({name} x, {name} y) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "max",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Int, maxFunc },
                        { BaseType.UInt, maxFunc },
                        { BaseType.Float, maxFunc },
                        { BaseType.Double, maxFunc }
                    }
                ));

                var lerpFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    result.Add($"public static {name} lerp({name} a, {name} b, {name} s) => a + s * (b - a);");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "lerp",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, lerpFunc },
                        { BaseType.Double, lerpFunc }
                    }
                ));

                var unlerpFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    result.Add($"public static {name} unlerp({name} a, {name} b, {name} x) => (x - a) / (b - a);");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "unlerp",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, unlerpFunc },
                        { BaseType.Double, unlerpFunc }
                    }
                ));

                var clampFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    result.Add($"public static {name} clamp({name} v, {name} a, {name} b) => max(a, min(b, v));");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "clamp",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Int, clampFunc },
                        { BaseType.UInt, clampFunc },
                        { BaseType.Float, clampFunc },
                        { BaseType.Double, clampFunc }
                    }
                ));

                var saturateFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);
                    var zero = type.ToTypedLiteral(0);
                    var one = type.ToTypedLiteral(1);

                    result.Add(level == 1
                        ? $"public static {name} saturate({name} x) => clamp(x, {zero}, {one});"
                        : $"public static {name} saturate({name} x) => clamp(x, new({zero}), new({one}));");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "saturate",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, saturateFunc },
                        { BaseType.Double, saturateFunc }
                    }
                ));

                var absFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    result.Add(type switch
                    {
                        BaseType.Int => $"public static {name} abs({name} x) => max(-x, x);",
                        BaseType.Float => $"public static {name} abs({name} x) => asfloat(asuint(x) & 0x7FFFFFFF);",
                        BaseType.Double => level == 1
                            ? $"public static {name} abs({name} x) => asdouble(asulong(x) & 0x7FFFFFFFFFFFFFFF);"
                            : $"public static {name} abs({name} x) => new({string.Join(", ", Enumerable.Range(0, level)
                                .Select(i => 
                                    $"asdouble(asulong(x.{VectorType.Components[i]}) & 0x7FFFFFFFFFFFFFFF)"))});",
                        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                    });

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "abs",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Int, absFunc },
                        { BaseType.Float, absFunc },
                        { BaseType.Double, absFunc }
                    }
                ));

                var dotFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        result.Add($"public static {name} dot({name} x, {name} y) => x * y;");
                    }
                    else
                    {
                        var components = string.Join(" + ", Enumerable.Range(0, level)
                            .Select(i =>
                            {
                                var member = VectorType.Components[i];
                                return $"x.{member} * y.{member}";
                            }));
                        var baseName = type.ToBaseTypeName();
                        result.Add($"public static {baseName} dot({name} x, {name} y) => {components};");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "dot",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Int, dotFunc },
                        { BaseType.UInt, dotFunc },
                        { BaseType.Float, dotFunc },
                        { BaseType.Double, dotFunc }
                    }
                ));

                var tanFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Tan(x)",
                            BaseType.Double => "System.Math.Tan(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} tan({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"tan(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} tan({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "tan",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                         { BaseType.Float, tanFunc },
                         { BaseType.Double, tanFunc }
                    }
                ));

                var tanhFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Tanh(x)",
                            BaseType.Double => "System.Math.Tanh(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} tanh({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"tanh(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} tanh({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "tanh",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, tanhFunc },
                        { BaseType.Double, tanhFunc }
                    }
                ));

                var atanFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Atan(x)",
                            BaseType.Double => "System.Math.Atan(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} atan({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"atan(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} atan({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "atan",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, atanFunc },
                        { BaseType.Double, atanFunc }
                    }
                ));

                var cosFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Cos(x)",
                            BaseType.Double => "System.Math.Cos(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} cos({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"cos(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} cos({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "cos",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                         { BaseType.Float, cosFunc },
                         { BaseType.Double, cosFunc }
                    }
                ));

                var coshFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Cosh(x)",
                            BaseType.Double => "System.Math.Cosh(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} cosh({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"cosh(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} cosh({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "cosh",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, coshFunc },
                        { BaseType.Double, coshFunc }
                    }
                ));

                var acosFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Acos(x)",
                            BaseType.Double => "System.Math.Acos(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} acos({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"acos(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} acos({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "acos",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, acosFunc },
                        { BaseType.Double, acosFunc }
                    }
                ));

                var sinFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Sin(x)",
                            BaseType.Double => "System.Math.Sin(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} sin({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"sin(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} sin({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "sin",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                         { BaseType.Float, sinFunc },
                         { BaseType.Double, sinFunc }
                    }
                ));

                var sinhFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Sinh(x)",
                            BaseType.Double => "System.Math.Sinh(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} sinh({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"sinh(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} sinh({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "sinh",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, sinhFunc },
                        { BaseType.Double, sinhFunc }
                    }
                ));

                var asinFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Asin(x)",
                            BaseType.Double => "System.Math.Asin(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} asin({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"asin(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} asin({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "asin",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, asinFunc },
                        { BaseType.Double, asinFunc }
                    }
                ));

                var rcpFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);
                    var one = type.ToTypedLiteral(1);

                    result.Add($"public static {name} rcp({name} x) => {one} / x;");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "rcp",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, rcpFunc },
                        { BaseType.Double, rcpFunc }
                    }
                ));

                var powFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Pow(x, y)",
                            BaseType.Double => "System.Math.Pow(x, y)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} pow({name} x, {name} y) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"pow(x.{VectorType.Components[i]}, y.{VectorType.Components[i]})"));
                        result.Add($"public static {name} pow({name} x, {name} y) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "pow",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, powFunc },
                        { BaseType.Double, powFunc }
                    }
                ));

                var expFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Exp(x)",
                            BaseType.Double => "System.Math.Exp(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} exp({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"exp(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} exp({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "exp",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, expFunc },
                        { BaseType.Double, expFunc }
                    }
                ));

                var logFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Log(x)",
                            BaseType.Double => "System.Math.Log(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} log({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"log(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} log({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "log",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, logFunc },
                        { BaseType.Double, logFunc }
                    }
                ));

                var sqrtFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        var template = type switch
                        {
                            BaseType.Float => "(float)System.Math.Sqrt(x)",
                            BaseType.Double => "System.Math.Sqrt(x)",
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        result.Add($"public static {name} sqrt({name} x) => {template};");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i => $"sqrt(x.{VectorType.Components[i]})"));
                        result.Add($"public static {name} sqrt({name} x) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "sqrt",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, sqrtFunc },
                        { BaseType.Double, sqrtFunc }
                    }
                ));

                var rsqrtFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    result.Add($"public static {name} rsqrt({name} x) => 1.0f / sqrt(x);");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "rsqrt",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, rsqrtFunc },
                        { BaseType.Double, rsqrtFunc }
                    }
                ));

                var normalizeFunc = new FunctionInfo((2, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    result.Add($"public static {name} normalize({name} x) => rsqrt(dot(x, x)) * x;");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "normalize",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, normalizeFunc },
                        { BaseType.Double, normalizeFunc }
                    }
                ));

                var anyFunc = new FunctionInfo((2, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);
                    
                    var components = string.Join(" || ", Enumerable.Range(0, level)
                        .Select(i =>
                        {
                            var member = VectorType.Components[i];
                            return type switch
                            {
                                BaseType.Bool => $"x.{member}",
                                BaseType.Int or BaseType.UInt => $"x.{member} != 0.0",
                                BaseType.Float => $"x.{member} != 0.0f",
                                BaseType.Double => $"x.{member} != 0.0",
                                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                            };
                        }));
                    result.Add($"public static bool any({name} x) => {components};");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "any",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Bool, anyFunc },
                        { BaseType.Int, anyFunc },
                        { BaseType.UInt, anyFunc },
                        { BaseType.Float, anyFunc },
                        { BaseType.Double, anyFunc }
                    }
                ));

                var allFunc = new FunctionInfo((2, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);
                    
                    var components = string.Join(" && ", Enumerable.Range(0, level)
                        .Select(i =>
                        {
                            var member = VectorType.Components[i];
                            return type switch
                            {
                                BaseType.Bool => $"x.{member}",
                                BaseType.Int or BaseType.UInt => $"x.{member} != 0.0",
                                BaseType.Float => $"x.{member} != 0.0f",
                                BaseType.Double => $"x.{member} != 0.0",
                                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                            };
                        }));
                    result.Add($"public static bool all({name} x) => {components};");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "all",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Bool, allFunc },
                        { BaseType.Int, allFunc },
                        { BaseType.UInt, allFunc },
                        { BaseType.Float, allFunc },
                        { BaseType.Double, allFunc }
                    }
                ));

                var selectFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var test = BaseType.Bool.ToTypeName(1, level);
                    var name = type.ToTypeName(1, level);

                    if (level == 1)
                    {
                        result.Add($"public static {name} select({name} a, {name} b, {test} test) => test ? b : a;");
                    }
                    else
                    {
                        var components = string.Join(", ", Enumerable.Range(0, level)
                            .Select(i =>
                            {
                                var member = VectorType.Components[i];
                                return $"test.{member} ? b.{member} : a.{member}";
                            }));
                        result.Add($"public static {name} select({name} a, {name} b, {test} test) => new({components});");
                    }

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "select",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Int, selectFunc },
                        { BaseType.UInt, selectFunc },
                        { BaseType.Float, selectFunc },
                        { BaseType.Double, selectFunc }
                    }
                ));

                var sincosFunc = new FunctionInfo((1, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    result.Add($"public static void sincos({name} v, out {name} s, out {name} c) {{ s = sin(v); c = cos(v); }}");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "sincos",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Float, sincosFunc },
                        { BaseType.Double, sincosFunc }
                    }
                ));

                var csumFunc = new FunctionInfo((2, 4), (type, level) =>
                {
                    List<string> result = ["[MethodImpl(MethodImplOptions.AggressiveInlining)]"];
                    var name = type.ToTypeName(1, level);

                    var components = string.Join(" + ", Enumerable.Range(0, level)
                        .Select(i => $"x.{VectorType.Components[i]}"));
                    result.Add($"public static {type.ToBaseTypeName()} csum({name} x) => {components};");

                    return result.ToArray();
                });
                BuildFunction(source, new Function(
                    "csum",
                    new Dictionary<BaseType, FunctionInfo>
                    {
                        { BaseType.Int, csumFunc },
                        { BaseType.UInt, csumFunc },
                        { BaseType.Float, csumFunc },
                        { BaseType.Double, csumFunc }
                    }
                ));
            }
        }

        context.AddSource("math.g.cs", builder.ToString());
    }

    private static void BuildFunction(IndentedTextWriter writer, Function func)
    {
        writer.WriteLine("#region {0}", func.Name);
        writer.WriteLineNoTabs(string.Empty);

        foreach (var data in func.Infos)
        {
            var type = data.Key;
            var (min, max) = data.Value.Scalar;
            var template = data.Value.Template;

            for (var i = min; i <= max; i++)
            {
                foreach (var line in template.Invoke(type, i))
                {
                    writer.WriteLine(line);
                }

                writer.WriteLineNoTabs(string.Empty);
            }
        }

        writer.WriteLine("#endregion");
        writer.WriteLineNoTabs(string.Empty);
    }
}