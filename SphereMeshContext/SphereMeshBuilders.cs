using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

public abstract class BaseSphereMeshBuilder(SphereMeshParameters parameters)
{
    private SphereMesh? _sphereMesh;
    protected Point3D[] _points = null!;
    protected List<FiniteElement> _elements = null!;
    protected List<int[]>? _parallelepipeds; // for python
    protected List<int[]>? _prisms; // for python

    public abstract void CreatePoints();
    public abstract void CreateElements();

    // TODO: change theta and phi places
    protected void CreatePointsInner(int thetaSplits, int phiSplits)
    {
        _points = new Point3D[(phiSplits - 1) * thetaSplits * parameters.Radius.Count];

        var phi = Math.PI / 4 / (phiSplits - 1);
        var theta = Math.PI / 2 / (thetaSplits - 1);

        // for (int t = parameters.Radius.Count - 1, idx = 0; t >= 0; t--, idx++)
        // {
        //     _points[idx] = parameters.Center + parameters.Radius[t] * Point3D.UnitZ;
        // }

        for (int i = 1, idx = 0; i < phiSplits; i++)
        {
            for (int k = parameters.Radius.Count - 1; k >= 0; k--)
            {
                var r = parameters.Radius[k];

                for (int j = 0; j < thetaSplits; j++, idx++)
                {
                    var x = r * Math.Sin(phi * i) * Math.Cos(j * theta) + parameters.Center.X;
                    var y = r * Math.Sin(phi * i) * Math.Sin(j * theta) + parameters.Center.Y;
                    var z = r * Math.Cos(phi * i) + parameters.Center.Z;

                    // var x = theta * j;
                    // var y = r;
                    // var z = phi * i;

                    _points[idx] = (x, y, z);
                }
            }
        }
    }

    public SphereMesh GetMeshInstance()
    {
        WriteToFile();
        return _sphereMesh ??= new(_points, _elements);
    }

    protected virtual void WriteToFile()
    {
        using StreamWriter sw1 = new("../../../SphereMeshContext/Python/points"),
            sw2 = new("../../../SphereMeshContext/Python/elements");

        foreach (var point in _points)
        {
            sw1.WriteLine(point.ToString());
        }

        foreach (var element in _elements)
        {
            for (var i = 0; i < 3; i++)
            {
                sw2.Write(element.Nodes[i] + " ");
            }

            sw2.WriteLine();
        }
    }
}

// public class LinearSphereMesh2DBuilder(SphereMeshParameters parameters) : BaseSphereMeshBuilder(parameters)
// {
//     public override void CreatePoints() => CreatePointsInner(parameters.PhiSplits, parameters.ThetaSplits);
//
//     public override void CreateElements()
//     {
//         // _elements = new FiniteElement[2 * (parameters.PhiSplits - 1) * (parameters.ThetaSplits - 1)];
//         _elements = [];
//
//         for (int i = 0, idx = 0; i < parameters.PhiSplits - 1; i++)
//         {
//             for (int j = 0; j < parameters.ThetaSplits - 1; j++, idx += 2)
//             {
//                 var v1 = i * parameters.ThetaSplits + j;
//                 var v2 = i * parameters.ThetaSplits + j + 1;
//                 var v3 = (i + 1) * parameters.ThetaSplits + j;
//                 var v4 = (i + 1) * parameters.ThetaSplits + j + 1;
//
//                 _elements[idx] = new(new[] { v1, v3, v2 }, 1, 1);
//                 _elements[idx + 1] = new(new[] { v4, v2, v3 }, 1, 1);
//             }
//         }
//     }
// }

// public class QuadraticSphereMesh2DBuilder(SphereMeshParameters parameters) : BaseSphereMeshBuilder(parameters)
// {
//     public override void CreatePoints() =>
//         CreatePointsInner(2 * parameters.PhiSplits - 1, 2 * parameters.ThetaSplits - 1);
//
//     public override void CreateElements()
//     {
//         const int elementSize = 6;
//         const int localSize = 3;
//         // _elements = new FiniteElement[2 * (parameters.PhiSplits - 1) * (parameters.ThetaSplits - 1)];
//         _elements = [];
//         Span<int> nodes = stackalloc int[elementSize + localSize];
//
//         for (int j = 0, idx = 0; j < parameters.PhiSplits - 1; j++)
//         {
//             for (int i = 0; i < parameters.ThetaSplits - 1; i++, idx += 2)
//             {
//                 for (int k = 0; k < nodes.Length; k++)
//                 {
//                     var lx = k % localSize;
//                     var ly = k / localSize;
//
//                     var ox = i * (localSize - 1) + lx;
//                     var oy = ly * ((localSize - 1) * parameters.ThetaSplits - 1) +
//                              j * ((localSize - 1) * parameters.ThetaSplits - 1) * (localSize - 1);
//
//                     nodes[k] = ox + oy;
//                 }
//
//                 _elements[idx] = new(new[] { nodes[0], nodes[6], nodes[2], nodes[3], nodes[4], nodes[1] });
//                 _elements[idx + 1] = new(new[] { nodes[8], nodes[2], nodes[6], nodes[5], nodes[4], nodes[7] });
//             }
//         }
//     }
// }

