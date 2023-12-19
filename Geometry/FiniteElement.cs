namespace SphereProblem.Geometry;

public record FiniteElement(IReadOnlyList<int> Nodes, int AreaNumber, double Lambda)
{
    public int this[int index] => Nodes[index];
}