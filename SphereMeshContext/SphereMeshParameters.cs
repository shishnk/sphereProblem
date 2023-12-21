using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using SphereProblem.Geometry;

namespace SphereProblem.SphereMeshContext;

// ReSharper disable once ClassNeverInstantiated.Global
public class SphereMeshParameters
{
    private readonly List<double> _radius = null!;
    private bool _isQuadratic;

    public Point3D Center { get; }

    public IReadOnlyList<double> Radius
    {
        get => _radius;
        private init => _radius = (value as List<double>)!;
    }
    
    public IReadOnlyList<double> NotChangedRadius { get; }

    [JsonPropertyName("Theta splits")]
    public int ThetaSplits { get; }

    [JsonPropertyName("Phi splits")]
    public int PhiSplits { get; }
    
    public IReadOnlyList<double> Properties { get; }

    [JsonConstructor]
    public SphereMeshParameters(Point3D center, List<double> radius,
        int thetaSplits,
        int phiSplits,
        int refinement, IReadOnlyList<double> properties, bool isQuadratic = false)
    {
        Center = center;
        Radius = radius;
        NotChangedRadius = radius.ToList();
        ThetaSplits = thetaSplits * (refinement + 1);
        PhiSplits = phiSplits * (refinement + 1);
        Properties = properties;
        _isQuadratic = isQuadratic;
        InsureRefinement(refinement);
    }

    private void InsureRefinement(int refinement)
    {
        if (_isQuadratic) refinement++;
        
        for (int i = 0; i < refinement; i++)
        {
            var count = _radius.Count - 1;

            for (var j = 0; j < count; j++)
            {
                _radius.Add((_radius[j] + _radius[j + 1]) / 2.0);
            }
        }

        _radius.Sort();
        
        // LINQ
        // _radius = Enumerable.Range(0, refinement)
        //     .Aggregate(_radius, (current, _) => current
        //         .Select((t, i) => i < current.Count - 1 ? new[] { t, (t + current[i + 1]) / 2.0 } : new[] { t })
        //         .SelectMany(x => x)
        //         .ToList())
        //     .OrderBy(x => x)
        //     .ToList();
    }

    public static SphereMeshParameters ReadFromJsonFile(string jsonPath)
    {
        if (!File.Exists(jsonPath)) throw new FileNotFoundException("File doesn't exist", jsonPath);

        using var sr = new StreamReader(jsonPath);

        return JsonSerializer.Deserialize<SphereMeshParameters?>(sr.ReadToEnd()) ??
               throw new JsonException("Bad sphere mesh parameters");
    }
}