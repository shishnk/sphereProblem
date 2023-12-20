using SphereProblem.DirichletBoundariesContext;
using SphereProblem.Geometry;

namespace SphereProblem;

public class FemSolver
{
    public class FemSolverBuilder
    {
        private readonly FemSolver _solverFem = new();

        public FemSolverBuilder SetAssembler(SystemAssembler systemAssembler)
        {
            _solverFem._assembler = systemAssembler;
            return this;
        }

        // public FemSolverBuilder SetTest((string FieldExpr, string SourceExpr) test)
        // {
        //     _solverFem._u = ExpressionCompiler.CompileToLambda(test.FieldExpr);
        //     _solverFem._f = ExpressionCompiler.CompileToLambda(test.SourceExpr);
        //     return this;
        // }

        public FemSolverBuilder SetTestWithArea(Func<Point3D, int, double> field, Func<Point3D, double> source)
        {
            _solverFem._u = field;
            _solverFem._f = source;
            return this;
        }

        public FemSolverBuilder SetIterativeSolver(IterativeSolver iterativeSolver)
        {
            _solverFem._iterativeSolver = iterativeSolver;
            return this;
        }

        public FemSolverBuilder SetDirichletBoundaries(IEnumerable<DirichletBoundary> boundaries)
        {
            _solverFem._boundaries = boundaries;
            return this;
        }

        public static implicit operator FemSolver(FemSolverBuilder builder)
            => builder._solverFem;
    }

    private IterativeSolver _iterativeSolver = null!;
    private SystemAssembler _assembler = null!;
    private Func<Point3D, double> _f = null!;
    private Func<Point3D, int, double> _u = null!;
    private IEnumerable<DirichletBoundary> _boundaries = null!;

    public static FemSolverBuilder CreateBuilder() => new();

    public void Solve()
    {
        EnsureInitialization();
        AssemblyInternal();

        // var exact = new Vector<double>(_assembler.Mesh.Points.Count);
        // for (int i = 0; i < _assembler.Mesh.Points.Count; i++)
        // {
        //     exact[i] = _assembler.Mesh.Points[i].X;
        // }
        // exact.Fill(1.0);
        //
        // var t = _assembler.GlobalMatrix! * exact;

        AccountingDirichletBoundary();

        _iterativeSolver.SetMatrixEx(_assembler.GlobalMatrix!).SetVectorEx(_assembler.Vector);
        _iterativeSolver.Compute();
        //
        // for (int i = 0; i < _iterativeSolver.Solution!.Value.Length; i++)
        // {
        //     Console.WriteLine(
        //         $" {i} Point {_assembler.Mesh.Points[i]} -- {_iterativeSolver.Solution.Value[i]}, exact -- {_u(_assembler.Mesh.Points[i])}");
        // }

        var exact = (from element in _assembler.Mesh.Elements
            from node in element.Nodes
            select (_u(_assembler.Mesh.Points[node], element.AreaNumber), node)).ToList();

        exact = exact.DistinctBy(tuple => tuple.Item2).OrderBy(tuple => tuple.Item2).ToList();

        var approx = _iterativeSolver.Solution!.Value.ToList();

        var result = exact.Zip(approx, (v1, v2) => (v2, v1.Item1));

        foreach (var (v1, v2) in result)
        {
            Console.WriteLine($"{v1} ------------ {v2} ");
        }

        Console.WriteLine("---------------------------");

        // CalculateError();
        CalculateErrorWithBreaking(approx, exact.Select(tuple => tuple.Item1).ToList());
    }

    // public void CalculateError()
    // {
    //     var exactValues = _assembler.Mesh.Points.Select(p => _u(p)).ToArray();
    //     var errors = exactValues.Select((v, i) => Math.Abs(v - _iterativeSolver.Solution!.Value[i])).ToArray();
    //     Console.WriteLine($"RMS = {Math.Sqrt(errors.Sum(e => e * e) / errors.Length)}");
    // }

    private void EnsureInitialization()
    {
        PortraitBuilder.Build(_assembler.Mesh, out var ig, out var jg);
        _assembler.GlobalMatrix = new(ig.Length - 1, jg.Length)
        {
            Ig = ig,
            Jg = jg
        };
    }

    private void AssemblyInternal()
    {
        for (int ielem = 0; ielem < _assembler.Mesh.Elements.Count; ielem++)
        {
            var element = _assembler.Mesh.Elements[ielem];
            var lambda = element.Lambda;

            _assembler.AssemblyLocalMatrices(ielem);
            _assembler.AssemblyVector(ielem, _f);

            for (int i = 0; i < _assembler.Basis.Size; i++)
            {
                for (int j = 0; j < _assembler.Basis.Size; j++)
                {
                    _assembler.FillGlobalMatrix(element[i], element[j],
                        lambda * _assembler.StiffnessMatrix[i, j]);
                }
            }
        }
    }

    private void AccountingDirichletBoundary()
    {
        Span<int> checkBc = stackalloc int[_assembler.Mesh.Points.Count];
        checkBc.Fill(-1);
        var boundariesArray = _boundaries.ToArray();

        for (int i = 0; i < boundariesArray.Length; i++)
        {
            checkBc[boundariesArray[i].Node] = i;
            boundariesArray[i].Value = boundariesArray[i].Type switch
            {
                BoundaryType.External => 10.0,
                BoundaryType.Internal => 0.0,
                BoundaryType.NeedExact => _u(_assembler.Mesh.Points[boundariesArray[i].Node],
                    boundariesArray[i].AreaNumber),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        // for (int i = 0, k = boundariesArray.Length - 1;
        //      i < boundariesArray.Length / 2;
        //      i++, k--)
        // {
        //     boundariesArray[i].Value = 10.0;
        //     boundariesArray[k].Value = 0.0;
        // }


        for (int i = 0; i < _assembler.Mesh.Points.Count; i++)
        {
            int index;

            if (checkBc[i] != -1)
            {
                _assembler.GlobalMatrix!.Di[i] = 1.0;
                _assembler.Vector[i] = boundariesArray[checkBc[i]].Value;

                for (int k = _assembler.GlobalMatrix.Ig[i]; k < _assembler.GlobalMatrix.Ig[i + 1]; k++)
                {
                    index = _assembler.GlobalMatrix.Jg[k];

                    if (checkBc[index] == -1)
                    {
                        _assembler.Vector[index] -= _assembler.GlobalMatrix.Gg[k] * _assembler.Vector[i];
                    }

                    _assembler.GlobalMatrix.Gg[k] = 0.0;
                }
            }
            else
            {
                for (int k = _assembler.GlobalMatrix!.Ig[i]; k < _assembler.GlobalMatrix.Ig[i + 1]; k++)
                {
                    index = _assembler.GlobalMatrix.Jg[k];

                    if (checkBc[index] == -1) continue;

                    _assembler.Vector[i] -= _assembler.GlobalMatrix.Gg[k] * _assembler.Vector[index];
                    _assembler.GlobalMatrix.Gg[k] = 0.0;
                }
            }
        }
    }

    private void CalculateErrorWithBreaking(IReadOnlyList<double> approx, IReadOnlyList<double> exact)
    {
        var error = new double[approx.Count];

        for (int i = 0; i < error.Length; i++)
        {
            error[i] = Math.Abs(approx[i] - exact[i]);
        }

        // Array.ForEach(error, Console.WriteLine);

        var sum = error.Sum(t => t * t);

        sum = Math.Sqrt(sum / _assembler.Mesh.Points.Count);

        Console.WriteLine($"rms = {sum}");
    }
}