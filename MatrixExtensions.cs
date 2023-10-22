namespace SphereProblem;

public static class DoubleMatrixExtensions
{
    // public static void Invert4X4(this Matrix<double> matrix)
    // {
    //     if (matrix.Size != 4) throw new ArgumentException("Matrix size must be 4x4.", nameof(matrix));
    //
    //     const int n = 4;
    //     const double epsilon = 1e-10;
    //
    //     Span<double> augmentedMatrix = stackalloc double[2 * n * n];
    //
    //     for (int i = 0; i < n; i++)
    //     {
    //         for (int j = 0; j < n; j++)
    //         {
    //             augmentedMatrix[i * 2 * n + j] = matrix[i, j];
    //         }
    //
    //         augmentedMatrix[i * 2 * n + i + n] = 1.0;
    //     }
    //
    //     for (int i = 0; i < n; i++)
    //     {
    //         var pivot = augmentedMatrix[i * 2 * n + i];
    //
    //         if (Math.Abs(pivot) < epsilon)
    //         {
    //             throw new InvalidOperationException("Matrix is not invertible.");
    //         }
    //
    //         for (int j = 0; j < 2 * n; j++)
    //         {
    //             augmentedMatrix[i * 2 * n + j] /= pivot;
    //         }
    //
    //         for (int j = 0; j < n; j++)
    //         {
    //             if (j == i) continue;
    //
    //             var factor = augmentedMatrix[j * 2 * n + i];
    //
    //             for (int k = 0; k < 2 * n; k++)
    //             {
    //                 augmentedMatrix[j * 2 * n + k] -= factor * augmentedMatrix[i * 2 * n + k];
    //             }
    //         }
    //     }
    //
    //     for (int i = 0; i < n; i++)
    //     {
    //         for (int j = 0; j < n; j++)
    //         {
    //             matrix[i, j] = augmentedMatrix[i * 2 * n + j + n];
    //         }
    //     }
    // }

    public static void Invert3X3(this Matrix<double> matrix)
    {
        const double epsilon = 1e-16;

        if (matrix.Size != 3) throw new ArgumentException("Matrix size must be 3x3.", nameof(matrix));

        var m11 = matrix[0, 0];
        var m12 = matrix[0, 1];
        var m13 = matrix[0, 2];
        var m21 = matrix[1, 0];
        var m22 = matrix[1, 1];
        var m23 = matrix[1, 2];
        var m31 = matrix[2, 0];
        var m32 = matrix[2, 1];
        var m33 = matrix[2, 2];
        
        var determinant = m11 * (m22 * m33 - m23 * m32) - m12 * (m21 * m33 - m23 * m31) + m13 * (m21 * m32 - m22 * m31);

        if (Math.Abs(determinant) < epsilon)
        {
            throw new InvalidOperationException("Matrix is not invertible.");
        }

        var invDet = 1.0 / determinant;

        matrix[0, 0] = (m22 * m33 - m23 * m32) * invDet;
        matrix[0, 1] = -(m12 * m33 - m13 * m32) * invDet;
        matrix[0, 2] = (m12 * m23 - m13 * m22) * invDet;
        matrix[1, 0] = -(m21 * m33 - m23 * m31) * invDet;
        matrix[1, 1] = (m11 * m33 - m13 * m31) * invDet;
        matrix[1, 2] = -(m11 * m23 - m13 * m21) * invDet;
        matrix[2, 0] = (m21 * m32 - m22 * m31) * invDet;
        matrix[2, 1] = -(m11 * m32 - m12 * m31) * invDet;
        matrix[2, 2] = (m11 * m22 - m12 * m21) * invDet;
    }
}