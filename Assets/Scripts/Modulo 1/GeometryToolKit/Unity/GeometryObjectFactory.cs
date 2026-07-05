using UnityEngine;

public class GeometryObjectFactory : MonoBehaviour
{
    [Header("Etiquetas")]
    [SerializeField] private WorldLabel labelPrefab;

    public GameObject Create(
        GeometryRenderItem item,
        Mesh mesh,
        GeometryLayerProfile profile,
        Material material
    )
    {
        if (item == null || mesh == null || profile == null)
            return null;

        GameObject go = new GameObject(item.DisplayName);

        if (profile.Parent != null)
            go.transform.SetParent(profile.Parent);

        go.transform.position = GetPosition(item, profile);

        MeshFilter filter = go.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        renderer.material = material;

        if (profile.CreateMapTarget)
            CreateMapTarget(go, item, profile);

        if (profile.CreateLabel)
            CreateLabel(go, item, profile);

        return go;
    }

    private Vector3 GetPosition(GeometryRenderItem item, GeometryLayerProfile profile)
    {
        if (!profile.UseMeshCenterAsPosition)
        {
            return new Vector3(
                profile.PositionOffset.x,
                profile.HeightOffset + profile.PositionOffset.y,
                profile.PositionOffset.z
            );
        }

        return new Vector3(
            item.MeshData.centro.x + profile.PositionOffset.x,
            profile.HeightOffset + profile.PositionOffset.y,
            item.MeshData.centro.y + profile.PositionOffset.z
        );
    }

    private void CreateMapTarget(
        GameObject go,
        GeometryRenderItem item,
        GeometryLayerProfile profile
    )
    {
        MapTarget target = go.AddComponent<MapTarget>();
        //Card cardtarget = go.AddComponent<Card>();

        target.displayName = item.DisplayName;
        target.focusPoint = go.transform;
        //target.capa = profile.DisplayName;
        //target.ubi = item.Center;
    }

    private void CreateLabel(
        GameObject targetObject,
        GeometryRenderItem item,
        GeometryLayerProfile profile
    )
    {
        if (labelPrefab == null)
            return;

        WorldLabel label = Instantiate(labelPrefab, targetObject.transform);

        label.Setup(
            targetObject.transform,
            item.DisplayName,
            profile.DisplayName
        );
    }
}