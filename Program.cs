﻿using System.Text.Json;
using SphereProblem;
using SphereProblem.DirichletBoundariesContext;
using SphereProblem.SphereMeshContext;

// var mesh = new TestMeshBuilder(new(
//         new(0.0, 3.0), 2,
//         new(0.0, 3.0), 2,
//         new(0.0, 3.0), 2,
//         isQuadratic: false))
//     .BuildTestMesh();
// FemSolver femSolver = FemSolver.CreateBuilder()
//     .SetTest(("point.X + point.Y + point.Z", "0.0"))
//     .SetAssembler(new(new LinearBasis3D(), mesh, new(Quadratures.TetrahedronOrder4())))
//     .SetIterativeSolver(new CGMCholesky(1000, 1E-15));
//
// femSolver.Solve();
// femSolver.CalculateError();

// var meshParameters = SphereMeshParameters.ReadFromJsonFile("InputParameters/SphereMeshParameters.json");
var meshParameters = new SphereMeshParameters((0, 0, 0), [1, 2, 3], 3, 3, 0);
var mesh2 = new SphereMeshManager(new LinearSphereMesh3DBuilder(meshParameters)).GetMeshInstance();
var boundaryHandler = new DirichletBoundaryHandler(meshParameters);
var dirichletBoundaries = boundaryHandler.Handle();

FemSolver femSolver2 = FemSolver.CreateBuilder()
    .SetTest(("point.X + point.Y + point.Z", "0.0"))
    .SetAssembler(new(new LinearBasis3D(), mesh2, new(Quadratures.TetrahedronOrder4())))
    .SetIterativeSolver(new CGMCholesky(1000, 1E-15))
    .SetDirichletBoundaries(dirichletBoundaries);

femSolver2.Solve();
femSolver2.CalculateError();