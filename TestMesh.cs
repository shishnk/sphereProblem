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
        if (!parameters.IsQuadratic) CreateLinearElementsInternal();
        else CreateQuadraticElementsInternal();
        return new(_points, _elements);
    }

    private void CreatePointsInternal()
    {
        var hx = parameters.IntervalX.Length / (!parameters.IsQuadratic ? parameters.SplitsX : 2 * parameters.SplitsX);
        var hy = parameters.IntervalY.Length / (!parameters.IsQuadratic ? parameters.SplitsY : 2 * parameters.SplitsY);
        var hz = parameters.IntervalZ.Length / (!parameters.IsQuadratic ? parameters.SplitsZ : 2 * parameters.SplitsZ);

        var numPointsX = !parameters.IsQuadratic ? parameters.SplitsX + 1 : 2 * parameters.SplitsX + 1;
        var numPointsY = !parameters.IsQuadratic ? parameters.SplitsY + 1 : 2 * parameters.SplitsY + 1;
        var numPointsZ = !parameters.IsQuadratic ? parameters.SplitsZ + 1 : 2 * parameters.SplitsZ + 1;

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

    private void CreateLinearElementsInternal()
    {
        // breaks up into 5 tetrahedrons
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
                    
                    // into 5 tetrahedrons
                    // _elements[index++] = new(new[] { nodes[4], nodes[6], nodes[7], nodes[2] });
                    // _elements[index++] = new(new[] { nodes[4], nodes[7], nodes[5], nodes[1] });
                    // _elements[index++] = new(new[] { nodes[4], nodes[7], nodes[1], nodes[2] });
                    //
                    // _elements[index++] = new(new[] { nodes[0], nodes[4], nodes[1], nodes[2] });
                    // _elements[index++] = new(new[] { nodes[2], nodes[7], nodes[1], nodes[3] });
                    
                    // into 6 tetrahedrons
                    _elements[index++] = new(new[] { nodes[6], nodes[7], nodes[5], nodes[3] });
                    _elements[index++] = new(new[] { nodes[4], nodes[6], nodes[5], nodes[1] });
                    _elements[index++] = new(new[] { nodes[0], nodes[4], nodes[1], nodes[2] });
                    
                    _elements[index++] = new(new[] { nodes[2], nodes[6], nodes[1], nodes[4] });
                    _elements[index++] = new(new[] { nodes[1], nodes[5], nodes[3], nodes[6] });
                    _elements[index++] = new(new[] { nodes[1], nodes[2], nodes[3], nodes[6] });
                }
            }
        }
    }

    private void CreateQuadraticElementsInternal()
    {
        const int localSize = 3;
        const int elementSize = 27;

        // breaks up into 6 tetrahedrons
        _elements = new FiniteElement[parameters.SplitsX * parameters.SplitsY * parameters.SplitsZ * 6];
        Span<int> nodes = stackalloc int[elementSize];

        for (int k = 0, idx = 0; k < parameters.SplitsZ; k++)
        {
            for (int j = 0; j < parameters.SplitsY; j++)
            {
                for (int i = 0; i < parameters.SplitsX; i++)
                {
                    for (int c = 0; c < elementSize; c++)
                    {
                        var lx = c % localSize;
                        var ly = c / localSize % localSize;
                        var lz = c / (localSize * localSize);

                        var ox = i * (localSize - 1) + lx;
                        var oy = ly * ((localSize - 1) * parameters.SplitsX + 1) +
                                 j * ((localSize - 1) * parameters.SplitsX + 1) * (localSize - 1);
                        var oz = lz * ((localSize - 1) * parameters.SplitsX + 1) *
                                 ((localSize - 1) * parameters.SplitsY + 1) +
                                 k * ((localSize - 1) * parameters.SplitsX + 1) *
                                 ((localSize - 1) * parameters.SplitsY + 1) * (localSize - 1);

                        nodes[c] = ox + oy + oz;
                    }

                    _elements[idx++] = new(new[]
                    {
                        // 24, 26, 20, 8 -- vertices
                        nodes[24], nodes[26], nodes[20], nodes[8],
                        nodes[25], nodes[22], nodes[16], nodes[23], nodes[17], nodes[14]
                    });
                    _elements[idx++] = new(new[]
                    {
                        // 18, 24, 20, 2
                        nodes[18], nodes[24], nodes[20], nodes[2],
                        nodes[21], nodes[19], nodes[10], nodes[22], nodes[13], nodes[11]
                    });
                    _elements[idx++] = new(new[]
                    {
                        // 0, 18, 2, 6
                        nodes[0], nodes[18], nodes[2], nodes[6],
                        nodes[9], nodes[1], nodes[3], nodes[10], nodes[12], nodes[4]
                    });
                    
                    _elements[idx++] = new(new[]
                    {
                        // 18, 24, 6, 2
                        nodes[18], nodes[24], nodes[6], nodes[2],
                        nodes[21], nodes[12], nodes[10], nodes[15], nodes[13], nodes[4]
                    });
                    _elements[idx++] = new(new[]
                    {
                        // 2, 20, 8, 24
                        nodes[2], nodes[20], nodes[8], nodes[24],
                        nodes[11], nodes[5], nodes[13], nodes[14], nodes[22], nodes[16]
                    });
                    _elements[idx++] = new(new[]
                    {
                        // 2, 6, 8, 24
                        nodes[2], nodes[6], nodes[8], nodes[24],
                        nodes[4], nodes[5], nodes[13], nodes[7], nodes[15], nodes[16]
                    });
                
                    
                    // breaks up into 5 tetrahedrons
                    // _elements[idx++] = new(new[]
                    // {
                    //     // 18, 24, 26, 6 -- vertices
                    //     nodes[18], nodes[24], nodes[26], nodes[6],
                    //     nodes[21], nodes[22], nodes[12], nodes[25], nodes[15], nodes[16]
                    // });
                    // _elements[idx++] = new(new[]
                    // {
                    //     // 18, 26, 20, 2
                    //     nodes[18], nodes[26], nodes[20], nodes[2],
                    //     nodes[22], nodes[19], nodes[10], nodes[23], nodes[14], nodes[11]
                    // });
                    // _elements[idx++] = new(new[]
                    // {
                    //     // 18, 26, 2, 6
                    //     nodes[18], nodes[26], nodes[2], nodes[6],
                    //     nodes[22], nodes[10], nodes[12], nodes[14], nodes[16], nodes[4]
                    // });
                    // _elements[idx++] = new(new[]
                    // {
                    //     // 0, 18, 2, 6
                    //     nodes[0], nodes[18], nodes[2], nodes[6],
                    //     nodes[9], nodes[1], nodes[3], nodes[10], nodes[12], nodes[4]
                    // });
                    // _elements[idx++] = new(new[]
                    // {
                    //     // 6, 26, 2, 8
                    //     nodes[6], nodes[26], nodes[2], nodes[8],
                    //     nodes[16], nodes[4], nodes[7], nodes[14], nodes[17], nodes[5]
                    // });
                }
            }
        }
    }
}

