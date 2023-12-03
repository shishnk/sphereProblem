namespace SphereProblem.SphereMeshContext;

[Obsolete("Use UvSphereMeshManager instead")]
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

public class UvSphereMeshManager(BaseUvSphereMeshBuilder builder)
{
    public BaseUvSphereMeshBuilder Builder => builder;

    public SphereMesh GetMeshInstance()
    {
        builder.CreatePoints();
        builder.CreateElements();
        return builder.GetMeshInstance();
    }
}