using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

public abstract class BaseSphereMeshBuilder(SphereMeshParameters parameters)
{
    private Point3D[]? _points;
    private SphereMesh? _sphereMesh;
    protected FiniteElement[]? _elements;

    protected abstract int ElementSize { get; }
    public abstract void CreatePoints();
    public abstract void CreateElements();

    protected void CreatePointsInternal(int phiSplits, int thetaSplits)
    {
        _points = new Point3D[phiSplits * thetaSplits];

        var phi = Math.PI / (phiSplits - 1);
        var theta = 2.0 * Math.PI / (thetaSplits - 1);

        for (int i = 0, idx = 0; i < phiSplits; i++)
        {
            for (int j = 0; j < thetaSplits; j++, idx++)
            {
                _points[idx] = (parameters.Radius, j * theta, phi * i);
            }
        }
    }

    public SphereMesh GetMeshInstance()
    {
        ConversionToCartesian.Convert(_points!, parameters, out var points);
        WriteToFile(points);
        return _sphereMesh ??= new(points ?? throw new NullReferenceException(),
            _elements ?? throw new NullReferenceException());
    }

    private void WriteToFile(IEnumerable<Point3D> points)
    {
        using StreamWriter sw1 = new("../../../SphereMeshContext/Python/points"),
            sw2 = new("../../../SphereMeshContext/Python/elements");

        foreach (var point in points)
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

public class Linear2DBaseSphereMeshBuilder(SphereMeshParameters parameters) : BaseSphereMeshBuilder(parameters)
{
    protected override int ElementSize => 3;

    public override void CreatePoints() => CreatePointsInternal(parameters.PhiSplits, parameters.ThetaSplits);

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

public class Quadratic2DBaseSphereMeshBuilder(SphereMeshParameters parameters) : BaseSphereMeshBuilder(parameters)
{
    protected override int ElementSize => 6;

    public override void CreatePoints() =>
        CreatePointsInternal(2 * parameters.PhiSplits - 1, 2 * parameters.ThetaSplits - 1);

    public override void CreateElements()
    {
        const int localSize = 3;
        _elements = new FiniteElement[2 * (parameters.PhiSplits - 1) * (parameters.ThetaSplits - 1)];
        Span<int> nodes = stackalloc int[ElementSize + localSize];

        for (int j = 0, idx = 0; j < parameters.PhiSplits - 1; j++)
        {
            for (int i = 0; i < parameters.ThetaSplits - 1; i++, idx += 2)
            {
                for (int k = 0; k < ElementSize + localSize; k++)
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

public class Linear3DSphereMeshBuilder(SphereMeshParameters parameters) : BaseSphereMeshBuilder(parameters)
{
    protected override int ElementSize => 4;

    public override void CreatePoints()
    {
        throw new NotImplementedException();
    }

    public override void CreateElements()
    {
        throw new NotImplementedException();
    }
}