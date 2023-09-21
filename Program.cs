using Newtonsoft.Json;
using SphereProblem.Geometry;
using SphereProblem.SphereMeshContext;

var manager = new SphereMeshManager(new QuadraticSphereMeshBuilder(SphereMeshParameters.ReadFromJsonFile("InputParameters/SphereMeshParameters.json")));
var kek = manager.GetMeshInstance();
Console.WriteLine();