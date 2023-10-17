using SphereProblem;

var mesh = new TestMeshBuilder(new(new(0.0, 1.0), 2, new(0.0, 1.0), 2, new(0.0, 1.0), 2))
    .BuildTestMesh();
FemSolver femSolver = FemSolver.CreateBuilder()
    .SetTest(("point.X + point.Y + point.Z", "0.0"))
    .SetAssembler(new(new LinearBasis3D(), mesh, new(Quadratures.TetrahedronOrder4())))
    .SetIterativeSolver(new CGMCholesky(1000, 1E-15));

femSolver.Solve();