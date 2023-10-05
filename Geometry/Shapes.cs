namespace SphereProblem.Geometry;

public class Triangle(Point3D v1, Point3D v2, Point3D v3)
{
    public Point3D V1 { get; } = v1;
    public Point3D V2 { get; } = v2;
    public Point3D V3 { get; } = v3;

    public IReadOnlyList<Point3D> Vertices => new[] { v1, v2, v3 };
}

public class Tetrahedron(Point3D v1, Point3D v2, Point3D v3, Point3D v4)
{
    public Point3D V1 { get; } = v1;
    public Point3D V2 { get; } = v2;
    public Point3D V3 { get; } = v3;
    public Point3D V4 { get; } = v4;

    public IReadOnlyList<Point3D> Vertices => new[] { v1, v2, v3, v4 };
}