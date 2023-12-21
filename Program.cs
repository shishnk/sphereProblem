﻿using SphereProblem;
using SphereProblem.DirichletBoundariesContext;
using SphereProblem.Geometry;
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
var meshParameters =
    new SphereMeshParameters(center: (0, 0, 0), radius: [1, 2, 3], 4,3, refinement: 3, properties: [1.0, 2.0],
        isQuadratic: true);
var mesh2 = new SphereMeshManager(new QuadraticSphereMesh3DBuilder(meshParameters)).GetMeshInstance();
var boundaryHandler = new DirichletBoundaryHandler(meshParameters, isQuadratic: true);
var dirichletBoundaries = boundaryHandler.Handle();

FemSolver femSolver2 = FemSolver.CreateBuilder()
    // .SetTest(("point.X * point.X + point.Y * point.Y + point.Z * point.Z", "-6.0"))
    .SetTestWithArea((point, areaNumber) =>
            areaNumber == 1
                ? 2.0 / 19.0 * (point.X * point.X + point.Y * point.Y + point.Z * point.Z) *
                  (point.X * point.X + point.Y * point.Y + point.Z * point.Z) +
                  28.0 / 19.0
                : 4.0 / 19.0 * (point.X * point.X + point.Y * point.Y + point.Z * point.Z) *
                  (point.X * point.X + point.Y * point.Y + point.Z * point.Z) -
                  4.0 / 19.0,
        point => -80.0 / 19.0 * (point.X * point.X + point.Y * point.Y + point.Z * point.Z))
    .SetAssembler(new(new QuadraticBasis3D(), mesh2, new(Quadratures.TetrahedronOrder4()), useLinearShapeFunc: true))
    .SetIterativeSolver(new CGMCholesky(1000, 1E-15))
    .SetDirichletBoundaries(dirichletBoundaries);

femSolver2.Solve();
// femSolver2.CalculateError();