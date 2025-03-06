using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Sia.Math.CodeGenerators.Writer;

public class InverseWriter(VectorType type) : IMathSourceWriter
{
    public HashSet<string> Imports { get; } = ["System.Runtime.CompilerServices"];
    public HashSet<string> Inherits { get; } = [];

    public Action<IndentedTextWriter> MathSourceWriter => source =>
    {
        if (type.Rows != type.Columns || type.Rows == 1) return;
        if (type.BaseType is not BaseType.Float and not BaseType.Double) return;

        var one = type.BaseType.ToTypedLiteral(1);
        var typeName = type.BaseTypeName;

        if (type.Rows == 2)
        {
            source.WriteLine("/// <summary>Returns the {0}2x2 full inverse of a {0}2x2 matrix.</summary>", typeName);
            source.WriteLine("/// <param name=\"m\">Matrix to invert.</param>");
            source.WriteLine("/// <returns>The inverted matrix.</returns>");
            source.WriteLine("public static {0}2x2 inverse({0}2x2 m)", typeName);
            source.WriteLine("{");
            source.Indent++;
            {
                source.WriteLine("var a = m.c0.x;");
                source.WriteLine("var b = m.c1.x;");
                source.WriteLine("var c = m.c0.y;");
                source.WriteLine("var d = m.c1.y;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var det = a * d - b * c;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("return new {0}2x2(d, -b, -c, a) * ({1} / det);", typeName, one);
            }
            source.Indent--;
            source.WriteLine("}");
        }
        else if (type.Rows == 3)
        {
            source.WriteLine("/// <summary>Returns the {0}3x3 full inverse of a {0}3x3 matrix.</summary>", typeName);
            source.WriteLine("/// <param name=\"m\">Matrix to invert.</param>");
            source.WriteLine("/// <returns>The inverted matrix.</returns>");
            source.WriteLine("public static {0}3x3 inverse({0}3x3 m)", typeName);
            source.WriteLine("{");
            source.Indent++;
            {
                source.WriteLine("var c0 = m.c0;");
                source.WriteLine("var c1 = m.c1;");
                source.WriteLine("var c2 = m.c2;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var t0 = new {0}3(c1.x, c2.x, c0.x);", typeName);
                source.WriteLine("var t1 = new {0}3(c1.y, c2.y, c0.y);", typeName);
                source.WriteLine("var t2 = new {0}3(c1.z, c2.z, c0.z);", typeName);
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var m0 = t1 * t2.yzx - t1.yzx * t2;");
                source.WriteLine("var m1 = t0.yzx * t2 - t0 * t2.yzx;");
                source.WriteLine("var m2 = t0 * t1.yzx - t0.yzx * t1;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var rcpDet = {0} / csum(t0.zxy * m0);", one);
                source.WriteLine("return new {0}3x3(m0, m1, m2) * rcpDet;", typeName);
            }
            source.Indent--;
            source.WriteLine("}");
        }
        else if (type.Rows == 4)
        {
            source.WriteLine("/// <summary>Returns the {0}4x4 full inverse of a {0}4x4 matrix.</summary>", typeName);
            source.WriteLine("/// <param name=\"m\">Matrix to invert.</param>");
            source.WriteLine("/// <returns>The inverted matrix.</returns>");
            source.WriteLine("public static {0}4x4 inverse({0}4x4 m)", typeName);
            source.WriteLine("{");
            source.Indent++;
            {
                source.WriteLine("var c0 = m.c0;");
                source.WriteLine("var c1 = m.c1;");
                source.WriteLine("var c2 = m.c2;");
                source.WriteLine("var c3 = m.c3;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var r0y_r1y_r0x_r1x = movelh(c1, c0);");
                source.WriteLine("var r0z_r1z_r0w_r1w = movelh(c2, c3);");
                source.WriteLine("var r2y_r3y_r2x_r3x = movehl(c0, c1);");
                source.WriteLine("var r2z_r3z_r2w_r3w = movehl(c3, c2);");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var r1y_r2y_r1x_r2x = shuffle(c1, c0, ShuffleComponent.LeftY, ShuffleComponent.LeftZ, ShuffleComponent.RightY, ShuffleComponent.RightZ);");
                source.WriteLine("var r1z_r2z_r1w_r2w = shuffle(c2, c3, ShuffleComponent.LeftY, ShuffleComponent.LeftZ, ShuffleComponent.RightY, ShuffleComponent.RightZ);");
                source.WriteLine("var r3y_r0y_r3x_r0x = shuffle(c1, c0, ShuffleComponent.LeftW, ShuffleComponent.LeftX, ShuffleComponent.RightW, ShuffleComponent.RightX);");
                source.WriteLine("var r3z_r0z_r3w_r0w = shuffle(c2, c3, ShuffleComponent.LeftW, ShuffleComponent.LeftX, ShuffleComponent.RightW, ShuffleComponent.RightX);");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var r0_wzyx = shuffle(r0z_r1z_r0w_r1w, r0y_r1y_r0x_r1x, ShuffleComponent.LeftZ, ShuffleComponent.LeftX, ShuffleComponent.RightX, ShuffleComponent.RightZ);");
                source.WriteLine("var r1_wzyx = shuffle(r0z_r1z_r0w_r1w, r0y_r1y_r0x_r1x, ShuffleComponent.LeftW, ShuffleComponent.LeftY, ShuffleComponent.RightY, ShuffleComponent.RightW);");
                source.WriteLine("var r2_wzyx = shuffle(r2z_r3z_r2w_r3w, r2y_r3y_r2x_r3x, ShuffleComponent.LeftZ, ShuffleComponent.LeftX, ShuffleComponent.RightX, ShuffleComponent.RightZ);");
                source.WriteLine("var r3_wzyx = shuffle(r2z_r3z_r2w_r3w, r2y_r3y_r2x_r3x, ShuffleComponent.LeftW, ShuffleComponent.LeftY, ShuffleComponent.RightY, ShuffleComponent.RightW);");
                source.WriteLine("var r0_xyzw = shuffle(r0y_r1y_r0x_r1x, r0z_r1z_r0w_r1w, ShuffleComponent.LeftZ, ShuffleComponent.LeftX, ShuffleComponent.RightX, ShuffleComponent.RightZ);");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("// Calculate remaining inner term pairs. inner terms have zw=-xy, so we only have to calculate xy and can pack two pairs per vector.");
                source.WriteLine("var inner12_23 = r1y_r2y_r1x_r2x * r2z_r3z_r2w_r3w - r1z_r2z_r1w_r2w * r2y_r3y_r2x_r3x;");
                source.WriteLine("var inner02_13 = r0y_r1y_r0x_r1x * r2z_r3z_r2w_r3w - r0z_r1z_r0w_r1w * r2y_r3y_r2x_r3x;");
                source.WriteLine("var inner30_01 = r3z_r0z_r3w_r0w * r0y_r1y_r0x_r1x - r3y_r0y_r3x_r0x * r0z_r1z_r0w_r1w;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("// Expand inner terms back to 4 components. zw signs still need to be flipped");
                source.WriteLine("var inner12 = shuffle(inner12_23, inner12_23, ShuffleComponent.LeftX, ShuffleComponent.LeftZ, ShuffleComponent.RightZ, ShuffleComponent.RightX);");
                source.WriteLine("var inner23 = shuffle(inner12_23, inner12_23, ShuffleComponent.LeftY, ShuffleComponent.LeftW, ShuffleComponent.RightW, ShuffleComponent.RightY);");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var inner02 = shuffle(inner02_13, inner02_13, ShuffleComponent.LeftX, ShuffleComponent.LeftZ, ShuffleComponent.RightZ, ShuffleComponent.RightX);");
                source.WriteLine("var inner13 = shuffle(inner02_13, inner02_13, ShuffleComponent.LeftY, ShuffleComponent.LeftW, ShuffleComponent.RightW, ShuffleComponent.RightY);");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("// Calculate minors");
                source.WriteLine("var minors0 = r3_wzyx * inner12 - r2_wzyx * inner13 + r1_wzyx * inner23;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var denom = r0_xyzw * minors0;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("// Horizontal sum of denominator. Free sign flip of z and w compensates for missing flip in inner terms.");
                source.WriteLine("denom = denom + shuffle(denom, denom, ShuffleComponent.LeftY, ShuffleComponent.LeftX, ShuffleComponent.RightW, ShuffleComponent.RightZ);   // x+y		x+y			z+w			z+w");
                source.WriteLine("denom = denom - shuffle(denom, denom, ShuffleComponent.LeftZ, ShuffleComponent.LeftZ, ShuffleComponent.RightX, ShuffleComponent.RightX);   // x+y-z-w  x+y-z-w		z+w-x-y		z+w-x-y");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var rcp_denom_ppnn = {0}4({1}) / denom;", typeName, one);
                source.WriteLine("{0}4x4 res;", typeName);
                source.WriteLine("res.c0 = minors0 * rcp_denom_ppnn;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var inner30 = shuffle(inner30_01, inner30_01, ShuffleComponent.LeftX, ShuffleComponent.LeftZ, ShuffleComponent.RightZ, ShuffleComponent.RightX);");
                source.WriteLine("var inner01 = shuffle(inner30_01, inner30_01, ShuffleComponent.LeftY, ShuffleComponent.LeftW, ShuffleComponent.RightW, ShuffleComponent.RightY);");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var minors1 = r2_wzyx * inner30 - r0_wzyx * inner23 - r3_wzyx * inner02;");
                source.WriteLine("res.c1 = minors1 * rcp_denom_ppnn;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var minors2 = r0_wzyx * inner13 - r1_wzyx * inner30 - r3_wzyx * inner01;");
                source.WriteLine("res.c2 = minors2 * rcp_denom_ppnn;");
                source.WriteLineNoTabs(string.Empty);
                source.WriteLine("var minors3 = r1_wzyx * inner02 - r0_wzyx * inner12 + r2_wzyx * inner01;");
                source.WriteLine("res.c3 = minors3 * rcp_denom_ppnn;");
                source.WriteLine("return res;");
            }
            source.Indent--;
            source.WriteLine("}");
        }
    };
}