public class LinearSphereMesh3DBuilder(SphereMeshParameters parameters) : BaseSphereMeshBuilder(parameters)
{
    public override void CreatePoints() => CreatePointsInner(parameters.PhiSplits, parameters.ThetaSplits);

    public override void CreateElements()
    {
        const int parallelepipedSize = 8;
        const int prismSize = 6;
        // _elements = new FiniteElement[(parameters.PhiSplits - 1) * (parameters.ThetaSplits - 1) *
        // (parameters.Radius.Count - 1) * 3 + (parameters.Radius.Count - 1) * (parameters.ThetaSplits - 1) * 3];
        _elements = [];
        _parallelepipeds = [];
        _prisms = [];
        Span<int> parallelepipedNodes = stackalloc int[parallelepipedSize];
        Span<int> prismNodes = stackalloc int[prismSize];

        for (var i = 0; i < _parallelepipeds.Count; i++)
        {
            _parallelepipeds[i] = new int[parallelepipedSize];
        }

        int skip = 0;
        var nx = parameters.PhiSplits;
        var ny = parameters.Radius.Count;

        for (int k = 0; k < parameters.ThetaSplits - 2; k++)
        {
            for (int j = 0; j < parameters.Radius.Count - 1; j++)
            {
                for (int i = 0; i < parameters.PhiSplits - 1; i++)
                {
                    // + skip == skip prizm points
                    // parallelepipedNodes[0] = i + (j + 1) * nx + k * nx * ny + skip + j * nx;
                    // parallelepipedNodes[1] = i + 1 + (j + 1) * nx + k * nx * ny + skip + j * nx;
                    // parallelepipedNodes[2] = i + j * nx + (k + 1) * nx * ny + skip + j * nx;
                    // parallelepipedNodes[3] = i + 1 + j * nx + (k + 1) * nx * ny + skip + j * nx;
                    // parallelepipedNodes[4] = i + j * nx + k * nx * ny + skip + j * nx;
                    // parallelepipedNodes[5] = i + 1 + j * nx + k * nx * ny + skip + j * nx;
                    // parallelepipedNodes[6] = parallelepipedNodes[2] - skip;
                    // parallelepipedNodes[7] = parallelepipedNodes[3] - skip;

                    parallelepipedNodes[0] = i + (j + 1) * nx + (k + 1) * nx * ny + skip;
                    parallelepipedNodes[1] = i + 1 + (j + 1) * nx + (k + 1) * nx * ny + skip;
                    parallelepipedNodes[2] = i + (j + 1) * nx + k * nx * ny + skip;
                    parallelepipedNodes[3] = i + 1 + (j + 1) * nx + k * nx * ny + skip;
                    parallelepipedNodes[4] = i + j * nx + (k + 1) * nx * ny + skip;
                    parallelepipedNodes[5] = i + 1 + j * nx + (k + 1) * nx * ny + skip;
                    parallelepipedNodes[6] = i + j * nx + k * nx * ny + skip;
                    parallelepipedNodes[7] = i + 1 + j * nx + k * nx * ny + skip;

                    _parallelepipeds.Add(parallelepipedNodes.ToArray());

                    var areaNumber = GetArea(j);
                    var lambda = parameters.Properties[areaNumber];

                    _elements.Add(new([
                        parallelepipedNodes[0], parallelepipedNodes[1], parallelepipedNodes[2], parallelepipedNodes[6]
                    ], areaNumber, lambda));
                    _elements.Add(new([
                        parallelepipedNodes[0], parallelepipedNodes[1], parallelepipedNodes[6], parallelepipedNodes[4]
                    ], areaNumber, lambda));
                    _elements.Add(new([
                        parallelepipedNodes[1], parallelepipedNodes[3], parallelepipedNodes[2], parallelepipedNodes[6]
                    ], areaNumber, lambda));
                    _elements.Add(new([
                        parallelepipedNodes[1], parallelepipedNodes[7], parallelepipedNodes[6], parallelepipedNodes[5]
                    ], areaNumber, lambda));
                    _elements.Add(new([
                        parallelepipedNodes[1], parallelepipedNodes[3], parallelepipedNodes[6], parallelepipedNodes[7]
                    ], areaNumber, lambda));
                    _elements.Add(new([
                        parallelepipedNodes[1], parallelepipedNodes[6], parallelepipedNodes[4], parallelepipedNodes[5]
                    ], areaNumber, lambda));
                }
            }
        }
        // creating prisms, may be later (working algorithm)
        // for (int i = 0; i < parameters.Radius.Count - 1; i++)
        // {
        //     for (int j = 0; j < parameters.PhiSplits - 1; j++)
        //     {
        //         prismNodes[0] = j + i * nx + skip;
        //         prismNodes[1] = j + 1 + i * nx + skip;
        //         prismNodes[2] = i;
        //         prismNodes[3] = j + (i + 1) * nx + skip;
        //         prismNodes[4] = j + 1 + (i + 1) * nx + skip;
        //         prismNodes[5] = i + 1;
        //
        //         _prisms.Add(prismNodes.ToArray());
        //
        //         areaNumber = GetArea(i);
        //         lambda = parameters.Properties[areaNumber];
        //
        //         _elements.Add(new([prismNodes[3], prismNodes[4], prismNodes[5], prismNodes[1]], areaNumber, lambda));
        //         _elements.Add(new([prismNodes[3], prismNodes[1], prismNodes[5], prismNodes[2]], areaNumber, lambda));
        //         _elements.Add(new([prismNodes[0], prismNodes[1], prismNodes[2], prismNodes[3]], areaNumber, lambda));
        //     }
        // }

        return;

        int GetArea(int i)
        {
            var idx = parameters.Radius.Count - i - 1;
            return parameters.Radius[idx] > parameters.NotChangedRadius[1] ? 1 : 0; // 3 main radius, 2 areas
        }
    }

