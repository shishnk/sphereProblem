using SphereProblem;

var mesh = new TestMeshBuilder(new(new(0.0, 1.0), 1, new(0.0, 1.0), 1, new(0.0, 1.0), 1))
    .BuildTestMesh();
FemSolver femSolver = FemSolver.CreateBuilder()
    .SetTest(("point.X", "0.0"))
    .SetAssembler(new(new LinearBasis3D(), mesh, new(Quadratures.TetrahedronOrder4())));

femSolver.Solve();