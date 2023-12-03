using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

[Obsolete("Use BaseUvSphereMeshBuilder")]
public abstract class BaseSphereMeshBuilder(SphereMeshParameters parameters)
{
    private SphereMesh? _sphereMesh;
    protected Point3D[]? _points;
    protected FiniteElement[]? _elements;
    protected (int, int, int)[]? _faces; // for python
    protected int[][]? _parallelepipedNodes; // for python

    public abstract void CreatePoints();
    public abstract void CreateElements();

    protected void CreatePointsInner(int phiSplits, int thetaSplits)
    {
        _points = new Point3D[phiSplits * thetaSplits * parameters.Radius.Count];

        var phi = Math.PI / (phiSplits - 1);
        var theta = 2.0 * Math.PI / (thetaSplits - 1);

        for (int i = 0, idx = 0; i < phiSplits; i++)
        {
            foreach (var r in parameters.Radius.Reverse())
            {
                for (int j = 0; j < thetaSplits; j++, idx++)
                {
                    var x = r * Math.Sin(phi * i) * Math.Cos(j * theta) + parameters.Center.X;
                    var y = r * Math.Sin(phi * i) * Math.Sin(j * theta) + parameters.Center.Y;
                    var z = r * Math.Cos(phi * i) + parameters.Center.Z;

                    _points[idx] = (x, y, z);
                }
            }
        }
    }

    public SphereMesh GetMeshInstance()
    {
        WriteToFile();
        return _sphereMesh ??= new(_points!, _elements!);
    }

    protected virtual void WriteToFile()
    {
        using StreamWriter sw1 = new("../../../SphereMeshContext/Python/points"),
            sw2 = new("../../../SphereMeshContext/Python/elements");

        foreach (var point in _points!)
        {
            sw1.WriteLine(point.ToString());
        }

        foreach (var element in _elements!)
        {
            for (var i = 0; i < 3; i++)
            {
                sw2.Write(element.Nodes[i] + " ");
            }

            sw2.WriteLine();
        }
    }
}

public class LinearSphereMesh2DBuilder(SphereMeshParameters parameters) : BaseSphereMeshBuilder(parameters)
{
    public override void CreatePoints() => CreatePointsInner(parameters.PhiSplits, parameters.ThetaSplits);

    public override void CreateElements()
    {
        _elements = new FiniteElement[2 * (parameters.PhiSplits - 1) * (parameters.ThetaSplits - 1)];

        for (int i = 0, idx = 0; i < parameters.PhiSplits - 1; i++)
        {
            for (int j = 0; j < parameters.ThetaSplits - 1; j++, idx += 2)
            {
                var v1 = i * parameters.ThetaSplits + j;
                var v2 = i * parameters.ThetaSplits + j + 1;
                var v3 = (i + 1) * parameters.ThetaSplits + j;
                var v4 = (i + 1) * parameters.ThetaSplits + j + 1;

                _elements[idx] = new(new[] { v1, v3, v2 });
                _elements[idx + 1] = new(new[] { v4, v2, v3 });
            }
        }
    }
}

public class QuadraticSphereMesh2DBuilder(SphereMeshParameters parameters) : BaseSphereMeshBuilder(parameters)
{
    public override void CreatePoints() =>
        CreatePointsInner(2 * parameters.PhiSplits - 1, 2 * parameters.ThetaSplits - 1);

    public override void CreateElements()
    {
        const int elementSize = 6;
        const int localSize = 3;
        _elements = new FiniteElement[2 * (parameters.PhiSplits - 1) * (parameters.ThetaSplits - 1)];
        Span<int> nodes = stackalloc int[elementSize + localSize];

        for (int j = 0, idx = 0; j < parameters.PhiSplits - 1; j++)
        {
            for (int i = 0; i < parameters.ThetaSplits - 1; i++, idx += 2)
            {
                for (int k = 0; k < nodes.Length; k++)
                {
                    var lx = k % localSize;
                    var ly = k / localSize;

                    var ox = i * (localSize - 1) + lx;
                    var oy = ly * ((localSize - 1) * parameters.ThetaSplits - 1) +
                             j * ((localSize - 1) * parameters.ThetaSplits - 1) * (localSize - 1);

                    nodes[k] = ox + oy;
                }

                _elements[idx] = new(new[] { nodes[0], nodes[6], nodes[2], nodes[3], nodes[4], nodes[1] });
                _elements[idx + 1] = new(new[] { nodes[8], nodes[2], nodes[6], nodes[5], nodes[4], nodes[7] });
            }
        }
    }
}

