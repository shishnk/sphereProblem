namespace SphereProblem;

public class FemSolver
{
    public class FemSolverBuilder
    {
        private readonly FemSolver _solverFem = new();

        public FemSolverBuilder SetSolverSlae(IterativeSolver iterativeSolver)
        {
            _solverFem._iterativeSolver = iterativeSolver;
            return this;
        }

        public FemSolverBuilder SetAssembler(SystemAssembler systemAssembler)
        {
            _solverFem._assembler = systemAssembler;
            return this;
        }

        public static implicit operator FemSolver(FemSolverBuilder builder)
            => builder._solverFem;
    }

    private IterativeSolver? _iterativeSolver = default!;
    private SystemAssembler _assembler = default!;

    public static FemSolverBuilder CreateBuilder() => new();
}