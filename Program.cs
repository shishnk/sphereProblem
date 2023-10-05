using Newtonsoft.Json;
using SphereProblem;
using SphereProblem.Geometry;
using SphereProblem.SphereMeshContext;

var integrator = new Integrator();

var templateElement = new Tetrahedron((0.0, 0.0, 0.0), (1.0, 0.0, 0.0), (0.0, 1.0, 0.0), (0.0, 0.0, 1.0));
var result = integrator.Gauss3D(_ => 1.0, templateElement);
Console.WriteLine(result);
var mesh = new SphereMesh(templateElement.Vertices, new[] { new FiniteElement(new[] { 0, 1, 2, 3 }) });
var assembler = new SystemAssembler(new LinearBasis3D(), mesh, integrator);
// assembler.AssemblyLocalMatrix(0);