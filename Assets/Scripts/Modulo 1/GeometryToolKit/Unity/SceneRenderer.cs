using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRenderer : MonoBehaviour
{
    public static SceneRenderer instance;
    [SerializeField] private GeometryProfileResolver profileResolver;
    [SerializeField] private GeometryObjectFactory objectFactory;

    private List<GeometryRenderItem> sceneItems = new List<GeometryRenderItem>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void SetData(List<MeshData> objects)
    {
        sceneItems = GeoVeinGeometryAdapter.ToRenderItems(objects);
    }

    public IEnumerator Draw()
    {
        
        LoadingUI.instance.Show();

        if (BottomTargetNavigator.instance != null)
            BottomTargetNavigator.instance.Clear();

        foreach (GeometryRenderItem item in sceneItems)
        {
            Mesh mesh = UnityMeshFactory.Create(item.MeshData);

            if (mesh == null)
                continue;

            GeometryLayerProfile profile = profileResolver.GetProfile(item);

            if (profile == null)
                continue;

            Material material = profileResolver.CreateMaterial(item, profile);

            GameObject go = objectFactory.Create(item, mesh, profile, material);

            if (go == null)
                continue;

            RegisterNavigationTarget(go, item, profile);

            yield return null;
        }

        if (StatusUI.instance != null)
            StatusUI.instance.SetStatusModelo3D(true);

        if (LoadingUI.instance != null)
            LoadingUI.instance.Hide();
    }

    private void RegisterNavigationTarget(
    GameObject go,
    GeometryRenderItem item,
    GeometryLayerProfile profile
    )
    {
        if (BottomTargetNavigator.instance == null)
            return;

        MapTarget target = go.GetComponent<MapTarget>();

        if (target == null)
            return;

        string layerName = profile != null
            ? profile.DisplayName
            : item.LayerDisplayName;

        Card card = new Card(
            item.DisplayName,
            layerName,
            item.Center.ToString(),
            item.Source
        );

        BottomTargetNavigator.instance.AddTarget(target, card);
    }

    public void Clear()
    {
        StopAllCoroutines();

        if (profileResolver != null)
            profileResolver.ClearProfileParents();

        if (BottomTargetNavigator.instance != null)
            BottomTargetNavigator.instance.Clear();

        sceneItems.Clear();
    }
}

