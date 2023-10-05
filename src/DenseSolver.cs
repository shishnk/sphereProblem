using System.Numerics;

namespace SphereProblem;

public static class DenseSolver
{
    public static Vector<T> Solve<T>(Matrix<T> matrix, Vector<T> f)
        where T : INumber<T>, IRootFunctions<T>
    {
        Vector<T> x = new(f.Length);
        Vector<T>.Copy(f, x);

        for (int i = 0; i < f.Length; i++)
        {
            var sum = T.Zero;

            for (int k = 0; k < i; k++)
            {
                sum += matrix[i, k] * x[k];
            }

            x[i] = (f[i] - sum) / matrix[i, i];
        }

        for (int i = x.Length - 1; i >= 0; i--)
        {
            var sum = T.Zero;

            for (int k = i + 1; k < x.Length; k++)
            {
                sum += matrix[i, k] * x[k];
            }

            x[i] -= sum;
        }

        return x;
    }
}