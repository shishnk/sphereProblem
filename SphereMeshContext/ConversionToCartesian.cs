using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

public static class ConversionToCartesian // TODO: mb don't need
{
    public static void Convert(IEnumerable<Point3D> points, SphereMeshParameters parameters,
        out IReadOnlyList<Point3D> outPoints)
    {
        outPoints = points.Select<Point3D, Point3D>(p =>
        {
            var x = parameters.Radius * Math.Sin(p.Z) * Math.Cos(p.Y) + parameters.Center.X;
            var y = parameters.Radius * Math.Sin(p.Z) * Math.Sin(p.Y) + parameters.Center.Y;
            var z = parameters.Radius * Math.Cos(p.Z) + parameters.Center.Z;

            return (x, y, z);
        }).ToList();
    }
}