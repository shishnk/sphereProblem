using Newtonsoft.Json;
using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

public readonly record struct SphereMeshParameters(
    [JsonProperty(Required = Required.Always)]
    Point3D Center,
    [JsonProperty(Required = Required.Always)]
    double Radius,
    [JsonProperty("Theta splits", Required = Required.Always)]
    int ThetaSplits,
    [JsonProperty("Phi splits", Required = Required.Always)]
    int PhiSplits);