using System.Diagnostics.CodeAnalysis;
using SphereProblem.Geometry;

namespace SphereProblem;

public class SystemAssembler(BaseBasis3D basis, TestMesh mesh, Integrator integrator)
{
    /// <summary>
    ///  Cache contains data for assembler and help methods.
    /// </summary>
    private class AssemblerCache
    {
        public Vector<double> ResultVector1 { get; } = new(3);
        public Vector<double> ResultVector2 { get; } = new(3);
        public Matrix<double> JacobianMatrix { get; } = new(3);
        public Vector<double> DerivativeVector { get; } = new(9);
        public Dictionary<Point3D, (double Determinant, Matrix<double> Inverse)> CalculateCache { get; } = new();

        public static Vector<double> MultiplyMatrixByVector(Matrix<double> matrix, ReadOnlySpan<double> vector,
            Vector<double> result)
        {
            for (int i = 0; i < matrix.Size; i++)
            {
                result[i] = 0.0;

                for (int j = 0; j < matrix.Size; j++)
                {
                    result[i] += matrix[i, j] * vector[j];
                }
            }

            return result;
        }
    }

    private readonly AssemblerCache _cache = new();
    private readonly Matrix<double> _baseStiffnessMatrix = new(basis.Size);
    private readonly Tetrahedron _templateElement = Tetrahedron.TemplateElement;

    /// <summary>
    /// 0..3 one vector, 3..6 another vector
    /// </summary>
    private readonly Vector<double> _doubleVector = new(6);

    public BaseBasis3D Basis => basis;
    public TestMesh Mesh => mesh;
    public Matrix<double> StiffnessMatrix => _baseStiffnessMatrix;

    private Matrix<double> MassMatrix { get; } = new(basis.Size);
    public Vector<double> Vector { get; } = new(mesh.Points.Count);
    public SparseMatrix? GlobalMatrix { get; set; }

    public void FillGlobalMatrix(int i, int j, double value)
    {
        if (GlobalMatrix is null)
        {
            throw new("Initialize the global matrix (use portrait builder)!");
        }

        if (i == j)
        {
            GlobalMatrix.Di[i] += value;
            return;
        }

        if (i <= j) return;

        for (int ind = GlobalMatrix.Ig[i]; ind < GlobalMatrix.Ig[i + 1]; ind++)
        {
            if (GlobalMatrix.Jg[ind] != j) continue;
            GlobalMatrix.Gg[ind] += value;
            return;
        }
    }

    public void AssemblyLocalMatrices(int ielem)
    {
        _cache.CalculateCache.Clear();

        for (int i = 0; i < Basis.Size; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                var i1 = i;
                var j1 = j;

                var function = double (Point3D p) =>
                {
                    var dxFi1 = Basis.GetDPsi(i1, 0, p);
                    var dxFj2 = Basis.GetDPsi(j1, 0, p);
                    var dyFi1 = Basis.GetDPsi(i1, 1, p);
                    var dyFj2 = Basis.GetDPsi(j1, 1, p);
                    var dzFi1 = Basis.GetDPsi(i1, 2, p);
                    var dzFj2 = Basis.GetDPsi(j1, 2, p);

                    if (!_cache.CalculateCache.TryGetValue(p, out var currentCalculates))
                    {
                        CalculateJacobian(ielem, p);
                        currentCalculates = (_cache.JacobianMatrix.Determinant!.Value, (Matrix<double>)_cache.JacobianMatrix.Clone());
                        _cache.CalculateCache[p] = currentCalculates;
                    }

                    _doubleVector[0] = dxFi1;
                    _doubleVector[1] = dyFi1;
                    _doubleVector[2] = dzFi1;
                    _doubleVector[3] = dxFj2;
                    _doubleVector[4] = dyFj2;
                    _doubleVector[5] = dzFj2;

                    return AssemblerCache.MultiplyMatrixByVector(currentCalculates.Inverse, _doubleVector.AsSpan(..3),
                               _cache.ResultVector1) *
                           AssemblerCache.MultiplyMatrixByVector(currentCalculates.Inverse, _doubleVector.AsSpan(3..6),
                               _cache.ResultVector2) * Math.Abs(currentCalculates.Determinant);
                };
                _baseStiffnessMatrix[i, j] =
                    _baseStiffnessMatrix[j, i] = integrator.Gauss3D(function, _templateElement);

                function = double (p) =>
                {
                    var fi1 = Basis.GetPsi(i1, p);
                    var fj2 = Basis.GetPsi(j1, p);

                    return fi1 * fj2 * Math.Abs(_cache.CalculateCache[p].Determinant);
                };

                MassMatrix[i, j] = MassMatrix[j, i] = integrator.Gauss3D(function, _templateElement);
            }
        }
    }

    public void AssemblyVector(int ielem, Func<Point3D, double> source)
    {
        for (int i = 0; i < Basis.Size; i++)
        {
            for (int j = 0; j < Basis.Size; j++)
            {
                Vector[mesh.Elements[ielem][i]] +=
                    MassMatrix[i, j] * source(mesh.Points[Mesh.Elements[ielem][j]]);
            }
        }
    }

    private void CalculateJacobian(int ielem, Point3D point)
    {
        const int varCount = 3;

        var dx = _cache.DerivativeVector.AsSpan(..3);
        var dy = _cache.DerivativeVector.AsSpan(3..6);
        var dz = _cache.DerivativeVector.AsSpan(6..9);

        var element = Mesh.Elements[ielem];

        for (int i = 0; i < Basis.Size; i++)
        {
            for (int k = 0; k < varCount; k++)
            {
                dx[k] += Basis.GetDPsi(i, k, point) * mesh.Points[element[i]].X;
                dy[k] += Basis.GetDPsi(i, k, point) * mesh.Points[element[i]].Y;
                dz[k] += Basis.GetDPsi(i, k, point) * mesh.Points[element[i]].Z;
            }
        }

        _cache.JacobianMatrix[0, 0] = dx[0];
        _cache.JacobianMatrix[0, 1] = dy[0];
        _cache.JacobianMatrix[0, 2] = dz[0];
        _cache.JacobianMatrix[1, 0] = dx[1];
        _cache.JacobianMatrix[1, 1] = dy[1];
        _cache.JacobianMatrix[1, 2] = dz[1];
        _cache.JacobianMatrix[2, 0] = dx[2];
        _cache.JacobianMatrix[2, 1] = dy[2];
        _cache.JacobianMatrix[2, 2] = dz[2];

        _cache.JacobianMatrix.Invert3X3();
        _cache.DerivativeVector.Fill(0.0);
    }

// private double CalculateDeterminant() // not used
// {
//     var x0 = _cachedVertices[0].X;
//     var y0 = _cachedVertices[0].Y;
//     var z0 = _cachedVertices[0].Z;
//
//     var x1 = _cachedVertices[1].X;
//     var y1 = _cachedVertices[1].Y;
//     var z1 = _cachedVertices[1].Z;
//
//     var x2 = _cachedVertices[2].X;
//     var y2 = _cachedVertices[2].Y;
//     var z2 = _cachedVertices[2].Z;
//
//     var x3 = _cachedVertices[3].X;
//     var y3 = _cachedVertices[3].Y;
//     var z3 = _cachedVertices[3].Z;
//
//     return (x1 - x0) * ((y2 - y0) * (z3 - z0) - (y3 - y0) * (z2 - z0)) +
//            (y1 - y0) * ((z2 - z0) * (x3 - x0) - (z3 - z0) * (x2 - x0)) +
//            (z1 - z0) * ((x2 - x0) * (y3 - y0) - (x3 - x0) * (y2 - y0));
// }
}