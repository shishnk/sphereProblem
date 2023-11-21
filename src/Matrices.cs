using System.Numerics;
using System.Runtime.CompilerServices;

namespace SphereProblem;

public class SparseMatrix(int size, int sizeOffDiag)
{
    public int[] Ig { get; init; } = new int[size + 1];
    public int[] Jg { get; init; } = new int[sizeOffDiag];
    public double[] Di { get; } = new double[size];
    public double[] Gg { get; } = new double[sizeOffDiag];
    public int Size { get; } = size;

    public static Vector<double> operator *(SparseMatrix matrix, Vector<double> vector)
    {
        Vector<double> product = new(vector.Length);

        for (int i = 0; i < vector.Length; i++)
        {
            product[i] = matrix.Di[i] * vector[i];

            for (int j = matrix.Ig[i]; j < matrix.Ig[i + 1]; j++)
            {
                product[i] += matrix.Gg[j] * vector[matrix.Jg[j]];
                product[matrix.Jg[j]] += matrix.Gg[j] * vector[i];
            }
        }

        return product;
    }

    public void PrintDense(string path)
    {
        var a = new double[Size, Size];

        for (int i = 0; i < Size; i++)
        {
            a[i, i] = Di[i];

            for (int j = Ig[i]; j < Ig[i + 1]; j++)
            {
                a[i, Jg[j]] = Gg[j];
                a[Jg[j], i] = Gg[j];
            }
        }

        using var sw = new StreamWriter(path);

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                sw.Write(a[i, j].ToString("G3") + "\t");
            }

            sw.WriteLine();
        }
    }

    public void Clear()
    {
        for (int i = 0; i < Size; i++)
        {
            Di[i] = 0.0;

            for (int k = Ig[i]; k < Ig[i + 1]; k++)
            {
                Gg[k] = 0.0;
            }
        }
    }
}

public class Matrix<T>(int size) : ICloneable where T : struct, INumber<T>, IRootFunctions<T>
{
    private readonly T[,] _storage = new T[size, size];
    private T? _determinant;

    public int Size { get; } = size;
    public bool IsDecomposed { get; private set; }

    public T Determinant
    {
        get
        {
            if (_determinant != null) return _determinant.Value;

            var m11 = this[0, 0];
            var m12 = this[0, 1];
            var m13 = this[0, 2];
            var m21 = this[1, 0];
            var m22 = this[1, 1];
            var m23 = this[1, 2];
            var m31 = this[2, 0];
            var m32 = this[2, 1];
            var m33 = this[2, 2];

            _determinant = m11 * (m22 * m33 - m23 * m32) - m12 * (m21 * m33 - m23 * m31) +
                           m13 * (m21 * m32 - m22 * m31);

            return _determinant.Value;
        }
    }

    public T this[int i, int j]
    {
        get => _storage[i, j];
        set => _storage[i, j] = value;
    }

    public void Clear() => Array.Clear(_storage, 0, _storage.Length);

    public void Copy(Matrix<T> destination)
    {
        for (int i = 0; i < destination.Size; i++)
        {
            for (int j = 0; j < destination.Size; j++)
            {
                destination[i, j] = _storage[i, j];
            }
        }
    }

