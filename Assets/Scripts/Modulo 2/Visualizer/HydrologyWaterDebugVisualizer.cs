using UnityEngine;
using UnityEngine.AdaptivePerformance.Provider;
using UnityEngine.Rendering;

public class HydrologyWaterDebugVisualizer : MonoBehaviour
{
    [Header("Referencias")]
    public Material waterMaterial;

    [Header("Visual")]
    public float yOffset = 0.08f;

    [Header("Optimización")]
    public int sampleEvery = 4;
    public float minWaterToShow = 0.001f;
    public float waterForFullAlpha = 0.2f;

    private HydrologyInputData data;
    private HydrologyEngine engine;

    private Mesh mesh;
    private Texture2D waterTexture;
    private Color32[] pixels;

    private int texWidth;
    private int texHeight;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    public void Initialize(
        HydrologyInputData inputData,
        HydrologyEngine hydrologyEngine,
        MeshFilter demMeshFilter
    )
    {
        data = inputData;
        engine = hydrologyEngine;

        if (data == null)
        {
            Debug.LogError("HydrologyWaterDebugVisualizer: inputData es null.");
            return;
        }

        if (engine == null)
        {
            Debug.LogError("HydrologyWaterDebugVisualizer: hydrologyEngine es null.");
            return;
        }

        if (demMeshFilter == null || demMeshFilter.sharedMesh == null)
        {
            Debug.LogError("HydrologyWaterDebugVisualizer: MeshFilter del DEM inválido.");
            return;
        }

        if (sampleEvery < 1)
            sampleEvery = 1;

        texWidth = Mathf.CeilToInt(data.Width / (float)sampleEvery);
        texHeight = Mathf.CeilToInt(data.Height / (float)sampleEvery);

        SetupComponents();
        AttachToDem(demMeshFilter);
        CreateOverlayMeshFromDem(demMeshFilter);
        CreateTexture();

        Refresh();
    }

    private void SetupComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (waterMaterial != null)
            meshRenderer.material = new Material(waterMaterial);
    }

    private void AttachToDem(MeshFilter demMeshFilter)
    {
        transform.SetParent(demMeshFilter.transform, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    private void CreateOverlayMeshFromDem(MeshFilter demMeshFilter)
    {
        Mesh sourceMesh = demMeshFilter.sharedMesh;

        mesh = new Mesh();
        mesh.name = "Hydrology Water Overlay Mesh";
        mesh.indexFormat = IndexFormat.UInt32;

        Vector3[] vertices = sourceMesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += Vector3.up * yOffset;
        }

        mesh.vertices = vertices;

        if (sourceMesh.uv != null && sourceMesh.uv.Length == sourceMesh.vertexCount)
        {
            mesh.uv = sourceMesh.uv;
        }
        else
        {
            mesh.uv = GeneratePlanarUVs(vertices);
        }

        mesh.subMeshCount = sourceMesh.subMeshCount;

        for (int i = 0; i < sourceMesh.subMeshCount; i++)
        {
            mesh.SetTriangles(sourceMesh.GetTriangles(i), i);
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }

    private Vector2[] GeneratePlanarUVs(Vector3[] vertices)
    {
        Vector2[] uvs = new Vector2[vertices.Length];

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];

            if (v.x < minX) minX = v.x;
            if (v.x > maxX) maxX = v.x;
            if (v.z < minZ) minZ = v.z;
            if (v.z > maxZ) maxZ = v.z;
        }

        float sizeX = Mathf.Max(maxX - minX, 0.0001f);
        float sizeZ = Mathf.Max(maxZ - minZ, 0.0001f);

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];

            uvs[i] = new Vector2(
                (v.x - minX) / sizeX,
                (v.z - minZ) / sizeZ
            );
        }

        return uvs;
    }

    private void CreateTexture()
    {
        waterTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        waterTexture.filterMode = FilterMode.Point;
        waterTexture.wrapMode = TextureWrapMode.Clamp;

        pixels = new Color32[texWidth * texHeight];

        if (meshRenderer != null && meshRenderer.material != null)
            meshRenderer.material.mainTexture = waterTexture;
    }

    public void Refresh()
    {
        if (data == null || engine == null || waterTexture == null || pixels == null)
            return;

        float[,] water = engine.State.SurfaceWater;

        for (int y = 0; y < texHeight; y++)
        {
            for (int x = 0; x < texWidth; x++)
            {
                int sourceX = Mathf.Min(x * sampleEvery, data.Width - 1);
                int sourceY = Mathf.Min(y * sampleEvery, data.Height - 1);

                int index = y * texWidth + x;

                bool insideDem =
                    data.DemMask == null ||
                    data.DemMask[sourceX, sourceY] != 0;

                if (!insideDem)
                {
                    pixels[index] = new Color32(0, 0, 0, 0);
                    continue;
                }

                float waterDepth = water[sourceX, sourceY];

                if (waterDepth < minWaterToShow)
                {
                    pixels[index] = new Color32(0, 0, 0, 0);
                    continue;
                }

                float alpha01 = Mathf.Clamp01(waterDepth / waterForFullAlpha);
                byte alpha = (byte)(alpha01 * 180f);

                pixels[index] = new Color32(0, 120, 255, alpha);
            }
        }

        waterTexture.SetPixels32(pixels);
        waterTexture.Apply(false);
    }

    public void Clear()
    {
        if (meshFilter != null)
            meshFilter.sharedMesh = null;

        if (meshRenderer != null)
            meshRenderer.enabled = false;

        mesh = null;
        waterTexture = null;
        pixels = null;
    }
}