using System.Numerics;
using SphereProblem.Geometry;

namespace SphereProblem;

public abstract class BaseBasis3D
{
    protected Matrix4x4 _alphasMatrix = Matrix4x4.Identity;

    public abstract int Size { get; }

    public abstract double GetPsi(int functionNumber, Point3D point);
    public abstract double GetDPsi(int functionNumber, int varNumber, Point3D point);

    public void UpdateCache(IReadOnlyList<Point3D> vertices)
    {
        _alphasMatrix[0, 0] = 1.0f;
        _alphasMatrix[0, 1] = 1.0f;
        _alphasMatrix[0, 2] = 1.0f;
        _alphasMatrix[0, 3] = 1.0f;
        _alphasMatrix[1, 0] = (float)vertices[0].X;
        _alphasMatrix[1, 1] = (float)vertices[1].X;
        _alphasMatrix[1, 2] = (float)vertices[2].X;
        _alphasMatrix[1, 3] = (float)vertices[3].X;
        _alphasMatrix[2, 0] = (float)vertices[0].Y;
        _alphasMatrix[2, 1] = (float)vertices[1].Y;
        _alphasMatrix[2, 2] = (float)vertices[2].Y;
        _alphasMatrix[2, 3] = (float)vertices[3].Y;
        _alphasMatrix[3, 0] = (float)vertices[0].Z;
        _alphasMatrix[3, 1] = (float)vertices[1].Z;
        _alphasMatrix[3, 2] = (float)vertices[2].Z;
        _alphasMatrix[3, 3] = (float)vertices[3].Z;

        // _alphasMatrix.Invert4X4();
        Matrix4x4.Invert(_alphasMatrix, out _alphasMatrix);
    }

    protected void CalculateBarycentricCoordinates(Point3D point,
        out double l1, out double l2, out double l3, out double l4)
    {
        l1 = _alphasMatrix[0, 0] + _alphasMatrix[0, 1] * point.X + _alphasMatrix[0, 2] * point.Y +
             _alphasMatrix[0, 3] * point.Z;
        l2 = _alphasMatrix[1, 0] + _alphasMatrix[1, 1] * point.X + _alphasMatrix[1, 2] * point.Y +
             _alphasMatrix[1, 3] * point.Z;
        l3 = _alphasMatrix[2, 0] + _alphasMatrix[2, 1] * point.X + _alphasMatrix[2, 2] * point.Y +
             _alphasMatrix[2, 3] * point.Z;
        l4 = _alphasMatrix[3, 0] + _alphasMatrix[3, 1] * point.X + _alphasMatrix[3, 2] * point.Y +
             _alphasMatrix[3, 3] * point.Z;
    }
}

public class LinearBasis3D : BaseBasis3D // tetrahedrons
{
    // private readonly Matrix<double> _alphasMatrix = Matrix<double>.Identity(4);

    public override int Size => 4;

    public override double GetPsi(int functionNumber, Point3D point) =>
        _alphasMatrix[functionNumber, 0] + _alphasMatrix[functionNumber, 1] * point.X +
        _alphasMatrix[functionNumber, 2] * point.Y + _alphasMatrix[functionNumber, 3] * point.Z;

    public override double GetDPsi(int functionNumber, int varNumber, Point3D point) =>
        _alphasMatrix[functionNumber, varNumber + 1];
}

public class QuadraticBasis3D : BaseBasis3D
{
    public override int Size => 10;

    public override double GetPsi(int functionNumber, Point3D point)
    {
        CalculateBarycentricCoordinates(point, out var l1, out var l2, out var l3, out var l4);

        return functionNumber switch
        {
            0 => l1 * (2.0 * l1 - 1.0),
            1 => l2 * (2.0 * l2 - 1.0),
            2 => l3 * (2.0 * l3 - 1.0),
            3 => l4 * (2.0 * l4 - 1.0),
            4 => 4.0 * l1 * l2,
            5 => 4.0 * l1 * l3,
            6 => 4.0 * l1 * l4,
            7 => 4.0 * l2 * l3,
            8 => 4.0 * l2 * l4,
            9 => 4.0 * l3 * l4,
            _ => throw new ArgumentOutOfRangeException(nameof(functionNumber), functionNumber,
                "Invalid function number")
        };
    }

