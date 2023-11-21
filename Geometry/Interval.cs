using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SphereProblem.Geometry;

public class IntervalJsonConverter : JsonConverter<Interval>
{
    public override Interval Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
            {
                // // 1 method
                // var array = JsonSerializer.Deserialize<double[]>(ref reader, options)!;
                //
                // if (array.Length != 2) throw new JsonException($"Wrong interval length({array.Length})!");
                // return new(array[0], array[1]);

                // 2 method
                var array = JsonNode.Parse(ref reader)?.AsArray() ??
                            throw new JsonException("Can't parse interval!");

                if (array.Count == 2)
                {
                    return new(array[0]!.GetValue<double>(), array[1]!.GetValue<double>());
                }

                throw new JsonException($"Wrong interval length({array.Count})!");
            }
            case JsonTokenType.String:
            {
                var line = reader.GetString();
                if (Interval.TryParse(line ?? string.Empty, out var interval)) return interval;
                break;
            }
            case JsonTokenType.StartObject:
            {
                while (reader.Read())
                {
                    if (reader.TokenType != JsonTokenType.String) continue;
                    var line = reader.GetString();
                    if (!Interval.TryParse(line ?? string.Empty, out var interval)) continue;
                    reader.Read(); // for getting EndObject
                    return interval;
                }

                break;
            }
            default:
                throw new NotSupportedException();
        }

        throw new FormatException("Can't parse as Vector3D!");
    }

    public override void Write(Utf8JsonWriter writer, Interval value, JsonSerializerOptions options)
    {
        writer.WriteRawValue($"[{value.LeftBorder}, {value.RightBorder}]");
        // JsonSerializer.Serialize(writer, new[] { value.X, value.Y, value.Z }, options);
    }
}

[JsonConverter(typeof(IntervalJsonConverter))]
[method: JsonConstructor]
public readonly record struct Interval([property: JsonPropertyName("Left border")]
    double LeftBorder, [property: JsonPropertyName("Right border")]
    double RightBorder)
{
    [JsonIgnore] public double Length { get; } = Math.Abs(RightBorder - LeftBorder);

    public static bool TryParse(string line, out Interval interval)
    {
        var words = line.Split(new[] { ' ', ',', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length != 2 || !double.TryParse(words[0], CultureInfo.InvariantCulture, out var x) ||
            !double.TryParse(words[1], CultureInfo.InvariantCulture, out var y))
        {
            interval = default;
            return false;
        }

        interval = new(x, y);
        return true;
    }
}