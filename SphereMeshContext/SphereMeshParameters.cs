using System.Text.Json;
using System.Text.Json.Serialization;
using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

public record SphereMeshParameters(
    [property: JsonRequired] Point3D Center,
    [property: JsonRequired] IReadOnlyList<double> Radius,
    [property: JsonPropertyName("Theta splits"), JsonRequired]
    int ThetaSplits,
    [property: JsonPropertyName("Phi splits"), JsonRequired]
    int PhiSplits)
{
    public static SphereMeshParameters ReadFromJsonFile(string jsonPath)
    {
        if (!File.Exists(jsonPath)) throw new FileNotFoundException("File doesn't exist", jsonPath);

        using var sr = new StreamReader(jsonPath);

        return JsonSerializer.Deserialize<SphereMeshParameters?>(sr.ReadToEnd()) ??
               throw new JsonException("Bad sphere mesh parameters");
    }
}