﻿using SphereProblem.Geometry;

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
        int areaNumber;
        double lambda;
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

                    areaNumber = GetArea(j);
                    lambda = parameters.Properties[areaNumber];

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
    public override void CreatePoints() => CreatePointsInner(2 * parameters.PhiSplits, 2 * parameters.ThetaSplits);

    public override void CreateElements()
    {
        const int localSize = 3;
        const int parallelepipedSize = 27;
        const int prismSize = 14;
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

        var skip = parameters.Radius.Count;

        for (int k = 0; k < parameters.ThetaSplits - 2; k++)
        {
            for (int j = 0; j < parameters.Radius.Count - 1; j++)
            {
                for (int i = 0; i < parameters.PhiSplits - 1; i++)
                {
                    for (int c = 0; c < parallelepipedSize; c++)
                    {
                        var lx = c % localSize;
                        var ly = c / localSize % localSize;
                        var lz = c / (localSize * localSize);

                        var ox = i * (localSize - 1) + lx;
                        var oy = ly * ((localSize - 1) * (parameters.PhiSplits - 1) + 1) +
                                 j * ((localSize - 1) * (parameters.PhiSplits - 1) + 1) * (localSize - 1);
                        var oz = lz * ((localSize - 1) * (parameters.PhiSplits - 1) + 1) *
                                 ((localSize - 1) * (parameters.Radius.Count - 1) + 1) +
                                 k * ((localSize - 1) * (parameters.PhiSplits - 1) + 1) *
                                 ((localSize - 1) * (parameters.Radius.Count - 1) + 1) * (localSize - 1);

                        parallelepipedNodes[c] = ox + oy + oz + skip;
                    }
                }
            }
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

            sw4.WriteLine();
        }
    }
}