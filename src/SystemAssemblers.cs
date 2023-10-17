using System.Collections;
using System.Diagnostics.CodeAnalysis;
using SphereProblem.Geometry;

namespace SphereProblem;

public class SystemAssembler(IBasis3D basis, TestMesh mesh, Integrator integrator)
{
    private readonly Matrix<double> _baseStiffnessMatrix = new(basis.Size);
    private Point3D[] _cachedVertices = new Point3D[basis.Size]; // for tetrahedron
    private readonly Matrix<double> _jacobianMatrix = new(3);
    private readonly Tetrahedron _templateElement = Tetrahedron.TemplateElement;
    private readonly CalculateCollection<(double Determinant, Matrix<double> Inverse)> _calculateCache = new();
    private readonly Vector<double> _vector1 = new(3);
    private readonly Vector<double> _vector2 = new(3);
    private double _lastLambda = 1.0;
    private bool _isEnough;

    public IBasis3D Basis => basis;
    public TestMesh Mesh => mesh;
    public Integrator Integrator => integrator;
    public Matrix<double> StiffnessMatrix => _baseStiffnessMatrix.MultiplyByConstant(_lastLambda);
    public Matrix<double> MassMatrix { get; } = new(basis.Size);
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

        // _cachedVertices[0] = Mesh.Points[selectedElement[0]];
        // _cachedVertices[1] = Mesh.Points[selectedElement[1]];
        // _cachedVertices[2] = Mesh.Points[selectedElement[2]];
        // _cachedVertices[3] = Mesh.Points[selectedElement[3]];

        _cachedVertices = _templateElement.Vertices;

        _lastLambda = 1.0; // TODO
        _isEnough = false;
        var enumerator = _calculateCache.GetEnumerator();

        Basis.UpdateCache(_cachedVertices);
        _calculateCache.Clear();

        for (int i = 0; i < Basis.Size; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                var i1 = i;
                var j1 = j;

                var function = double (Point3D p) =>
                {
                    (double Determinant, Matrix<double> Inverse) currentCalculates;

                    var dxFi1 = Basis.GetDPsi(i1, 0);
                    var dxFi2 = Basis.GetDPsi(j1, 0);
                    var dyFi1 = Basis.GetDPsi(i1, 1);
                    var dyFi2 = Basis.GetDPsi(j1, 1);
                    var dzFi1 = Basis.GetDPsi(i1, 2);
                    var dzFi2 = Basis.GetDPsi(j1, 2);

                    if (!_isEnough)
                    {
                        currentCalculates = CalculateJacobian(ielem);
                        _calculateCache.Add(currentCalculates);
                    }
                    else
                    {
                        currentCalculates = enumerator.Current;
                        enumerator.MoveNext();
                    }

                    _vector1[0] = dxFi1;
                    _vector1[1] = dyFi1;
                    _vector1[2] = dzFi1;

                    _vector2[0] = dxFi2;
                    _vector2[1] = dyFi2;
                    _vector2[2] = dzFi2;

                    return currentCalculates.Inverse * _vector1 * (currentCalculates.Inverse * _vector2) *
                           Math.Abs(currentCalculates.Determinant);
                };
                _baseStiffnessMatrix[i, j] =
                    _baseStiffnessMatrix[j, i] = Integrator.Gauss3D(function, _templateElement);
                _isEnough = true;

                enumerator.Reset();

                function = double (p) =>
                {
                    var fi1 = Basis.GetPsi(i1, p);
                    var fi2 = Basis.GetPsi(j1, p);
                    var calculates = enumerator.Current;
                    enumerator.MoveNext();

                    return fi1 * fi2 * Math.Abs(calculates.Determinant);
                };

                MassMatrix[i, j] = MassMatrix[j, i] = Integrator.Gauss3D(function, _templateElement);
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
                    MassMatrix[i, j] * source(Mesh.Points[Mesh.Elements[ielem][j]]);
            }
        }
    }

    private (double Determinant, Matrix<double> Inverse) CalculateJacobian(int ielem)
    {
        const int varCount = 3;

        Span<double> dx = stackalloc double[varCount];
        Span<double> dy = stackalloc double[varCount];
        Span<double> dz = stackalloc double[varCount];

        var element = Mesh.Elements[ielem];

        for (int i = 0; i < Basis.Size; i++)
        {
            for (int k = 0; k < varCount; k++)
            {
                dx[k] += Basis.GetDPsi(i, k) * Mesh.Points[element[i]].X;
                dy[k] += Basis.GetDPsi(i, k) * Mesh.Points[element[i]].Y;
                dz[k] += Basis.GetDPsi(i, k) * Mesh.Points[element[i]].Z;
            }
        }

        var determinant = dx[0] * dy[1] * dz[2] + (dx[1] * dy[2] * dz[0] + dy[0] * dz[1] * dx[2]) -
                          (dz[0] * dy[1] * dx[2] + dx[0] * dy[2] * dz[1] + dy[2] * dz[1] * dx[0]);

        _jacobianMatrix[0, 0] = dx[0];
        _jacobianMatrix[0, 1] = dy[0];
        _jacobianMatrix[0, 2] = dz[0];
        _jacobianMatrix[1, 0] = dx[1];
        _jacobianMatrix[1, 1] = dy[1];
        _jacobianMatrix[1, 2] = dz[1];
        _jacobianMatrix[2, 0] = dx[2];
        _jacobianMatrix[2, 1] = dy[2];
        _jacobianMatrix[2, 2] = dz[2];

        _jacobianMatrix.Invert3X3();

        return (determinant, _jacobianMatrix.MultiplyByConstant(1.0 / determinant));
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

        public bool MoveNext() => ++_index < list.Count;

        public void Reset() => _index = 0;

        public T Current => list[_index];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}