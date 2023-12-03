using SphereProblem.SphereMeshContext;

namespace SphereProblem.DirichletBoundariesContext;

public record struct DirichletBoundary(int Node, double Value);

/// <summary>
/// Dirichlet boundary handler.
/// Sets the first bounds on all boundaries of the region without specifying explicit parameters.
/// </summary>
/// <param name="parameters">Mesh parameters</param>
[Obsolete("Use UvDirichletBoundaryHandler instead")]
public class DirichletBoundaryHandler(SphereMeshParameters parameters)
{
    /// <summary>
    /// Process dirichlet boundaries
    /// </summary>
    public IEnumerable<DirichletBoundary> Handle()
    {
        var set = new HashSet<DirichletBoundary>(
            parameters.ThetaSplits * parameters.PhiSplits * parameters.Radius.Count);

        // Bottom face
        for (int j = 0; j < parameters.Radius.Count; j++)
        {
            for (int i = 0; i < parameters.ThetaSplits; i++)
            {
                set.Add(new(i + j * parameters.ThetaSplits, 0.0));
            }
        }

        // Top face
        for (int j = 0; j < parameters.Radius.Count; j++)
        {
            for (int i = 0; i < parameters.ThetaSplits; i++)
            {
                set.Add(new(
                    parameters.ThetaSplits * parameters.Radius.Count * (parameters.PhiSplits - 1) + i +
                    j * parameters.ThetaSplits, 0.0));
            }
        }

        // Left face
        for (int j = 0; j < parameters.PhiSplits; j++)
        {
            for (int i = 0; i < parameters.Radius.Count; i++)
            {
                set.Add(new(i * parameters.ThetaSplits + j * parameters.ThetaSplits * parameters.Radius.Count, 0.0));
            }
        }

        // Right face
        for (int j = 0; j < parameters.PhiSplits; j++)
        {
            for (int i = 0; i < parameters.Radius.Count; i++)
            {
                set.Add(new(i * parameters.ThetaSplits + j * parameters.ThetaSplits * parameters.Radius.Count +
                    parameters.ThetaSplits - 1, 0.0));
            }
        }

        // Front face
        for (int j = 0; j < parameters.PhiSplits; j++)
        {
            for (int i = 0; i < parameters.ThetaSplits; i++)
            {
                set.Add(new(i + j * parameters.ThetaSplits * parameters.Radius.Count, 0.0));
            }
        }

        // Back face
        for (int j = 0; j < parameters.PhiSplits; j++)
        {
            for (int i = 0; i < parameters.ThetaSplits; i++)
            {
                set.Add(new(
                    parameters.ThetaSplits * (parameters.Radius.Count - 1) + i +
                    j * parameters.ThetaSplits * parameters.Radius.Count, 0.0));
            }
        }

        return set.OrderBy(b => b.Node);
    }
}

public class UvDirichletBoundaryHandler(UvSphereMeshParameters parameters)
{
    /// <summary>
    /// Process dirichlet boundaries
    /// </summary>
    public IEnumerable<DirichletBoundary> Handle()
    {
        var nodesCount = (parameters.StackSplits - 1) * parameters.SliceSplits + 2;
        var set = new List<DirichletBoundary>(nodesCount * 2); // internal sphere + external sphere

        for (int i = 0; i < nodesCount; i++)
        {
            set.Add(new(i, 0.0));
            set.Add(new(i + (parameters.Radius.Count - 1) * nodesCount, 0.0));
        }

        return set.OrderBy(b => b.Node);
    }
}