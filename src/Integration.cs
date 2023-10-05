using System.Diagnostics.CodeAnalysis;
using SphereProblem.Geometry;

namespace SphereProblem;

public class Integrator
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public double Gauss3D(Func<Point3D, double> function, Tetrahedron element)
    {
        var quadratures = Quadratures.TetrahedronOrder4();
        var vertices = element.Vertices;

        return (from q in quadratures
            // let pnt = (1.0 - q.Node.X - q.Node.Y - q.Node.Z) * vertices[0] + q.Node.X * vertices[1] +
                      // q.Node.Y * vertices[2] + q.Node.Z * vertices[3]
            select function(q.Node) * q.Weight).Sum();
    }
}