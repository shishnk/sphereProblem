namespace SphereProblem.SphereMeshContext;

public class SphereMeshManager(SphereMeshBuilder builder)
{
    public SphereMeshBuilder Builder => builder;

    public SphereMesh GetMeshInstance()
    {
        builder.CreatePoints();
        builder.CreateElements();
        return builder.GetMeshInstance();
    }
}