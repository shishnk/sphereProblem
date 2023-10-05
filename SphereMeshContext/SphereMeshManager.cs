namespace SphereProblem.SphereMeshContext;

public class SphereMeshManager(BaseSphereMeshBuilder builder)
{
    public BaseSphereMeshBuilder Builder => builder;

    public SphereMesh GetMeshInstance()
    {
        builder.CreatePoints();
        builder.CreateElements();
        return builder.GetMeshInstance();
    }
}