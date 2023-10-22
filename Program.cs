using SphereProblem;

var mesh = new TestMeshBuilder(new(
        new(0.0, 1.0), 1, 
        new(0.0, 1.0), 1, 
        new(0.0, 1.0), 1, 
        isQuadratic:true))
    .BuildTestMesh();
FemSolver femSolver = FemSolver.CreateBuilder()
    .SetTest(("point.X + point.Y + point.Z", "0.0"))
    .SetAssembler(new(new QuadraticBasis3D(), mesh, new(Quadratures.TetrahedronOrder4())))
    .SetIterativeSolver(new CGMCholesky(1000, 1E-15));

femSolver.Solve();