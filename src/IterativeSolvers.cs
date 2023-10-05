using System.Collections.Immutable;
using System.Diagnostics;

namespace SphereProblem;

public static class IterativeSolverExtensions
{
    public static IterativeSolver SetMatrixEx(this IterativeSolver solver, SparseMatrix matrix)
    {
        solver.SetMatrix(matrix);
        return solver;
    }

    public static IterativeSolver SetVectorEx(this IterativeSolver solver, Vector<double> vector)
    {
        solver.SetVector(vector);
        return solver;
    }
}

public abstract class IterativeSolver(int maxIters, double eps)
{
    private ImmutableArray<double>? _cachedSolution;

    protected TimeSpan? _runningTime;
    protected SparseMatrix _matrix = default!;
    protected Vector<double> _vector = default!;
    protected Vector<double>? _solution;

    public int MaxIters { get; } = maxIters;
    public double Eps { get; } = eps;
    public TimeSpan? RunningTime => _runningTime;

    public ImmutableArray<double>? Solution
    {
        get
        {
            if (_solution != null) _cachedSolution ??= _solution.ToImmutableArray();
            return _cachedSolution;
        }
    }

    public void SetMatrix(SparseMatrix matrix)
        => _matrix = matrix;

    public void SetVector(Vector<double> vector)
        => _vector = vector;

    public abstract void Compute();

    protected void Cholesky(Span<double> ggnew, Span<double> dinew)
    {
        double suml = 0.0;
        double sumdi = 0.0;

        for (int i = 0; i < _matrix.Size; i++)
        {
            int i0 = _matrix.Ig[i];
            int i1 = _matrix.Ig[i + 1];

            for (int k = i0; k < i1; k++)
            {
                int j = _matrix.Jg[k];
                int j0 = _matrix.Ig[j];
                int j1 = _matrix.Ig[j + 1];
                int ik = i0;
                int kj = j0;

                while (ik < k && kj < j1)
                {
                    if (_matrix.Jg[ik] == _matrix.Jg[kj])
                    {
                        suml += ggnew[ik] * ggnew[kj];
                        ik++;
                        kj++;
                    }
                    else
                    {
                        if (_matrix.Jg[ik] > _matrix.Jg[kj])
                            kj++;
                        else
                            ik++;
                    }
                }

                ggnew[k] = (ggnew[k] - suml) / dinew[j];
                sumdi += ggnew[k] * ggnew[k];
                suml = 0.0;
            }

            dinew[i] = Math.Sqrt(dinew[i] - sumdi);
            sumdi = 0.0;
        }
    }

    protected Vector<double> MoveForCholesky(Vector<double> vector, Span<double> ggnew, Span<double> dinew)
    {
        Vector<double> y = new(vector.Length);
        Vector<double> x = new(vector.Length);
        Vector<double>.Copy(vector, y);

        double sum = 0.0;

        for (int i = 0; i < _matrix.Size; i++) // Прямой ход
        {
            int i0 = _matrix.Ig[i];
            int i1 = _matrix.Ig[i + 1];

            for (int k = i0; k < i1; k++)
                sum += ggnew[k] * y[_matrix.Jg[k]];

            y[i] = (y[i] - sum) / dinew[i];
            sum = 0.0;
        }

        Vector<double>.Copy(y, x);

        for (int i = _matrix.Size - 1; i >= 0; i--) // Обратный ход
        {
            int i0 = _matrix.Ig[i];
            int i1 = _matrix.Ig[i + 1];
            x[i] = y[i] / dinew[i];

            for (int k = i0; k < i1; k++)
                y[_matrix.Jg[k]] -= ggnew[k] * x[i];
        }

        return x;
    }
}

public class CGM(int maxIters, double eps) : IterativeSolver(maxIters, eps)
{
    public override void Compute()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_matrix, $"{nameof(_matrix)} cannot be null, set the matrix");
            ArgumentNullException.ThrowIfNull(_vector, $"{nameof(_vector)} cannot be null, set the vector");

            double vectorNorm = _vector.Norm();

            _solution = new(_vector.Length);

            Vector<double> z = new(_vector.Length);

            var sw = Stopwatch.StartNew();

            var r = _vector - (_matrix * _solution);

            Vector<double>.Copy(r, z);

            for (int iter = 0; iter < MaxIters && r.Norm() / vectorNorm >= Eps; iter++)
            {
                var tmp = _matrix * z;
                var alpha = r * r / (tmp * z);
                _solution += alpha * z;
                var squareNorm = r * r;
                r -= alpha * tmp;
                var beta = r * r / squareNorm;
                z = r + beta * z;
            }

            sw.Stop();

            _runningTime = sw.Elapsed;
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"We had problem with null: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }
}

public class CGMCholesky : IterativeSolver
{
    public CGMCholesky(int maxIters, double eps) : base(maxIters, eps)
    {
    }

    public override void Compute()
    {
        try
        {
            ArgumentNullException.ThrowIfNull(_matrix, $"{nameof(_matrix)} cannot be null, set the matrix");
            ArgumentNullException.ThrowIfNull(_vector, $"{nameof(_vector)} cannot be null, set the vector");

            double vectorNorm = _vector.Norm();

            _solution = new(_vector.Length);

            Span<double> ggnew = new double[_matrix.Gg.Length];
            Span<double> dinew = new double[_matrix.Di.Length];

            _matrix.Gg.Copy(ggnew);
            _matrix.Di.Copy(dinew);

            var sw = Stopwatch.StartNew();

            Cholesky(ggnew, dinew);

            var r = _vector - _matrix * _solution;
            var z = MoveForCholesky(r, ggnew, dinew);

            for (int iter = 0; iter < MaxIters && r.Norm() / vectorNorm >= Eps; iter++)
            {
                var tmp = MoveForCholesky(r, ggnew, dinew) * r;
                var sndTemp = _matrix * z;
                var alpha = tmp / (sndTemp * z);
                _solution += alpha * z;
                r -= alpha * sndTemp;
                var fstTemp = MoveForCholesky(r, ggnew, dinew);
                var beta = fstTemp * r / tmp;
                z = fstTemp + beta * z;
            }

            sw.Stop();

            _runningTime = sw.Elapsed;
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"We had problem with null: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"We had problem: {ex.Message}");
        }
    }
}