[Obsolete("The algorithm has degenerate elements, use LinearUvSphereMeshBuilder instead")]
public class LinearSphereMesh3DBuilder(SphereMeshParameters parameters) : BaseSphereMeshBuilder(parameters)
{
    public override void CreatePoints() => CreatePointsInner(parameters.ThetaSplits, parameters.PhiSplits);

    public override void CreateElements()
    {
        const int elementSize = 8; // parallelepiped's vertices count
        _elements = new FiniteElement[(parameters.PhiSplits - 1) * (parameters.ThetaSplits - 1) *
                                      (parameters.Radius.Count - 1) * 6];
        _faces = new (int, int, int)[_elements.Length * 4]; // rough estimation
        _parallelepipedNodes = new int[_elements.Length / 6][];
        Span<int> nodes = stackalloc int[elementSize];

        for (var i = 0; i < _parallelepipedNodes.Length; i++)
        {
            _parallelepipedNodes[i] = new int[elementSize];
        }

        var nx = parameters.ThetaSplits;
        var ny = parameters.Radius.Count;

        for (int k = 0, index = 0, faceIdx = 0, pIdx = 0; k < parameters.PhiSplits - 1; k++)
        {
            for (int j = 0; j < parameters.Radius.Count - 1; j++)
            {
                for (int i = 0; i < parameters.ThetaSplits - 1; i++)
                {
                    nodes[0] = i + j * nx + k * nx * ny;
                    nodes[1] = i + 1 + j * nx + k * nx * ny;
                    nodes[2] = i + (j + 1) * nx + k * nx * ny;
                    nodes[3] = i + 1 + (j + 1) * nx + k * nx * ny;
                    nodes[4] = i + j * nx + (k + 1) * nx * ny;
                    nodes[5] = i + 1 + j * nx + (k + 1) * nx * ny;
                    nodes[6] = i + (j + 1) * nx + (k + 1) * nx * ny;
                    nodes[7] = i + 1 + (j + 1) * nx + (k + 1) * nx * ny;

                    _parallelepipedNodes[pIdx++] = nodes.ToArray();

                    _elements[index++] = new(new[] { nodes[6], nodes[7], nodes[5], nodes[3] });
                    _faces[faceIdx++] = (nodes[6], nodes[7], nodes[5]);
                    _faces[faceIdx++] = (nodes[3], nodes[5], nodes[7]);
                    _faces[faceIdx++] = (nodes[3], nodes[6], nodes[7]);
                    _faces[faceIdx++] = (nodes[3], nodes[5], nodes[6]);

                    _elements[index++] = new(new[] { nodes[4], nodes[6], nodes[5], nodes[1] });
                    _faces[faceIdx++] = (nodes[4], nodes[6], nodes[5]);
                    _faces[faceIdx++] = (nodes[1], nodes[5], nodes[6]);
                    _faces[faceIdx++] = (nodes[1], nodes[4], nodes[6]);
                    _faces[faceIdx++] = (nodes[1], nodes[5], nodes[4]);

                    _elements[index++] = new(new[] { nodes[0], nodes[4], nodes[1], nodes[2] });
                    _faces[faceIdx++] = (nodes[0], nodes[4], nodes[1]);
                    _faces[faceIdx++] = (nodes[2], nodes[1], nodes[4]);
                    _faces[faceIdx++] = (nodes[2], nodes[0], nodes[4]);
                    _faces[faceIdx++] = (nodes[2], nodes[1], nodes[0]);

                    _elements[index++] = new(new[] { nodes[2], nodes[6], nodes[1], nodes[4] });
                    _faces[faceIdx++] = (nodes[2], nodes[6], nodes[1]);
                    _faces[faceIdx++] = (nodes[4], nodes[1], nodes[6]);
                    _faces[faceIdx++] = (nodes[4], nodes[2], nodes[6]);
                    _faces[faceIdx++] = (nodes[4], nodes[1], nodes[2]);

                    _elements[index++] = new(new[] { nodes[1], nodes[5], nodes[3], nodes[6] });
                    _faces[faceIdx++] = (nodes[1], nodes[5], nodes[3]);
                    _faces[faceIdx++] = (nodes[6], nodes[3], nodes[5]);
                    _faces[faceIdx++] = (nodes[6], nodes[1], nodes[5]);
                    _faces[faceIdx++] = (nodes[6], nodes[3], nodes[1]);

                    _elements[index++] = new(new[] { nodes[1], nodes[2], nodes[3], nodes[6] });
                    _faces[faceIdx++] = (nodes[1], nodes[6], nodes[3]);
                    _faces[faceIdx++] = (nodes[2], nodes[3], nodes[6]);
                    _faces[faceIdx++] = (nodes[2], nodes[1], nodes[6]);
                    _faces[faceIdx++] = (nodes[2], nodes[3], nodes[1]);
                }
            }
        }
    }

