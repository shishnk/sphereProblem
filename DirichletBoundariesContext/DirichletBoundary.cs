using SphereProblem.SphereMeshContext;

namespace SphereProblem.DirichletBoundariesContext;

public record struct DirichletBoundary(int Node, double Value);

/// <summary>
/// Dirichlet boundary handler.
/// Sets the first bounds on all boundaries of the region without specifying explicit parameters.
/// </summary>
/// <param name="parameters">Mesh parameters</param>
public class DirichletBoundaryHandler(SphereMeshParameters parameters)
{
    /// <summary>
    /// Process dirichlet boundaries
    /// </summary>
    public IEnumerable<DirichletBoundary> Handle()
    {
        // var set = new HashSet<DirichletBoundary>(
        //     parameters.ThetaSplits * parameters.PhiSplits * parameters.Radius.Count);
        //
        // // Bottom face
        // for (int j = 0; j < parameters.Radius.Count; j++)
        // {
        //     for (int i = 0; i < parameters.ThetaSplits; i++)
        //     {
        //         set.Add(new(i + j * parameters.ThetaSplits, 0.0));
        //     }
        // }
        //
        // // Top face
        // for (int j = 0; j < parameters.Radius.Count; j++)
        // {
        //     for (int i = 0; i < parameters.ThetaSplits; i++)
        //     {
        //         set.Add(new(
        //             parameters.ThetaSplits * parameters.Radius.Count * (parameters.PhiSplits - 1) + i +
        //             j * parameters.ThetaSplits, 0.0));
        //     }
        // }
        //
        // // Left face
        // for (int j = 0; j < parameters.PhiSplits; j++)
        // {
        //     for (int i = 0; i < parameters.Radius.Count; i++)
        //     {
        //         set.Add(new(i * parameters.ThetaSplits + j * parameters.ThetaSplits * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // // Right face
        // for (int j = 0; j < parameters.PhiSplits; j++)
        // {
        //     for (int i = 0; i < parameters.Radius.Count; i++)
        //     {
        //         set.Add(new(i * parameters.ThetaSplits + j * parameters.ThetaSplits * parameters.Radius.Count +
        //             parameters.ThetaSplits - 1, 0.0));
        //     }
        // }
        //
        // // Front face
        // for (int j = 0; j < parameters.PhiSplits; j++)
        // {
        //     for (int i = 0; i < parameters.ThetaSplits; i++)
        //     {
        //         set.Add(new(i + j * parameters.ThetaSplits * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // // Back face
        // for (int j = 0; j < parameters.PhiSplits; j++)
        // {
        //     for (int i = 0; i < parameters.ThetaSplits; i++)
        //     {
        //         set.Add(new(
        //             parameters.ThetaSplits * (parameters.Radius.Count - 1) + i +
        //             j * parameters.ThetaSplits * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // return set.OrderBy(b => b.Node);

        var skip = parameters.Radius.Count;
        var capacity = parameters.ThetaSplits * (parameters.PhiSplits - 1) * 2;
        var list = new List<DirichletBoundary>
        {
            new(0, 0.0),
            new(parameters.Radius.Count - 1, 0.0)
        };

        for (int i = 0; i < capacity / 2; i++)
        {
            list.Add(new(skip + i, 0.0));
            list.Add(new(parameters.ThetaSplits * (parameters.PhiSplits - 1) * (parameters.Radius.Count - 1) + i + skip,
                0.0));
        }

        return list.OrderBy(b => b.Node);
    }
}