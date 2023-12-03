using SphereProblem;
using SphereProblem.DirichletBoundariesContext;
using SphereProblem.SphereMeshContext;

// var mesh = new TestMeshBuilder(new(
//         new(0.0, 1.0), 1,
//         new(0.0, 1.0), 1,
//         new(0.0, 1.0), 1,
//         isQuadratic: true))
//     .BuildTestMesh();
// FemSolver femSolver = FemSolver.CreateBuilder()
//     .SetTest(("point.X + point.Y + point.Z", "0.0"))
//     .SetAssembler(new(new QuadraticBasis3D(), mesh, new(Quadratures.TetrahedronOrder4())))
//     .SetIterativeSolver(new CGMCholesky(1000, 1E-15));
//
// femSolver.Solve();

// var meshParameters = SphereMeshParameters.ReadFromJsonFile("InputParameters/SphereMeshParameters.json");
// var mesh2 = new SphereMeshManager(new LinearSphereMesh3DBuilder(meshParameters)).GetMeshInstance();
// var boundaryHandler = new DirichletBoundaryHandler(meshParameters);
// var dirichletBoundaries = boundaryHandler.Handle();
//
// FemSolver femSolver2 = FemSolver.CreateBuilder()
//     .SetTest(("point.X + point.Y + point.Z", "0.0"))
//     .SetAssembler(new(new LinearBasis3D(), mesh2, new(Quadratures.TetrahedronOrder4())))
//     .SetIterativeSolver(new CGMCholesky(1000, 1E-15))
//     .SetDirichletBoundaries(dirichletBoundaries);
//
// femSolver2.Solve();

var mp = new UvSphereMeshParameters(new(0.0, 0.0, 0.0), new[] { 2.0, 5.0, 10.0 }, 4, 4);
var boundaryHandler = new UvDirichletBoundaryHandler(mp);
var builder = new LinearUvSphereMeshBuilder(mp);
var mesh2 = new UvSphereMeshManager(builder).GetMeshInstance();

FemSolver femSolver2 = FemSolver.CreateBuilder()
    // .SetTest(("point.X + point.Y + point.Z", "0.0"))
    .SetTest(p => p.X + p.Y + p.Z, p => 0.0)
    .SetAssembler(new(new LinearBasis3D(), mesh2, new(Quadratures.TetrahedronOrder4())))
    .SetIterativeSolver(new CGMCholesky(1000, 1E-15))
    .SetDirichletBoundaries(boundaryHandler.Handle());

femSolver2.Solve();
femSolver2.CalculateError();