using Newtonsoft.Json;
using SphereProblem.Geometry;

namespace SphereProblem;

public class TestMeshBuilder(TestMeshParameters parameters)
{
    private const int ElementSize = 4;

    private Point3D[] _points = null!;
    private FiniteElement[] _elements = null!;

    public TestMesh BuildTestMesh()
    {
        CreatePointsInternal();
        CreateElementsInternal();
        return new(_points, _elements);
    }

    private void CreatePointsInternal()
    {
        var hx = parameters.IntervalX.Length / parameters.SplitsX;
        var hy = parameters.IntervalY.Length / parameters.SplitsY;
        var hz = parameters.IntervalZ.Length / parameters.SplitsZ;

        var numPointsX = parameters.SplitsX + 1;
        var numPointsY = parameters.SplitsY + 1;
        var numPointsZ = parameters.SplitsZ + 1;

        _points = new Point3D[numPointsX * numPointsY * numPointsZ];

        for (int k = 0, index = 0; k < numPointsZ; k++)
        {
            for (int j = 0; j < numPointsY; j++)
            {
                for (int i = 0; i < numPointsX; i++)
                {
                    var x = parameters.IntervalX.LeftBorder + i * hx;
                    var y = parameters.IntervalY.LeftBorder + j * hy;
                    var z = parameters.IntervalZ.LeftBorder + k * hz;

                    _points[index++] = (x, y, z);
                }
            }
        }
    }

    private void CreateElementsInternal()
    {
        // breaks up into 6 tetrahedrons
        _elements = new FiniteElement[parameters.SplitsX * parameters.SplitsY * parameters.SplitsZ * 6];
        Span<int> nodes = stackalloc int[ElementSize * 2];

        var nx = parameters.SplitsX + 1;
        var ny = parameters.SplitsY + 1;

        for (int k = 0, index = 0; k < parameters.SplitsZ; k++)
        {
            for (int j = 0; j < parameters.SplitsY; j++)
            {
                for (int i = 0; i < parameters.SplitsX; i++)
                {
                    nodes[0] = i + j * nx + k * nx * ny;
                    nodes[1] = i + 1 + j * nx + k * nx * ny;
                    nodes[2] = i + (j + 1) * nx + k * nx * ny;
                    nodes[3] = i + 1 + (j + 1) * nx + k * nx * ny;
                    nodes[4] = i + j * nx + (k + 1) * nx * ny;
                    nodes[5] = i + 1 + j * nx + (k + 1) * nx * ny;
                    nodes[6] = i + (j + 1) * nx + (k + 1) * nx * ny;
                    nodes[7] = i + 1 + (j + 1) * nx + (k + 1) * nx * ny;

                    _elements[index++] = new(new[] { nodes[0], nodes[4], nodes[5], nodes[7] });
                    _elements[index++] = new(new[] { nodes[0], nodes[1], nodes[5], nodes[7] });
                    _elements[index++] = new(new[] { nodes[0], nodes[1], nodes[3], nodes[7] });
                    
                    _elements[index++] = new(new[] { nodes[3], nodes[6], nodes[7], nodes[4] });
                    _elements[index++] = new(new[] { nodes[3], nodes[2], nodes[6], nodes[4] });
                    _elements[index++] = new(new[] { nodes[3], nodes[2], nodes[0], nodes[4] });
                }
            }
        }
    }
}

public class TestMeshParameters(
    Interval intervalX, int splitsX,
    Interval intervalY, int splitsY,
    Interval intervalZ, int splitsZ)
{
    public Interval IntervalX { get; } = intervalX;
    public int SplitsX { get; } = splitsX;
    public Interval IntervalY { get; } = intervalY;
    public int SplitsY { get; } = splitsY;
    public Interval IntervalZ { get; } = intervalZ;
    public int SplitsZ { get; } = splitsZ;

    public static TestMeshParameters ReadJson(string jsonPath)
    {
        if (!File.Exists(jsonPath)) throw new FileNotFoundException("File does not exist");

        using var sr = new StreamReader(jsonPath);
        return JsonConvert.DeserializeObject<TestMeshParameters>(sr.ReadToEnd()) ??
               throw new JsonSerializationException("Bad deserialization");
    }
}

/// <summary>
///  Test mesh with parallelepiped elements breaking into tetrahedrons
/// </summary>
public class TestMesh(IReadOnlyList<Point3D> nodes, IReadOnlyList<FiniteElement> elements)
{
    public IReadOnlyList<Point3D> Points => nodes;
    public IReadOnlyList<FiniteElement> Elements => elements;
}