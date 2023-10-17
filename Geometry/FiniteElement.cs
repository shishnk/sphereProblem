namespace SphereProblem.Geometry;

public record FiniteElement(IReadOnlyList<int> Nodes, int AreaNumber = 1)
{
    public int this[int index] => Nodes[index];
}