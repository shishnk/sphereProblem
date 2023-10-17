using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SphereProblem.Geometry;

public class Point3DJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => typeof(Point3D) == objectType;

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartArray)
        {
            var array = JArray.Load(reader);
            if (array.Count == 3)
                return new Point3D(array[0].Value<double>(), array[1].Value<double>(), array[2].Value<double>());
            throw new FormatException($"Wrong vector length({array.Count})!");
        }

        if (Point3D.TryParse((string?)reader.Value ?? "", out var point)) return point;
        throw new FormatException($"Can't parse({(string?)reader.Value}) as Point2D!");
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        value ??= new Point3D();
        var p = (Point3D)value;
        writer.WriteRawValue($"[{p.X}, {p.Y}, {p.Z}]");
        // [[0, 0],[0, 0]] // runtime exception if use method WriteRaw()
        // [[0, 0][0, 0]]
    }
}

[JsonConverter(typeof(Point3DJsonConverter))]
public readonly record struct Point3D(double X, double Y, double Z)
{
    public override string ToString() => $"{X} {Y} {Z}";

    public double this[int index] => index switch
    {
        0 => X,
        1 => Y,
        2 => Z,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Invalid point index")
    };

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D operator +(Point3D a, Point3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D operator -(Point3D a, Point3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point3D operator *(double b, Point3D a) => new(a.X * b, a.Y * b, a.Z * b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Point3D((double x, double y, double z) tuple) => new(tuple.x, tuple.y, tuple.z);
}