    protected override void WriteToFile()
    {
        using StreamWriter sw1 = new("../../../SphereMeshContext/Python/points"),
            sw2 = new("../../../SphereMeshContext/Python/faces"),
            sw3 = new("../../../SphereMeshContext/Python/parallelepipeds");

        foreach (var point in _points!)
        {
            sw1.WriteLine(point.ToString());
        }

        foreach (var face in _faces!)
        {
            sw2.WriteLine($"{face.Item1} {face.Item2} {face.Item3}");
        }

        foreach (var node in _parallelepipedNodes!)
        {
            foreach (var i in node)
            {
                sw3.Write(i + " ");
            }

            sw3.WriteLine();
        }
    }
}

public abstract class BaseUvSphereMeshBuilder(UvSphereMeshParameters parameters)
{
    private SphereMesh? _sphereMesh;
    protected Point3D[]? _points;
    protected FiniteElement[]? _elements;

    public abstract void CreatePoints();
    public abstract void CreateElements();

    protected void CreatePointsInner(int stackSplits, int sliceSplits)
    {
        var oneSphereSize = (stackSplits - 1) * sliceSplits + 2; // +2 for bottom and top points
        _points = new Point3D[oneSphereSize * parameters.Radius.Count]; 

        for (int k = parameters.Radius.Count - 1, idx = 0; k >= 0; k--)
        {
            var r = parameters.Radius[k];
            _points[idx++] = (parameters.Center.X, parameters.Center.Y, parameters.Center.Z + r);

            for (int i = 0; i < stackSplits - 1; i++)
            {
                var phiAngle = Math.PI * (i + 1) / stackSplits;

                for (int j = 0; j < sliceSplits; j++)
                {
                    var thetaAngle = 2.0 * Math.PI * j / sliceSplits;
                    var x = r * Math.Sin(phiAngle) * Math.Cos(thetaAngle) + parameters.Center.X;
                    var y = r * Math.Sin(phiAngle) * Math.Sin(thetaAngle) + parameters.Center.Y;
                    var z = r * Math.Cos(phiAngle) + parameters.Center.Z;

                    _points[idx++] = (x, y, z);
                }
            }
            
            _points[idx++] = (parameters.Center.X, parameters.Center.Y, parameters.Center.Z - r);
        }
    }

    public SphereMesh GetMeshInstance() => _sphereMesh ??= new(_points!, _elements!);
}

public class LinearUvSphereMeshBuilder(UvSphereMeshParameters parameters) : BaseUvSphereMeshBuilder(parameters)
{
    public override void CreatePoints() => CreatePointsInner(parameters.StackSplits, parameters.SliceSplits);