public class TestMeshParameters(
    Interval intervalX, int splitsX,
    Interval intervalY, int splitsY,
    Interval intervalZ, int splitsZ, bool isQuadratic = false)
{
    public Interval IntervalX { get; } = intervalX;
    public int SplitsX { get; } = splitsX;
    public Interval IntervalY { get; } = intervalY;
    public int SplitsY { get; } = splitsY;
    public Interval IntervalZ { get; } = intervalZ;
    public int SplitsZ { get; } = splitsZ;
    public bool IsQuadratic { get; } = isQuadratic;

    // public static TestMeshParameters ReadJson(string jsonPath)
    // {
    //     if (!File.Exists(jsonPath)) throw new FileNotFoundException("File does not exist");
    //
    //     using var sr = new StreamReader(jsonPath);
    //     return JsonConvert.DeserializeObject<TestMeshParameters>(sr.ReadToEnd()) ??
    //            throw new JsonSerializationException("Bad deserialization");
    // }
}

/// <summary>
///  Test mesh with parallelepiped elements breaking into tetrahedrons
/// </summary>
public class TestMesh(IReadOnlyList<Point3D> nodes, IReadOnlyList<FiniteElement> elements)
{
    public IReadOnlyList<Point3D> Points => nodes;
    public IReadOnlyList<FiniteElement> Elements => elements;
}