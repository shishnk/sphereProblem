using System.Numerics;

namespace SphereProblem;

public static class ArrayHelper
{
    public static T[] Copy<T>(this T[] source, T[] destination)
        where T : INumber<T>
    {
        for (int i = 0; i < source.Length; i++)
        {
            destination[i] = source[i];
        }

        return destination;
    }

    public static Span<T> Copy<T>(this T[] source, Span<T> destination)
        where T : INumber<T>
    {
        for (int i = 0; i < source.Length; i++)
        {
            destination[i] = source[i];
        }

        return destination;
    }

    public static void Fill<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }
}