    protected override void WriteToFile()
    {
        using StreamWriter sw1 = new("../../../SphereMeshContext/Python/points"),
            sw2 = new("../../../SphereMeshContext/Python/prisms"),
            sw3 = new("../../../SphereMeshContext/Python/parallelepipeds"),
            sw4 = new("../../../SphereMeshContext/Python/elements");

        foreach (var point in _points)
        {
            sw1.WriteLine(point.ToString());
        }

        foreach (var node in _prisms!)
        {
            foreach (var i in node)
            {
                sw2.Write(i + " ");
            }

            sw2.WriteLine();
        }

        foreach (var node in _parallelepipeds!)
        {
            foreach (var i in node)
            {
                sw3.Write(i + " ");
            }

            sw3.WriteLine();
        }

        foreach (var element in _elements)
        {
            foreach (var node in element.Nodes)
            {
                sw4.Write(node + " ");
            }

            sw4.Write(element.AreaNumber);
            sw4.WriteLine();
        }
    }
}

public class QuadraticSphereMesh3DBuilder(SphereMeshParameters parameters) : BaseSphereMeshBuilder(parameters)
{
    public override void CreatePoints() =>
        CreatePointsInner(2 * parameters.PhiSplits - 1, 2 * parameters.ThetaSplits - 2);

