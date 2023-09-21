using Newtonsoft.Json;
using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

public readonly record struct SphereMeshParameters(
    [property: JsonProperty(Required = Required.Always)]
    Point3D Center,
    [property: JsonProperty(Required = Required.Always)]
    double Radius,
    [property: JsonProperty("Theta splits", Required = Required.Always)]
    int ThetaSplits,
    [property: JsonProperty("Phi splits", Required = Required.Always)]
    int PhiSplits)
{
    public static SphereMeshParameters ReadFromJsonFile(string jsonPath)
    {
        if (!File.Exists(jsonPath)) throw new FileNotFoundException("File doesn't exist", jsonPath);

        using var sr = new StreamReader(jsonPath);

        return JsonConvert.DeserializeObject<SphereMeshParameters?>(sr.ReadToEnd()) ??
               throw new JsonSerializationException("Bad sphere mesh parameters");
    }
}