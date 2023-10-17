using SphereProblem.Geometry;

namespace SphereProblem;

public interface IBasis3D
{
    int Size { get; }

    double GetPsi(int functionNumber, Point3D point);
    double GetDPsi(int functionNumber, int varNumber);
    void UpdateCache(IReadOnlyList<Point3D> vertices);
}

public class LinearBasis3D : IBasis3D // tetrahedrons
{
    private readonly Matrix<double> _alphasMatrix = Matrix<double>.Identity(4);

    public int Size => 4;

    public double GetPsi(int functionNumber, Point3D point) =>
        _alphasMatrix[functionNumber, 0] + _alphasMatrix[functionNumber, 1] * point.X +
        _alphasMatrix[functionNumber, 2] * point.Y + _alphasMatrix[functionNumber, 3] * point.Z;

    public double GetDPsi(int functionNumber, int varNumber) => _alphasMatrix[functionNumber, varNumber + 1];

    public void UpdateCache(IReadOnlyList<Point3D> vertices)
    {
        _alphasMatrix[0, 0] = 1.0;
        _alphasMatrix[0, 1] = 1.0;
        _alphasMatrix[0, 2] = 1.0;
        _alphasMatrix[0, 3] = 1.0;
        _alphasMatrix[1, 0] = vertices[0].X;
        _alphasMatrix[1, 1] = vertices[1].X;
        _alphasMatrix[1, 2] = vertices[2].X;
        _alphasMatrix[1, 3] = vertices[3].X;
        _alphasMatrix[2, 0] = vertices[0].Y;
        _alphasMatrix[2, 1] = vertices[1].Y;
        _alphasMatrix[2, 2] = vertices[2].Y;
        _alphasMatrix[2, 3] = vertices[3].Y;
        _alphasMatrix[3, 0] = vertices[0].Z;
        _alphasMatrix[3, 1] = vertices[1].Z;
        _alphasMatrix[3, 2] = vertices[2].Z;
        _alphasMatrix[3, 3] = vertices[3].Z;

        _alphasMatrix.Invert4X4();
    }
}