    /// <summary>
    /// LU decomposition
    /// </summary>
    public void Decompose()
    {
        IsDecomposed = true;

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                var suml = T.Zero;
                var sumu = T.Zero;

                if (i < j)
                {
                    for (int k = 0; k < i; k++)
                    {
                        sumu += _storage[i, k] * _storage[k, j];
                    }

                    _storage[i, j] = (_storage[i, j] - sumu) / _storage[i, i];
                }
                else
                {
                    for (int k = 0; k < j; k++)
                    {
                        suml += _storage[i, k] * _storage[k, j];
                    }

                    _storage[i, j] -= suml;
                }
            }
        }
    }

    public static Matrix<T> operator +(Matrix<T> fstMatrix, Matrix<T> sndMatrix)
    {
        var resultMatrix = new Matrix<T>(fstMatrix.Size);

        for (int i = 0; i < resultMatrix.Size; i++)
        {
            for (int j = 0; j < resultMatrix.Size; j++)
            {
                resultMatrix[i, j] = fstMatrix[i, j] + sndMatrix[i, j];
            }
        }

        return resultMatrix;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix<T> operator *(T value, Matrix<T> matrix)
    {
        var resultMatrix = new Matrix<T>(matrix.Size);

        for (int i = 0; i < resultMatrix.Size; i++)
        {
            for (int j = 0; j < resultMatrix.Size; j++)
            {
                resultMatrix[i, j] = value * matrix[i, j];
            }
        }

        return resultMatrix;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<T> operator *(Matrix<T> matrix, Vector<T> vector)
    {
        var result = new Vector<T>(matrix.Size);

        for (int i = 0; i < matrix.Size; i++)
        {
            for (int j = 0; j < matrix.Size; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }

        return result;
    }

    public static Matrix<T> Identity(int size)
    {
        var matrix = new Matrix<T>(size);

        for (int i = 0; i < size; i++)
        {
            matrix[i, i] = T.One;
        }

        return matrix;
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static explicit operator Matrix<T>(Matrix4x4 matrix) => new(4)
    // {
    //     [0, 0] = T.CreateChecked(matrix.M11),
    //     [0, 1] = T.CreateChecked(matrix.M12),
    //     [0, 2] = T.CreateChecked(matrix.M13),
    //     [0, 3] = T.CreateChecked(matrix.M14),
    //     [1, 0] = T.CreateChecked(matrix.M21),
    //     [1, 1] = T.CreateChecked(matrix.M22),
    //     [1, 2] = T.CreateChecked(matrix.M23),
    //     [1, 3] = T.CreateChecked(matrix.M24),
    //     [2, 0] = T.CreateChecked(matrix.M31),
    //     [2, 1] = T.CreateChecked(matrix.M32),
    //     [2, 2] = T.CreateChecked(matrix.M33),
    //     [2, 3] = T.CreateChecked(matrix.M34),
    //     [3, 0] = T.CreateChecked(matrix.M41),
    //     [3, 1] = T.CreateChecked(matrix.M42),
    //     [3, 2] = T.CreateChecked(matrix.M43),
    //     [3, 3] = T.CreateChecked(matrix.M44),
    // };
    //
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static implicit operator Matrix4x4(Matrix<T> matrix) => new()
    // {
    //     M11 = (float)Convert.ChangeType(matrix[0, 0], typeof(float)),
    //     M12 = (float)Convert.ChangeType(matrix[0, 1], typeof(float)),
    //     M13 = (float)Convert.ChangeType(matrix[0, 2], typeof(float)),
    //     M14 = (float)Convert.ChangeType(matrix[0, 3], typeof(float)),
    //     M21 = (float)Convert.ChangeType(matrix[1, 0], typeof(float)),
    //     M22 = (float)Convert.ChangeType(matrix[1, 1], typeof(float)),
    //     M23 = (float)Convert.ChangeType(matrix[1, 2], typeof(float)),
    //     M24 = (float)Convert.ChangeType(matrix[1, 3], typeof(float)),
    //     M31 = (float)Convert.ChangeType(matrix[2, 0], typeof(float)),
    //     M32 = (float)Convert.ChangeType(matrix[2, 1], typeof(float)),
    //     M33 = (float)Convert.ChangeType(matrix[2, 2], typeof(float)),
    //     M34 = (float)Convert.ChangeType(matrix[2, 3], typeof(float)),
    //     M41 = (float)Convert.ChangeType(matrix[3, 0], typeof(float)),
    //     M42 = (float)Convert.ChangeType(matrix[3, 1], typeof(float)),
    //     M43 = (float)Convert.ChangeType(matrix[3, 2], typeof(float)),
    //     M44 = (float)Convert.ChangeType(matrix[3, 3], typeof(float))
    // };

    public object Clone()
    {
        var clone = new Matrix<T>(Size);
        Copy(clone);
        return clone;
    }
}

// public static class MatrixExtensions
// {
//     public static Matrix<T> ToMatrix3<T>(this Matrix4x4 matrix)
//         where T : INumber<T>, IRootFunctions<T> =>
//         new(3)
//         {
//             [0, 0] = T.CreateChecked(matrix.M11),
//             [0, 1] = T.CreateChecked(matrix.M12),
//             [0, 2] = T.CreateChecked(matrix.M13),
//             [1, 0] = T.CreateChecked(matrix.M21),
//             [1, 1] = T.CreateChecked(matrix.M22),
//             [1, 2] = T.CreateChecked(matrix.M23),
//             [2, 0] = T.CreateChecked(matrix.M31),
//             [2, 1] = T.CreateChecked(matrix.M32),
//             [2, 2] = T.CreateChecked(matrix.M33)
//         };
// }