using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

public abstract class SphereMeshBuilder(SphereMeshParameters parameters)
{
    private Point3D[]? _points;
    private SphereMesh? _sphereMesh;
    protected FiniteElement[]? _elements;

    protected abstract int ElementSize { get; }
    public abstract void CreatePoints();
    public abstract void CreateElements();

    protected void CreatePointsInternal(int phiSplits, int thetaSplits)
    {
        _points = new Point3D[parameters.PhiSplits * parameters.ThetaSplits];

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

    private void WriteToFile(IReadOnlyList<Point3D> points)
    {
        using StreamWriter sw1 = new("Python/points"), sw2 = new("Python/elements");

        foreach (var point in points)
        {
            sw1.WriteLine(point.ToString());
        }
        
        foreach (var element in _elements!)
        {
            foreach (var elementNode in element.Nodes)
            {
                sw2.Write(elementNode + " ");
            }
            
            sw2.WriteLine();
        }
    }
}

public class LinearSphereMeshBuilder(SphereMeshParameters parameters) : SphereMeshBuilder(parameters)
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

                _elements[idx] = new(new[] { v1, v2, v3 });
                _elements[idx + 1] = new(new[] { v2, v4, v3 });
            }
        }
    }
}

public class QuadraticSphereMeshBuilder(SphereMeshParameters parameters) : SphereMeshBuilder(parameters)
{
    protected override int ElementSize => 6;

    public override void CreatePoints() => CreatePointsInternal(2 * parameters.PhiSplits, 2 * parameters.ThetaSplits);

    public override void CreateElements()
    {
        throw new NotImplementedException();
    }
}