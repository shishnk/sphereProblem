using SphereProblem.SphereMeshContext;

namespace SphereProblem.DirichletBoundariesContext;

// hack

public enum BoundaryType
{
    External,
    Internal,
    NeedExact
}

// hack
public record struct DirichletBoundary(int Node, double Value, BoundaryType Type, int AreaNumber);

/// <summary>
/// Dirichlet boundary handler.
/// Sets the first bounds on all boundaries of the region without specifying explicit parameters.
/// </summary>
/// <param name="parameters">Mesh parameters</param>
public class DirichletBoundaryHandler(SphereMeshParameters parameters, bool isQuadratic)
{
    private bool _isQuadratic = isQuadratic;
    
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

        // need bottom, back and front faces
        var set = new HashSet<DirichletBoundary>();

        // faces in spherical coordinates x -- phi, y -- r, z -- theta
        // have used symmetry (1/8 sphere) and one exact face
        // Bottom face
        var phiSplits = _isQuadratic ? parameters.PhiSplits * 2 - 1 : parameters.PhiSplits;
        var thetaSplits = _isQuadratic ? parameters.ThetaSplits * 2 - 2 : parameters.ThetaSplits;
        
        for (int i = 0; i < parameters.Radius.Count; i++)
        {
            for (int j = 0; j < phiSplits; j++)
            {
                var area = GetArea(i);
                set.Add(new(j + i * phiSplits, 0.0, BoundaryType.NeedExact, area));
            }
        }

        // Top face
        for (int i = 0; i < parameters.Radius.Count; i++)
        {
            for (int j = 0; j < phiSplits; j++)
            {
                var area = GetArea(i);
                set.Add(new(
                    j + (thetaSplits - 2) * parameters.Radius.Count * phiSplits +
                    i * phiSplits,
                    0.0, BoundaryType.NeedExact, area));
            }
        }

        // Back face
        for (int i = 0; i < thetaSplits - 1; i++)
        {
            for (int j = 0; j < phiSplits; j++)
            {
                set.Add(new(j + i * parameters.Radius.Count * phiSplits, 0.0, BoundaryType.External, 1));
            }
        }

        // Front face
        for (int i = 0; i < thetaSplits - 1; i++)
        {
            for (int j = 0; j < phiSplits; j++)
            {
                set.Add(new(
                    phiSplits * (parameters.Radius.Count - 1) + j +
                    i * parameters.Radius.Count * phiSplits, 0.0, BoundaryType.Internal, 0));
            }
        }

        // Left face
        for (int i = 0; i < thetaSplits - 1; i++)
        {
            for (int j = 0; j < parameters.Radius.Count; j++)
            {
                var area = GetArea(j);
                set.Add(new(j * phiSplits +
                            i * phiSplits * parameters.Radius.Count, 0.0, BoundaryType.NeedExact, area));
            }
        }

        // Right face
        for (int i = 0; i < thetaSplits - 1; i++)
        {
            for (int j = 0; j < parameters.Radius.Count; j++)
            {
                var area = GetArea(j);
                set.Add(new(
                    j * phiSplits + (phiSplits - 1) +
                    i * phiSplits * parameters.Radius.Count, 0.0, BoundaryType.NeedExact, area));
            }
        }

        return set.OrderBy(b => b.Node);

        int GetArea(int i)
        {
            var idx = parameters.Radius.Count - i - 1;
            return parameters.Radius[idx] > parameters.NotChangedRadius[1] ? 1 : 0; // 3 main radius, 2 areas
        }
    }
}