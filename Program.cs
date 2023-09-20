using Newtonsoft.Json;
using SphereProblem.Geometry;
using SphereProblem.SphereMeshContext;

var mp = new SphereMeshParameters(default(Point3D), 1.0, 5, 5);

var manager = new SphereMeshManager(new LinearSphereMeshBuilder(mp));
var kek = manager.GetMeshInstance();
Console.WriteLine();