    public override void CreateElements()
    {
        const int localSize = 3;
        const int parallelepipedSize = 27;
        // const int prismSize = 14;
        // _elements = new FiniteElement[(parameters.PhiSplits - 1) * (parameters.ThetaSplits - 1) *
        // (parameters.Radius.Count - 1) * 3 + (parameters.Radius.Count - 1) * (parameters.ThetaSplits - 1) * 3];
        _elements = [];
        _parallelepipeds = [];
        // _prisms = [];
        Span<int> parallelepipedNodes = stackalloc int[parallelepipedSize];
        // Span<int> prismNodes = stackalloc int[prismSize];

        for (var i = 0; i < _parallelepipeds.Count; i++)
        {
            _parallelepipeds[i] = new int[parallelepipedSize];
        }

        var nx = parameters.PhiSplits - 1;
        var ny = parameters.Radius.Count / (localSize - 1);
        var yPadding = nx * (localSize - 1) + 1;
        var zPadding = (nx * (localSize - 1) + 1) * (ny * (localSize - 1) + 1);

        for (int k = 0; k < parameters.ThetaSplits - 2; k++)
        {
            for (int j = 0; j < ny; j++)
            {
                for (int i = 0; i < parameters.PhiSplits - 1; i++)
                {
                    var ox = i * (localSize - 1);
                    var oy = yPadding * (localSize - 1) * j;
                    var oz = zPadding * (localSize - 1) * k;

                    var op = ox + oy + oz;

                    for (int c = 0; c < parallelepipedSize; c++)
                    {
                        var lx = c % localSize;
                        var ly = c / localSize % localSize;
                        var lz = c / (localSize * localSize);

                        parallelepipedNodes[c] = op + lx + ly * yPadding + lz * zPadding;
                    }

                    _parallelepipeds.Add([
                        parallelepipedNodes[0], parallelepipedNodes[2], parallelepipedNodes[6], parallelepipedNodes[8],
                        parallelepipedNodes[18], parallelepipedNodes[20], parallelepipedNodes[24],
                        parallelepipedNodes[26]
                    ]);

                    // var p1 = _points[parallelepipedNodes[0]];
                    // var p2 = _points[parallelepipedNodes[26]];
                    // var area = GetArea((p1.Z + p2.Z) / 2);
                    var area = GetArea(j);
                    var lambda = parameters.Properties[area];

                    //
                    // // you should have flipped
                    // int[] newOrder =
                    // [
                    //     24, 25, 26, 15, 16, 17, 6, 7, 8, 21, 22, 23, 12, 13, 14, 3, 4, 5, 18, 19, 20, 9, 10, 11, 0, 1, 2
                    // ];
                    // var tempNodes = new int[27];
                    //
                    // for (int idx = 0; idx < newOrder.Length; idx++)
                    // {
                    //     tempNodes[idx] = parallelepipedNodes[newOrder[idx]];
                    // }
                    //
                    // parallelepipedNodes = tempNodes;

                    // _elements.Add(new(new[]
                    // {
                    //     // 0, 2, 6, 24 -- vertices
                    //     parallelepipedNodes[0], parallelepipedNodes[2], parallelepipedNodes[6], parallelepipedNodes[24],
                    //     parallelepipedNodes[1], parallelepipedNodes[3], parallelepipedNodes[12], parallelepipedNodes[4],
                    //     parallelepipedNodes[13], parallelepipedNodes[15]
                    // }, area, lambda));
                    // _elements.Add(new(new[]
                    // {
                    //     // 0, 2, 24, 18
                    //     parallelepipedNodes[0], parallelepipedNodes[2], parallelepipedNodes[24],
                    //     parallelepipedNodes[18],
                    //     parallelepipedNodes[1], parallelepipedNodes[12], parallelepipedNodes[9],
                    //     parallelepipedNodes[13], parallelepipedNodes[10], parallelepipedNodes[21]
                    // }, area, lambda));
                    // _elements.Add(new(new[]
                    // {
                    //     // 2, 8, 6, 24
                    //     parallelepipedNodes[2], parallelepipedNodes[8], parallelepipedNodes[6], parallelepipedNodes[24],
                    //     parallelepipedNodes[5], parallelepipedNodes[4], parallelepipedNodes[13], parallelepipedNodes[7],
                    //     parallelepipedNodes[16], parallelepipedNodes[15]
                    // }, area, lambda));
                    //
                    // _elements.Add(new(new[]
                    // {
                    //     // 2, 26, 24, 20
                    //     parallelepipedNodes[2], parallelepipedNodes[26], parallelepipedNodes[24],
                    //     parallelepipedNodes[20],
                    //     parallelepipedNodes[14], parallelepipedNodes[13], parallelepipedNodes[11],
                    //     parallelepipedNodes[25], parallelepipedNodes[23], parallelepipedNodes[22]
                    // }, area, lambda));
                    // _elements.Add(new(new[]
                    // {
                    //     // 2, 8, 24, 26
                    //     parallelepipedNodes[2], parallelepipedNodes[8], parallelepipedNodes[24],
                    //     parallelepipedNodes[26],
                    //     parallelepipedNodes[5], parallelepipedNodes[13], parallelepipedNodes[14],
                    //     parallelepipedNodes[16], parallelepipedNodes[17], parallelepipedNodes[25]
                    // }, area, lambda));
                    // _elements.Add(new(new[]
                    // {
                    //     // 2, 24, 18, 20
                    //     parallelepipedNodes[2], parallelepipedNodes[24], parallelepipedNodes[18],
                    //     parallelepipedNodes[20],
                    //     parallelepipedNodes[13], parallelepipedNodes[10], parallelepipedNodes[11],
                    //     parallelepipedNodes[21], parallelepipedNodes[22], parallelepipedNodes[19]
                    // }, area, lambda));

                    _elements.Add(new(new[]
                    {
                        // 24, 26, 20, 8 -- vertices
                        parallelepipedNodes[24], parallelepipedNodes[26], parallelepipedNodes[20],
                        parallelepipedNodes[8],
                        parallelepipedNodes[25], parallelepipedNodes[22], parallelepipedNodes[16],
                        parallelepipedNodes[23], parallelepipedNodes[17], parallelepipedNodes[14]
                    }, area, lambda));
                    _elements.Add(new(new[]
                    {
                        // 18, 24, 20, 2
                        parallelepipedNodes[18], parallelepipedNodes[24], parallelepipedNodes[20],
                        parallelepipedNodes[2],
                        parallelepipedNodes[21], parallelepipedNodes[19], parallelepipedNodes[10],
                        parallelepipedNodes[22], parallelepipedNodes[13], parallelepipedNodes[11]
                    }, area, lambda));
                    _elements.Add(new(new[]
                    {
                        // 0, 18, 2, 6
                        parallelepipedNodes[0], parallelepipedNodes[18], parallelepipedNodes[2], parallelepipedNodes[6],
                        parallelepipedNodes[9], parallelepipedNodes[1], parallelepipedNodes[3], parallelepipedNodes[10],
                        parallelepipedNodes[12], parallelepipedNodes[4]
                    }, area, lambda));

                    _elements.Add(new(new[]
                    {
                        // 18, 24, 6, 2
                        parallelepipedNodes[18], parallelepipedNodes[24], parallelepipedNodes[6],
                        parallelepipedNodes[2],
                        parallelepipedNodes[21], parallelepipedNodes[12], parallelepipedNodes[10],
                        parallelepipedNodes[15], parallelepipedNodes[13], parallelepipedNodes[4]
                    }, area, lambda));
                    _elements.Add(new(new[]
                    {
                        // 2, 20, 8, 24
                        parallelepipedNodes[2], parallelepipedNodes[20], parallelepipedNodes[8],
                        parallelepipedNodes[24],
                        parallelepipedNodes[11], parallelepipedNodes[5], parallelepipedNodes[13],
                        parallelepipedNodes[14], parallelepipedNodes[22], parallelepipedNodes[16]
                    }, area, lambda));
                    _elements.Add(new(new[]
                    {
                        // 2, 6, 8, 24
                        parallelepipedNodes[2], parallelepipedNodes[6], parallelepipedNodes[8], parallelepipedNodes[24],
                        parallelepipedNodes[4], parallelepipedNodes[5], parallelepipedNodes[13], parallelepipedNodes[7],
                        parallelepipedNodes[15], parallelepipedNodes[16]
                    }, area, lambda));
                }
            }
        }

        return;

        int GetArea(int i) => ny / 2 <= i ? 0 : 1;
    }

    // TODO: create common method for both builders
    protected override void WriteToFile()
    {
        using StreamWriter sw1 = new("../../../SphereMeshContext/Python/points"),
            // sw2 = new("../../../SphereMeshContext/Python/prisms"),
            sw3 = new("../../../SphereMeshContext/Python/parallelepipeds"),
            sw4 = new("../../../SphereMeshContext/Python/elements");

        foreach (var point in _points)
        {
            sw1.WriteLine(point.ToString());
        }

        // foreach (var node in _prisms!)
        // {
        //     foreach (var i in node)
        //     {
        //         sw2.Write(i + " ");
        //     }
        //
        //     sw2.WriteLine();
        // }

        foreach (var node in _parallelepipeds!)
        {
            foreach (var i in node)
            {
                sw3.Write(i + " ");
            }

            sw3.WriteLine();
        }

        foreach (var element in _elements)
        {
            foreach (var node in element.Nodes.Take(4)) // first 4 geometric nodes
            {
                sw4.Write(node + " ");
            }

            sw4.Write(element.AreaNumber);
            sw4.WriteLine();
        }
    }
}