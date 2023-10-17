using SphereProblem.Geometry;

namespace SphereProblem;

public class QuadratureNode<T>(T node, double weight)
    where T : notnull
{
    public T Node { get; } = node;
    public double Weight { get; } = weight;
}

public static class Quadratures
{
    public static IEnumerable<QuadratureNode<Point3D>> TetrahedronOrder4()
    {
        double[] p1 = { 1.0 / 4.0, 1.0 / 2.0, 1.0 / 6.0, 1.0 / 6.0, 1.0 / 6.0 };
        double[] p2 = { 1.0 / 4.0, 1.0 / 6.0, 1.0 / 2.0, 1.0 / 6.0, 1.0 / 6.0 };
        double[] p3 = { 1.0 / 4.0, 1.0 / 6.0, 1.0 / 6.0, 1.0 / 2.0, 1.0 / 6.0 };
        double[] w = { -4.0 / 5.0, 9.0 / 20.0, 9.0 / 20.0, 9.0 / 20.0, 9.0 / 20.0 };

        for (int i = 0; i < w.Length; i++)
        {
            yield return new((p1[i], p2[i], p3[i]), w[i] / 6.0);
        }
    }
}       