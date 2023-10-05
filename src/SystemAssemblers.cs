﻿using System.Numerics;
using SphereProblem.Geometry;
using SphereProblem.SphereMeshContext;

namespace SphereProblem;

public class SystemAssembler(IBasis3D basis, SphereMesh mesh, Integrator integrator)
{
    private Matrix<double>? _baseMassMatrix;
    private Matrix<double>? _baseStiffnessMatrix;
    private readonly Point3D[] _cachedVertices = new Point3D[4]; // for tetrahedron

    public IBasis3D Basis => basis;
    public SphereMesh Mesh => mesh;
    public Integrator Integrator => integrator;
    public Matrix<double> StiffnessMatrix { get; } = new(basis.Size);
    public SparseMatrix? GlobalMatrix { get; set; }
    public Matrix<double> MassMatrix { get; } = new(basis.Size);

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

    public void AssemblyLocalMatrix(int ielem)
    {
        var templateElement = new Tetrahedron((0.0, 0.0, 0.0), (1.0, 0.0, 0.0), (0.0, 1.0, 0.0), (0.0, 0.0, 1.0));

        var selectedElement = mesh.Elements[ielem];

        _cachedVertices[0] = Mesh.Points[selectedElement.Nodes[0]];
        _cachedVertices[1] = Mesh.Points[selectedElement.Nodes[1]];
        _cachedVertices[2] = Mesh.Points[selectedElement.Nodes[2]];
        _cachedVertices[3] = Mesh.Points[selectedElement.Nodes[3]];

        // var determinant = CalculateDeterminant();
        var determinant = 1.0;

        Basis.DisposeCache();

        if (_baseStiffnessMatrix == null || _baseMassMatrix == null)
            // only for linear, because Jacobian is constant, TODO: remove after adding quadratic basis
        {
            for (int i = 0; i < Basis.Size; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    var i1 = i;
                    var j1 = j;
                    var function = double (Point3D p) =>
                    {
                        var dxFi1 = Basis.GetDPsi(i1, 0, p, _cachedVertices);
                        var dxFi2 = Basis.GetDPsi(j1, 0, p, _cachedVertices);
                        var dyFi1 = Basis.GetDPsi(i1, 1, p, _cachedVertices);
                        var dyFi2 = Basis.GetDPsi(j1, 1, p, _cachedVertices);
                        var dzFi1 = Basis.GetDPsi(i1, 2, p, _cachedVertices);
                        var dzFi2 = Basis.GetDPsi(j1, 2, p, _cachedVertices);

                        var calculates = CalculateJacobian(ielem, p, _cachedVertices);
                        var vector1 = new Vector<double>(calculates.inverse.Size) { new[] { dxFi1, dyFi1, dzFi1 } };
                        var vector2 = new Vector<double>(calculates.inverse.Size) { new[] { dxFi2, dyFi2, dzFi2 } };

                        return calculates.inverse * vector1 * (calculates.inverse * vector2) *
                               Math.Abs(calculates.determinant);
                    };

                    _baseStiffnessMatrix![i, j] =
                        _baseStiffnessMatrix[j, i] = Integrator.Gauss3D(function, templateElement);

                    function = p =>
                    {
                        var fi1 = Basis.GetPsi(i1, p, _cachedVertices);
                        var fi2 = Basis.GetPsi(j1, p, _cachedVertices);
                        var calculates = CalculateJacobian(ielem, p, _cachedVertices);

                        return fi1 * fi2 * Math.Abs(calculates.determinant);
                    };
                    _baseMassMatrix![i, j] = _baseMassMatrix[j, i] = Integrator.Gauss3D(function, templateElement);
                }
            }
        }

        for (int i = 0; i < Basis.Size; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                StiffnessMatrix[i, j] =
                    StiffnessMatrix[j, i] =
                        1.0 * _baseStiffnessMatrix![i, j] * determinant; // already ABS, $\lambda$ = 1.0
            }
        }

        for (int i = 0; i < Basis.Size; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                MassMatrix[i, j] = MassMatrix[j, i] = _baseMassMatrix![i, j] * determinant;
            }
        }
    }

    public void AssemblyVector(int ielem)
    {
    }

    private double CalculateDeterminant() // not used
    {
        var x0 = _cachedVertices[0].X;
        var y0 = _cachedVertices[0].Y;
        var z0 = _cachedVertices[0].Z;

        var x1 = _cachedVertices[1].X;
        var y1 = _cachedVertices[1].Y;
        var z1 = _cachedVertices[1].Z;

        var x2 = _cachedVertices[2].X;
        var y2 = _cachedVertices[2].Y;
        var z2 = _cachedVertices[2].Z;

        var x3 = _cachedVertices[3].X;
        var y3 = _cachedVertices[3].Y;
        var z3 = _cachedVertices[3].Z;

        return (x1 - x0) * ((y2 - y0) * (z3 - z0) - (y3 - y0) * (z2 - z0)) +
               (y1 - y0) * ((z2 - z0) * (x3 - x0) - (z3 - z0) * (x2 - x0)) +
               (z1 - z0) * ((x2 - x0) * (y3 - y0) - (x3 - x0) * (y2 - y0));
    }

    private (double determinant, Matrix<double> inverse) CalculateJacobian(int ielem, Point3D point,
        IReadOnlyList<Point3D> vertices)
    {
        Span<double> dx = stackalloc double[3];
        Span<double> dy = stackalloc double[3];
        Span<double> dz = stackalloc double[3];

        var element = Mesh.Elements[ielem];

        for (int i = 0; i < Basis.Size; i++)
        {
            for (int k = 0; k < 3; k++)
            {
                dx[k] += Basis.GetDPsi(i, k, point, vertices) * Mesh.Points[element.Nodes[i]].X;
                dy[k] += Basis.GetDPsi(i, k, point, vertices) * Mesh.Points[element.Nodes[i]].Y;
                dz[k] += Basis.GetDPsi(i, k, point, vertices) * Mesh.Points[element.Nodes[i]].Z;
            }
        }

        var determinant = dx[0] * dy[1] * dz[2] + (dx[1] * dy[2] * dz[0] + dy[0] * dz[1] * dx[2]) -
                          (dz[0] * dy[1] * dx[2] + dx[0] * dy[2] * dz[1] + dy[2] * dz[1] * dx[0]);

        Matrix4x4.Invert(new()
        {
            [0, 0] = (float)dx[0],
            [0, 1] = (float)dy[0],
            [0, 2] = (float)dz[0],
            [1, 0] = (float)dx[1],
            [1, 1] = (float)dy[1],
            [1, 2] = (float)dz[1],
            [2, 0] = (float)dx[2],
            [2, 1] = (float)dy[2],
            [2, 2] = (float)dz[2]
        }, out var inverse);

        return (determinant, 1.0 / determinant * (Matrix<double>)inverse);
    }
}