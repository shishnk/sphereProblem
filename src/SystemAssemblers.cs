using System.Collections;
using System.Diagnostics.CodeAnalysis;
using SphereProblem.Geometry;
using SphereProblem.SphereMeshContext;

namespace SphereProblem;

public class SystemAssembler(BaseBasis3D basis, SphereMesh mesh, Integrator integrator)
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
        public CalculateCollection<(double Determinant, Matrix<double> Inverse)> CalculateCache { get; } = new();

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
    private readonly Point3D[] _cachedVertices = new Point3D[4]; // for tetrahedron
    private readonly Tetrahedron _templateElement = Tetrahedron.TemplateElement;

    /// <summary>
    /// 0..3 one vector, 3..6 another vector
    /// </summary>
    private readonly Vector<double> _doubleVector = new(6);
    private bool _isEnough;

    public BaseBasis3D Basis => basis;
    public SphereMesh Mesh => mesh;
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

    [SuppressMessage("ReSharper", "GenericEnumeratorNotDisposed")]
    public void AssemblyLocalMatrices(int ielem)
    {
        var selectedElement = mesh.Elements[ielem];

        _cachedVertices[0] = mesh.Points[selectedElement[0]];
        _cachedVertices[1] = mesh.Points[selectedElement[1]];
        _cachedVertices[2] = mesh.Points[selectedElement[2]];
        _cachedVertices[3] = mesh.Points[selectedElement[3]];

        _isEnough = false;

        var enumerator = _cache.CalculateCache.GetEnumerator();
        Basis.UpdateCache(_cachedVertices);
        _cache.CalculateCache.Clear();

        // for (int i = 0; i < 10; i++)
        // {
        //     Console.WriteLine($"Value at node {i}: {basis.GetPsi(i, mesh.Points[selectedElement[i]])}");
        //
        //     for (int j = 0; j < 10; j++)
        //     {
        //         if (i == j) continue;
        //         
        //         Console.WriteLine($"Value at node {j}: {basis.GetPsi(i, mesh.Points[selectedElement[j]])}");
        //     }
        // }
        //
        // Console.WriteLine();

        for (int i = 0; i < Basis.Size; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                var i1 = i;
                var j1 = j;

                var function = double (Point3D p) =>
                {
                    (double Determinant, Matrix<double> Inverse) currentCalculates;

                    var dxFi1 = Basis.GetDPsi(i1, 0, p);
                    var dxFi2 = Basis.GetDPsi(j1, 0, p);
                    var dyFi1 = Basis.GetDPsi(i1, 1, p);
                    var dyFi2 = Basis.GetDPsi(j1, 1, p);
                    var dzFi1 = Basis.GetDPsi(i1, 2, p);
                    var dzFi2 = Basis.GetDPsi(j1, 2, p);

                    if (!_isEnough)
                    {
                        CalculateJacobian(ielem, p);
                        _cache.CalculateCache.Add((_cache.JacobianMatrix.Determinant, (Matrix<double>)_cache.JacobianMatrix.Clone()));
                        currentCalculates = (_cache.JacobianMatrix.Determinant, _cache.JacobianMatrix);
                    }
                    else
                    {
                        currentCalculates = enumerator.Current;
                        enumerator.MoveNext();
                    }

                    _doubleVector[0] = dxFi1;
                    _doubleVector[1] = dyFi1;
                    _doubleVector[2] = dzFi1;
                    _doubleVector[3] = dxFi2;
                    _doubleVector[4] = dyFi2;
                    _doubleVector[5] = dzFi2;

                    return AssemblerCache.MultiplyMatrixByVector(currentCalculates.Inverse, _doubleVector.AsSpan(..3),
                               _cache.ResultVector1) *
                           AssemblerCache.MultiplyMatrixByVector(currentCalculates.Inverse, _doubleVector.AsSpan(3..6),
                               _cache.ResultVector2) * Math.Abs(currentCalculates.Determinant);
                };
                _baseStiffnessMatrix[i, j] =
                    _baseStiffnessMatrix[j, i] = integrator.Gauss3D(function, _templateElement);
                _isEnough = true;

                enumerator.Reset();

                function = double (p) =>
                {
                    var fi1 = Basis.GetPsi(i1, p);
                    var fi2 = Basis.GetPsi(j1, p);
                    var currentCalculates = enumerator.Current;
                    enumerator.MoveNext();

                    return fi1 * fi2 * Math.Abs(currentCalculates.Determinant);
                };

                MassMatrix[i, j] = MassMatrix[j, i] = integrator.Gauss3D(function, _templateElement);
                enumerator.Reset();
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

    private class CalculateCollection<T>(IEnumerable<T> collection) : IEnumerable<T> where T : struct
    {
        private readonly List<T> _list = collection.ToList();

        public CalculateCollection() : this(Enumerable.Empty<T>())
        {
        }

        public IEnumerator<T> GetEnumerator() => new CalculateEnumerator<T>(_list);

        public void Add(T item) => _list.Add(item);

        public void Clear() => _list.Clear();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }  

    private class CalculateEnumerator<T>(IReadOnlyList<T> list) : IEnumerator<T> where T : struct
    {
        private int _index;
        
        public T Current => list[_index];

        public bool MoveNext() => ++_index < list.Count;

        public void Reset() => _index = 0;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}