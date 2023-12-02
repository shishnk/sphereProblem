using SphereProblem.Geometry;

namespace SphereProblem;

public class Integrator(IEnumerable<QuadratureNode<Point3D>> quadratures)
{
    public double Gauss3D(Func<Point3D, double> function, Tetrahedron element)
    {
        var vertices = element.Vertices;
        // var determinant = Math.Abs(CalculateDeterminant());
        var determinant = 1.0;

        return (from q in quadratures
            let pnt = (1.0 - q.Node.X - q.Node.Y - q.Node.Z) * vertices[0] + q.Node.X * vertices[1] +
                      q.Node.Y * vertices[2] + q.Node.Z * vertices[3]
            select function(q.Node) * q.Weight * determinant).Sum();

        double CalculateDeterminant()
        {
            var x0 = vertices[0].X;
            var y0 = vertices[0].Y;
            var z0 = vertices[0].Z;

            var x1 = vertices[1].X;
            var y1 = vertices[1].Y;
            var z1 = vertices[1].Z;

            var x2 = vertices[2].X;
            var y2 = vertices[2].Y;
            var z2 = vertices[2].Z;

            var x3 = vertices[3].X;
            var y3 = vertices[3].Y;
            var z3 = vertices[3].Z;

            return (x1 - x0) * ((y2 - y0) * (z3 - z0) - (y3 - y0) * (z2 - z0)) +
                   (y1 - y0) * ((z2 - z0) * (x3 - x0) - (z3 - z0) * (x2 - x0)) +
                   (z1 - z0) * ((x2 - x0) * (y3 - y0) - (x3 - x0) * (y2 - y0));
        }
    }
}