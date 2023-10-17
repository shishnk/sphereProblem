namespace SphereProblem;

public static class DoubleMatrixExtensions
{
    public static void Invert4X4(this Matrix<double> matrix)
    {
        const double epsilon = 1e-10;

        if (matrix.Size != 4) throw new ArgumentException("Matrix size must be 4x4.", nameof(matrix));

        var m11 = matrix[0, 0];
        var m12 = matrix[0, 1];
        var m13 = matrix[0, 2];
        var m14 = matrix[0, 3];
        var m21 = matrix[1, 0];
        var m22 = matrix[1, 1];
        var m23 = matrix[1, 2];
        var m24 = matrix[1, 3];
        var m31 = matrix[2, 0];
        var m32 = matrix[2, 1];
        var m33 = matrix[2, 2];
        var m34 = matrix[2, 3];
        var m41 = matrix[3, 0];
        var m42 = matrix[3, 1];
        var m43 = matrix[3, 2];
        var m44 = matrix[3, 3];

        var det1 = m33 * m44 - m34 * m43;
        var det2 = m32 * m44 - m34 * m42;
        var det3 = m32 * m43 - m33 * m42;
        var det4 = m31 * m44 - m34 * m41;
        var det5 = m31 * m43 - m33 * m41;
        var det6 = m31 * m42 - m32 * m41;

        var det = m11 * (m22 * det1 - m23 * det2 + m24 * det3) -
                  m12 * (m21 * det1 - m23 * det4 + m24 * det5) +
                  m13 * (m21 * det2 - m22 * det4 + m24 * det6) -
                  m14 * (m21 * det3 - m22 * det5 + m23 * det6);

        if (Math.Abs(det) < epsilon) throw new InvalidOperationException("Matrix is not invertible.");

        var invDet = 1.0 / det;

        matrix[0, 0] = (m22 * det1 - m23 * det2 + m24 * det3) * invDet;
        matrix[0, 1] = (-m12 * det1 + m13 * det2 - m14 * det3) * invDet;
        matrix[0, 2] = (m42 * m23 - m43 * m22) * invDet;
        matrix[0, 3] = (-m42 * m13 + m43 * m12 - m44 * m11) * invDet;
        matrix[1, 0] = (-m21 * det1 + m23 * det4 - m24 * det5) * invDet;
        matrix[1, 1] = (m11 * det1 - m13 * det4 + m14 * det5) * invDet;
        matrix[1, 2] = (-m41 * m23 + m43 * m21) * invDet;
        matrix[1, 3] = (m41 * m13 - m43 * m11 + m44 * m21) * invDet;
        matrix[2, 0] = (m21 * det2 - m22 * det4 + m24 * det6) * invDet;
        matrix[2, 1] = (-m11 * det2 + m12 * det4 - m14 * det6) * invDet;
        matrix[2, 2] = (m41 * m22 - m42 * m21) * invDet;
        matrix[2, 3] = (-m41 * m12 + m42 * m11 - m44 * m22) * invDet;
        matrix[3, 0] = (-m21 * det3 + m22 * det5 - m23 * det6) * invDet;
        matrix[3, 1] = (m11 * det3 - m12 * det5 + m13 * det6) * invDet;
        matrix[3, 2] = (-m41 * m23 + m42 * m13 - m43 * m11) * invDet;
        matrix[3, 3] = (m41 * m22 - m42 * m12 + m43 * m11) * invDet;
    }

    public static void Invert3X3(this Matrix<double> matrix)
    {
        const double epsilon = 1e-10;

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

        var det = m11 * (m22 * m33 - m23 * m32) - m12 * (m21 * m33 - m23 * m31) + m13 * (m21 * m32 - m22 * m31);

        if (Math.Abs(det) < epsilon)
        {
            throw new InvalidOperationException("Matrix is not invertible.");
        }

        var invDet = 1.0f / det;

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
    
    public static Matrix<double> MultiplyByConstant(this Matrix<double> matrix, double value)
    {
        for (int i = 0; i < matrix.Size; i++)
        {
            for (int j = 0; j < matrix.Size; j++)
            {
                matrix[i, j] *= value;
            }
        }

        return matrix;
    }
}