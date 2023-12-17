using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SphereProblem.Geometry;

public class Point3DJsonConverter : JsonConverter<Point3D>
{
    public override Point3D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
            {
                var array = JsonSerializer.Deserialize<double[]>(ref reader, options)!;
                if (array.Length != 3) throw new JsonException($"Wrong vector length({array.Length})!");
                return new(array[0], array[1], array[2]);
            }
            case JsonTokenType.String:
            {
                var line = reader.GetString();
                if (Point3D.TryParse(line ?? string.Empty, out var pnt)) return pnt;
                break;
            }
            case JsonTokenType.StartObject:
            {
                while (reader.Read())
                {
                    if (reader.TokenType != JsonTokenType.String) continue;
                    var line = reader.GetString();
                    if (!Point3D.TryParse(line ?? string.Empty, out var pnt)) continue;
                    reader.Read();
                    return pnt;
                }

                break;
            }
            default:
                throw new NotSupportedException();
        }

        throw new FormatException("Can't parse as Point3D!");
    }

    public override void Write(Utf8JsonWriter writer, Point3D value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, new[] { value.X, value.Y, value.Z }, options);
}

[JsonConverter(typeof(Point3DJsonConverter))]
public readonly record struct Point3D(double X, double Y, double Z)
{
    public double this[int index] => index switch
    {
        0 => X,
        1 => Y,
        2 => Z,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Invalid point index")
    };

    public static Point3D UnitX => (1.0, 0.0, 0.0);
    
    public static Point3D UnitY => (0.0, 1.0, 0.0);
    
    public static Point3D UnitZ => (0.0, 0.0, 1.0);

    public static bool TryParse(string line, out Point3D point)
    {
        var words = line.Split(new[] { ' ', ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length != 4 || !double.TryParse(words[1], out var x) || !double.TryParse(words[2], out var y) ||
            double.TryParse(words[3], out var z))
        {
            point = default;
            return false;
        }

        point = new(x, y, z);
        return true;
    }

    public override string ToString() => $"{X} {Y} {Z}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D operator +(Point3D a, Point3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D operator -(Point3D a, Point3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D operator *(double b, Point3D a) => new(a.X * b, a.Y * b, a.Z * b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point3D((double X, double Y, double Z) tuple) => new(tuple.X, tuple.Y, tuple.Z);
}