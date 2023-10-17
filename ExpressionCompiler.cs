﻿using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SphereProblem.Geometry;

namespace SphereProblem;

public static class ExpressionCompiler
{
    private static readonly ScriptOptions _options;

    static ExpressionCompiler() => _options = ScriptOptions.Default.AddReferences(typeof(Point3D).Assembly);

    public static Func<Point3D, double> CompileToLambda(string expression) =>
        CSharpScript.EvaluateAsync<Func<Point3D, double>>("point => " + expression, _options).Result;
}