using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

public class SphereMesh(IReadOnlyList<Point3D> nodes, IReadOnlyList<FiniteElement> elements)
{
    public IReadOnlyList<Point3D> Points => nodes;
    public IReadOnlyList<FiniteElement> Elements => elements;
}