    public override double GetDPsi(int functionNumber, int varNumber, Point3D point)
    {
        CalculateBarycentricCoordinates(point, out var l1, out var l2, out var l3, out var l4);

        return functionNumber switch
        {
            0 => varNumber switch
            {
                0 => 4.0 * l1 * _alphasMatrix[0, 1] - _alphasMatrix[0, 1],
                1 => 4.0 * l1 * _alphasMatrix[0, 2] - _alphasMatrix[0, 2],
                2 => 4.0 * l1 * _alphasMatrix[0, 3] - _alphasMatrix[0, 3],
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid variable number")
            },
            1 => varNumber switch
            {
                0 => 4.0 * l2 * _alphasMatrix[1, 1] - _alphasMatrix[1, 1],
                1 => 4.0 * l2 * _alphasMatrix[1, 2] - _alphasMatrix[1, 2],
                2 => 4.0 * l2 * _alphasMatrix[1, 3] - _alphasMatrix[1, 3],
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid variable number")
            },
            2 => varNumber switch
            {
                0 => 4.0 * l3 * _alphasMatrix[2, 1] - _alphasMatrix[2, 1],
                1 => 4.0 * l3 * _alphasMatrix[2, 2] - _alphasMatrix[2, 2],
                2 => 4.0 * l3 * _alphasMatrix[2, 3] - _alphasMatrix[2, 3],
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid variable number")
            },
            3 => varNumber switch
            {
                0 => 4.0 * l4 * _alphasMatrix[3, 1] - _alphasMatrix[3, 1],
                1 => 4.0 * l4 * _alphasMatrix[3, 2] - _alphasMatrix[3, 2],
                2 => 4.0 * l4 * _alphasMatrix[3, 3] - _alphasMatrix[3, 3],
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid variable number")
            },
            4 => varNumber switch
            {
                0 => 4.0 * (_alphasMatrix[0, 1] * l2 + _alphasMatrix[1, 1] * l1),
                1 => 4.0 * (_alphasMatrix[0, 2] * l2 + _alphasMatrix[1, 2] * l1),
                2 => 4.0 * (_alphasMatrix[0, 3] * l2 + _alphasMatrix[1, 3] * l1),
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid variable number")
            },
            5 => varNumber switch
            {
                0 => 4.0 * (_alphasMatrix[0, 1] * l3 + _alphasMatrix[2, 1] * l1),
                1 => 4.0 * (_alphasMatrix[0, 2] * l3 + _alphasMatrix[2, 2] * l1),
                2 => 4.0 * (_alphasMatrix[0, 3] * l3 + _alphasMatrix[2, 3] * l1),
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid variable number")
            },
            6 => varNumber switch
            {
                0 => 4.0 * (_alphasMatrix[0, 1] * l4 + _alphasMatrix[3, 1] * l1),
                1 => 4.0 * (_alphasMatrix[0, 2] * l4 + _alphasMatrix[3, 2] * l1),
                2 => 4.0 * (_alphasMatrix[0, 3] * l4 + _alphasMatrix[3, 3] * l1),
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid variable number")
            },
            7 => varNumber switch
            {
                0 => 4.0 * (_alphasMatrix[1, 1] * l3 + _alphasMatrix[2, 1] * l2),
                1 => 4.0 * (_alphasMatrix[1, 2] * l3 + _alphasMatrix[2, 2] * l2),
                2 => 4.0 * (_alphasMatrix[1, 3] * l3 + _alphasMatrix[2, 3] * l2),
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid variable number")
            },
            8 => varNumber switch
            {
                0 => 4.0 * (_alphasMatrix[1, 1] * l4 + _alphasMatrix[3, 1] * l2),
                1 => 4.0 * (_alphasMatrix[1, 2] * l4 + _alphasMatrix[3, 2] * l2),
                2 => 4.0 * (_alphasMatrix[1, 3] * l4 + _alphasMatrix[3, 3] * l2),
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid variable number")
            },
            9 => varNumber switch
            {
                0 => 4.0 * (_alphasMatrix[2, 1] * l4 + _alphasMatrix[3, 1] * l3),
                1 => 4.0 * (_alphasMatrix[2, 2] * l4 + _alphasMatrix[3, 2] * l3),
                2 => 4.0 * (_alphasMatrix[2, 3] * l4 + _alphasMatrix[3, 3] * l3),
                _ => throw new ArgumentOutOfRangeException(nameof(varNumber), varNumber, "Invalid variable number")
            },
            _ => throw new ArgumentOutOfRangeException(nameof(functionNumber), functionNumber,
                "Invalid function number")
        };
    }
}