    public override void CreateElements()
    {
        const int prismSize = 6;
        const int parallelepipedSize = 8;
        var oneSphereSize = (parameters.StackSplits - 1) * parameters.SliceSplits + 2;
        _elements = new FiniteElement[2 * parameters.SliceSplits * (parameters.Radius.Count - 1) * 3 +
                                      2 * parameters.StackSplits * (parameters.Radius.Count - 1) * 6];
        Span<int> prismNodes = stackalloc int[prismSize];
        Span<int> parallelepipedNodes = stackalloc int[parallelepipedSize];
        var index = 0;

        // top and bottom prisms
        for (int k = 0; k < parameters.Radius.Count - 1; k++)
        {
            for (int i = 0; i < parameters.SliceSplits; i++)
            {
                // top
                prismNodes[0] = k * oneSphereSize; // go to the next first sphere point
                prismNodes[1] = i + 1 + k * oneSphereSize;
                prismNodes[2] = (i + 1) % parameters.SliceSplits + 1 + k * oneSphereSize;
                prismNodes[3] = oneSphereSize * (k + 1);
                prismNodes[4] = i + 1 + (k + 1) * oneSphereSize;
                prismNodes[5] = (i + 1) % parameters.SliceSplits + 1 + (k + 1) * oneSphereSize;

                FormingElements(prismNodes);

                // bottom
                prismNodes[0] = oneSphereSize * (k + 2) - 1;
                prismNodes[1] = i + parameters.SliceSplits * (parameters.StackSplits - 2) + 1 +
                                (k + 1) * oneSphereSize;
                prismNodes[2] = (i + 1) % parameters.SliceSplits +
                                parameters.SliceSplits * (parameters.StackSplits - 2) + 1 +
                                (k + 1) * oneSphereSize;
                prismNodes[3] = (k + 1) * oneSphereSize - 1;
                prismNodes[4] = i + parameters.SliceSplits * (parameters.StackSplits - 2) + 1 + k * oneSphereSize;
                prismNodes[5] = (i + 1) % parameters.SliceSplits +
                                parameters.SliceSplits * (parameters.StackSplits - 2) + 1 + k * oneSphereSize;

                FormingElements(prismNodes);
                continue;

                void FormingElements(ReadOnlySpan<int> nodes)
                {
                    _elements![index++] = new(new[] { nodes[5], nodes[4], nodes[3], nodes[1] });
                    _elements[index++] = new(new[] { nodes[5], nodes[3], nodes[2], nodes[1] });
                    _elements[index++] = new(new[] { nodes[3], nodes[2], nodes[1], nodes[0] });
                }
            }
        }

        // middle parallelepipeds
        for (int k = 0; k < parameters.Radius.Count - 1; k++)
        {
            for (int j = 0; j < parameters.StackSplits - 2; j++)
            {
                var j0 = j * parameters.SliceSplits + 1;
                var j1 = (j + 1) * parameters.SliceSplits + 1;

                for (int i = 0; i < parameters.SliceSplits; i++)
                {
                    parallelepipedNodes[0] = j1 + i + k * oneSphereSize;
                    parallelepipedNodes[1] = j1 + (i + 1) % parameters.SliceSplits + k * oneSphereSize;
                    parallelepipedNodes[2] = parallelepipedNodes[0] + oneSphereSize;
                    parallelepipedNodes[3] = parallelepipedNodes[1] + oneSphereSize;
                    parallelepipedNodes[4] = j0 + i + k * oneSphereSize;
                    parallelepipedNodes[5] = j0 + (i + 1) % parameters.SliceSplits + k * oneSphereSize;
                    parallelepipedNodes[6] = parallelepipedNodes[4] + oneSphereSize;
                    parallelepipedNodes[7] = parallelepipedNodes[5] + oneSphereSize;

                    _elements[index++] = new(new[]
                    {
                        parallelepipedNodes[6], parallelepipedNodes[7], parallelepipedNodes[5], parallelepipedNodes[3]
                    });
                    _elements[index++] = new(new[]
                    {
                        parallelepipedNodes[4], parallelepipedNodes[6], parallelepipedNodes[5], parallelepipedNodes[1]
                    });
                    _elements[index++] = new(new[]
                    {
                        parallelepipedNodes[0], parallelepipedNodes[4], parallelepipedNodes[1], parallelepipedNodes[2]
                    });

                    _elements[index++] = new(new[]
                    {
                        parallelepipedNodes[2], parallelepipedNodes[6], parallelepipedNodes[1], parallelepipedNodes[4]
                    });
                    _elements[index++] = new(new[]
                    {
                        parallelepipedNodes[1], parallelepipedNodes[5], parallelepipedNodes[3], parallelepipedNodes[6]
                    });
                    _elements[index++] = new(new[]
                    {
                        parallelepipedNodes[1], parallelepipedNodes[2], parallelepipedNodes[3], parallelepipedNodes[6]
                    });
                }
            }
        }
        
        using StreamWriter sw = new("../../../SphereMeshContext/Python/points"),
            sw1 = new("../../../SphereMeshContext/Python/tetrahedrons");
        
        foreach (var point in _points!) sw.WriteLine(point.ToString());
        
        foreach (var element in _elements)
        {
            foreach (var node in element.Nodes)
            {
                sw1.Write(node + " ");
            }
            
            sw1.WriteLine();
        }
    }
}