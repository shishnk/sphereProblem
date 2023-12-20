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
        // cartesian coordinates
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

        // var set = new HashSet<DirichletBoundary>(
        //     parameters.ThetaSplits * parameters.PhiSplits * parameters.Radius.Count);
        //
        // // faces in spherical coordinates x -- phi, y -- r, z -- theta
        //
        // // Back face
        // for (int i = 0; i < parameters.PhiSplits - 1; i++)
        // {
        //     for (int j = 0; j < parameters.ThetaSplits; j++)
        //     {
        //         set.Add(new(j + i * parameters.PhiSplits * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // set.Clear();
        //
        // // Top face
        // for (int i = 0; i < parameters.ThetaSplits - 1; i++)
        // {
        //     for (int j = 0; j < parameters.PhiSplits; j++)
        //     {
        //         set.Add(new(
        //             parameters.ThetaSplits - 1 + j * parameters.Radius.Count +
        //             i * parameters.Radius.Count * parameters.PhiSplits, 0.0));
        //     }
        // }
        //
        // set.Clear();
        //
        // // Front face
        // for (int i = 0; i < parameters.PhiSplits - 1; i++)
        // {
        //     for (int j = 0; j < parameters.ThetaSplits; j++)
        //     {
        //         set.Add(new(
        //             (parameters.PhiSplits - 1) * parameters.Radius.Count + j +
        //             i * parameters.ThetaSplits * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // set.Clear();
        //
        // // Bottom face
        // for (int i = 0; i < parameters.PhiSplits - 1; i++)
        // {
        //     for (int j = 0; j < parameters.Radius.Count; j++)
        //     {
        //         set.Add(new(j * parameters.PhiSplits + i * parameters.PhiSplits * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // set.Clear();
        //
        // // Left face
        // for (int i = 0; i < parameters.Radius.Count; i++)
        // {
        //     for (int j = 0; j < parameters.ThetaSplits; j++)
        //     {
        //         set.Add(new(j + i * parameters.ThetaSplits, 0.0));
        //     }
        // }
        //
        // set.Clear();
        //
        // // Right face
        // for (int i = 0; i < parameters.Radius.Count; i++)
        // {
        //     for (int j = 0; j < parameters.ThetaSplits; j++)
        //     {
        //         set.Add(new(
        //             parameters.ThetaSplits * parameters.Radius.Count * (parameters.PhiSplits - 2) + j +
        //             i * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // return set.OrderBy(b => b.Node);

        // var set = new HashSet<DirichletBoundary>(
        //     parameters.ThetaSplits * parameters.PhiSplits * parameters.Radius.Count);
        //
        // // faces in spherical coordinates x -- phi, y -- r, z -- theta
        //
        // // Back face
        // for (int i = 0; i < parameters.PhiSplits - 1; i++)
        // {
        //     for (int j = 0; j < parameters.ThetaSplits; j++)
        //     {
        //         set.Add(new(j + i * parameters.PhiSplits * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // set.Clear();
        //
        // // Top face
        // for (int i = 0; i < parameters.ThetaSplits - 1; i++)
        // {
        //     for (int j = 0; j < parameters.PhiSplits; j++)
        //     {
        //         set.Add(new(
        //             parameters.ThetaSplits - 1 + j * parameters.Radius.Count +
        //             i * parameters.Radius.Count * parameters.PhiSplits, 0.0));
        //     }
        // }
        //
        // set.Clear();
        //
        // // Front face
        // for (int i = 0; i < parameters.PhiSplits - 1; i++)
        // {
        //     for (int j = 0; j < parameters.ThetaSplits; j++)
        //     {
        //         set.Add(new(
        //             (parameters.PhiSplits - 1) * parameters.Radius.Count + j +
        //             i * parameters.ThetaSplits * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // set.Clear();
        //
        // // Bottom face
        // for (int i = 0; i < parameters.PhiSplits - 1; i++)
        // {
        //     for (int j = 0; j < parameters.Radius.Count; j++)
        //     {
        //         set.Add(new(j * parameters.PhiSplits + i * parameters.PhiSplits * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // set.Clear();
        //
        // // Left face
        // for (int i = 0; i < parameters.Radius.Count; i++)
        // {
        //     for (int j = 0; j < parameters.ThetaSplits; j++)
        //     {
        //         set.Add(new(j + i * parameters.ThetaSplits, 0.0));
        //     }
        // }
        //
        // set.Clear();
        //
        // // Right face
        // for (int i = 0; i < parameters.Radius.Count; i++)
        // {
        //     for (int j = 0; j < parameters.ThetaSplits; j++)
        //     {
        //         set.Add(new(
        //             parameters.ThetaSplits * parameters.Radius.Count * (parameters.PhiSplits - 2) + j +
        //             i * parameters.Radius.Count, 0.0));
        //     }
        // }
        //
        // return set.OrderBy(b => b.Node);

        var set = new HashSet<DirichletBoundary>(
            parameters.ThetaSplits * parameters.PhiSplits * parameters.Radius.Count);

        // faces in spherical coordinates x -- phi, y -- r, z -- theta

        // Bottom face
        for (int i = 0; i < parameters.Radius.Count; i++)
        {
            for (int j = 0; j < parameters.PhiSplits; j++)
            {
                set.Add(new(j + i * parameters.PhiSplits, 0.0));
            }
        }

        // Top face
        for (int i = 0; i < parameters.Radius.Count; i++)
        {
            for (int j = 0; j < parameters.PhiSplits; j++)
            {
                set.Add(new(
                    j + (parameters.ThetaSplits - 2) * parameters.Radius.Count * parameters.PhiSplits +
                    i * parameters.PhiSplits,
                    0.0));
            }
        }

        // Front face
        for (int i = 0; i < parameters.ThetaSplits - 1; i++)
        {
            for (int j = 0; j < parameters.PhiSplits; j++)
            {
                set.Add(new(
                    parameters.PhiSplits * (parameters.Radius.Count - 1) + j +
                    i * parameters.Radius.Count * parameters.PhiSplits, 0.0));
            }
        }

        // Back face
        for (int i = 0; i < parameters.ThetaSplits - 1; i++)
        {
            for (int j = 0; j < parameters.PhiSplits; j++)
            {
                set.Add(new(j + i * parameters.Radius.Count * parameters.PhiSplits, 0.0));
            }
        }

        // Left face
        for (int i = 0; i < parameters.ThetaSplits - 1; i++)
        {
            for (int j = 0; j < parameters.Radius.Count; j++)
            {
                set.Add(new(j * parameters.PhiSplits +
                            i * parameters.PhiSplits * parameters.Radius.Count, 0.0));
            }
        }

        // Right face
        for (int i = 0; i < parameters.ThetaSplits - 1; i++)
        {
            for (int j = 0; j < parameters.Radius.Count; j++)
            {
                set.Add(new(
                    j * parameters.PhiSplits + (parameters.PhiSplits - 1) +
                    i * parameters.PhiSplits * parameters.Radius.Count, 0.0));
            }
        }

        return set.OrderBy(b => b.Node);
    }
}