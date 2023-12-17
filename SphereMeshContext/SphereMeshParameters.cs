using System.Text.Json;
using System.Text.Json.Serialization;
using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

public record SphereMeshParameters
{
    [JsonRequired] public Point3D Center { get; init; }
    [JsonRequired] public IReadOnlyList<double> Radius { get; init; }

    [JsonPropertyName("Theta splits"), JsonRequired]
    public int ThetaSplits { get; init; }

    [JsonPropertyName("Phi splits"), JsonRequired]
    public int PhiSplits { get; init; }

    [JsonRequired] public int Refinement { get; init; }

    public SphereMeshParameters(Point3D center, IReadOnlyList<double> radius,
        int thetaSplits,
        int phiSplits,
        int refinement)
    {
        Center = center;
        Radius = radius;
        ThetaSplits = thetaSplits * (refinement + 1);
        PhiSplits = phiSplits * (refinement + 1);
        Refinement = refinement;
    }

    public static SphereMeshParameters ReadFromJsonFile(string jsonPath)
    {
        if (!File.Exists(jsonPath)) throw new FileNotFoundException("File doesn't exist", jsonPath);

        using var sr = new StreamReader(jsonPath);

        return JsonSerializer.Deserialize<SphereMeshParameters?>(sr.ReadToEnd()) ??
               throw new JsonException("Bad sphere mesh parameters");
    }
}