using System.Numerics;
using SphereProblem.Geometry;

namespace SphereProblem;

public interface IBasis3D
{
    public int Size { get; }

    public double GetPsi(int functionNumber, Point3D point, IReadOnlyList<Point3D> vertices);
    public double GetDPsi(int functionNumber, int varNumber, Point3D point, IReadOnlyList<Point3D> vertices);
    public void UpdateCache(IReadOnlyList<Point3D> vertices);
    public void DisposeCache();
}

public class LinearBasis3D : IBasis3D // tetrahedrons
{
    private Matrix<double>? _cachedMatrix;
    private Matrix<double>? _decomposedCachedMatrix;
    private Matrix4x4? _alphasMatrix;

    public int Size => 4;

    public double GetPsi(int functionNumber, Point3D point, IReadOnlyList<Point3D> vertices)
    {
        if (!_decomposedCachedMatrix!.IsDecomposed) _decomposedCachedMatrix.Decompose(); 

        var result = DenseSolver.Solve(_decomposedCachedMatrix, new(Size)
        {
            [0] = 1.0,
            [1] = point.X,
            [2] = point.Y,
            [3] = point.Z
        });

        return functionNumber switch
        {
            0 => result[0],
            1 => result[1],
            2 => result[2],
            3 => result[3],
            _ => throw new ArgumentOutOfRangeException(nameof(functionNumber), functionNumber,
                "Invalid function number")
        };
    }

    public double GetDPsi(int functionNumber, int varNumber, Point3D point, IReadOnlyList<Point3D> vertices)
    {
        return functionNumber switch
        {
            0 => varNumber switch
            {
                0 => _alphasMatrix!.Value[0, 1],
                1 => _alphasMatrix!.Value[0, 2],
                2 => _alphasMatrix!.Value[0, 3],
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid var number")
            },
            1 => varNumber switch
            {
                0 => _alphasMatrix!.Value[1, 1],
                1 => _alphasMatrix!.Value[1, 2],
                2 => _alphasMatrix!.Value[1, 3],
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid var number")
            },
            2 => varNumber switch
            {
                0 => _alphasMatrix!.Value[2, 1],
                1 => _alphasMatrix!.Value[2, 2],
                2 => _alphasMatrix!.Value[2, 3],
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid var number")
            },
            3 => varNumber switch
            {
                0 => _alphasMatrix!.Value[3, 1],
                1 => _alphasMatrix!.Value[3, 2],
                2 => _alphasMatrix!.Value[3, 3],
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid var number")
            },
            _ => throw new ArgumentOutOfRangeException(nameof(functionNumber), functionNumber,
                "Invalid function number")
        };
    }

    public void UpdateCache(IReadOnlyList<Point3D> vertices)
    {
        _cachedMatrix ??= new(Size)
        {
            [0, 0] = 1.0,
            [0, 1] = 1.0,
            [0, 2] = 1.0,
            [0, 3] = 1.0,
            [1, 0] = vertices[0].X,
            [1, 1] = vertices[1].X,
            [1, 2] = vertices[2].X,
            [1, 3] = vertices[3].X,
            [2, 0] = vertices[0].Y,
            [2, 1] = vertices[1].Y,
            [2, 2] = vertices[2].Y,
            [2, 3] = vertices[3].Y,
            [3, 0] = vertices[0].Z,
            [3, 1] = vertices[1].Z,
            [3, 2] = vertices[2].Z,
            [3, 3] = vertices[3].Z,
        };

        _decomposedCachedMatrix ??= new(_cachedMatrix.Size);
        _cachedMatrix.Copy(_decomposedCachedMatrix);
        Matrix4x4.Invert(_cachedMatrix!, out var alphasMatrix);
        _alphasMatrix = alphasMatrix;
    }

    public void DisposeCache()
    {
        _cachedMatrix = null;
        _decomposedCachedMatrix = null;
        _alphasMatrix = null;
    }
}