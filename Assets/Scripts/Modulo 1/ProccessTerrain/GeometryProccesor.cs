using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryProccesor : MonoBehaviour
{
    public static GeometryProccesor instance;

    [Header("Datos")]
    public List<IElementMap> scene = new List<IElementMap>(128);
    public List<MeshData> objetos = new List<MeshData>(128);
    public List<MeshData> objetosProcesados = new List<MeshData>(128);

    [Header("Procesamiento")]
    [SerializeField] private float simplificationThreshold = 0.000001f;
    [SerializeField] private float normalizerScale = 1000f;
    [SerializeField] private float heightScale = 10f;

    [Header("Extrusión")]
    [SerializeField] private bool extrudeDem = true;
    [SerializeField] private float meshExtrusionHeight = 10f;
    [SerializeField] private float demExtrusionHeight = 1f;

    [Header("Performance")]
    [SerializeField] private float frameBudget = 0.01f;

    [Header("Perfiles")]
    [SerializeField] private GeometryProfileResolver profileResolver;

    private readonly List<IGeometricElement> geos = new List<IGeometricElement>(128);

    private readonly List<MeshData> lakeMeshes = new List<MeshData>(32);
    private readonly List<MeshData> aquiferMeshes = new List<MeshData>(32);
    private readonly List<MeshData> basinMeshes = new List<MeshData>(32);

    private Coroutine processCoroutine;

    private IRasterElement raster;
    private GeoNormalizer normalizer;
    private Texture2D heightMap;
    private MeshData demData;

    private HydrologyInputData hydroInput;
    private bool hasHydroInput;

    private float lastYieldTime;

    private List<Vector2> simplifyResult;
    private Action<List<Vector2>> simplifyCallback;

    private float debugStartTime;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        simplifyCallback = OnSimplifyFinished;
    }

    private void DebugStepStart(string stepName)
    {
        debugStartTime = Time.realtimeSinceStartup;
        Debug.Log($"[GEOMETRY] >>> INICIO {stepName}");
    }

    private void DebugStepEnd(string stepName)
    {
        float elapsed = Time.realtimeSinceStartup - debugStartTime;
        Debug.Log($"[GEOMETRY] <<< FIN {stepName} | {elapsed:0.000}s");
    }

    private void OnSimplifyFinished(List<Vector2> result)
    {
        simplifyResult = result;
    }

    public void SaveData(List<IElementMap> objects)
    {
        scene.Clear();

        if (objects != null)
            scene.AddRange(objects);

        if (StatusUI.instance != null)
            StatusUI.instance.SetStatusAquifero(true);
    }

    public void DeleteData()
    {
        StopCurrentProcess();

        ClearRuntimeData();

        if (SceneRenderer.instance != null)
            SceneRenderer.instance.Clear();

        if (HydrologySimulator.instance != null)
            HydrologySimulator.instance.DeleteData();

        if (BottomTargetNavigator.instance != null)
            BottomTargetNavigator.instance.Clear();

        if (LoadingUI.instance != null)
            LoadingUI.instance.Hide();

        if (StatusUI.instance != null)
        {
            StatusUI.instance.SetStatusAquifero(false);
            StatusUI.instance.SetStatusModelo3D(false);
            StatusUI.instance.SetStatusSim(false);
        }
    }

    private void ClearRuntimeData()
    {
        scene.Clear();

        objetos.Clear();
        objetosProcesados.Clear();

        geos.Clear();

        lakeMeshes.Clear();
        aquiferMeshes.Clear();
        basinMeshes.Clear();

        raster = null;
        normalizer = null;
        heightMap = null;
        demData = null;

        hydroInput = null;
        hasHydroInput = false;

        simplifyResult = null;

        lastYieldTime = 0f;
    }

    public void ProccesGeometry()
    {
        if (scene.Count == 0)
            return;

        StopCurrentProcess();

        processCoroutine = StartCoroutine(ProccesGeometryCoroutine());
    }

    private void StopCurrentProcess()
    {
        if (processCoroutine != null)
        {
            StopCoroutine(processCoroutine);
            processCoroutine = null;
        }
    }

    private IEnumerator ProccesGeometryCoroutine()
    {
        LoadingUI.instance.Show();
        DebugStepStart("ResetWorkingData");
        ResetWorkingData();
        DebugStepEnd("ResetWorkingData");
        yield return null;

        DebugStepStart("CollectSceneElements");
        CollectSceneElements();
        DebugStepEnd("CollectSceneElements");
        yield return null;

        if (raster == null)
        {
            Debug.LogError("No hay raster/DEM en la escena.");
            processCoroutine = null;
            yield break;
        }

        if (profileResolver == null)
        {
            Debug.LogError("GeometryProccesor no tiene GeometryProfileResolver asignado.");
            processCoroutine = null;
            yield break;
        }

        DebugStepStart("GeoNormalizer");
        normalizer = new GeoNormalizer(
            geos,
            raster,
            normalizerScale,
            heightScale
        );
        DebugStepEnd("GeoNormalizer");
        yield return null;

        DebugStepStart("BuildFlatElementMeshes");
        yield return StartCoroutine(BuildFlatElementMeshes());
        DebugStepEnd("BuildFlatElementMeshes");
        yield return null;

        DebugStepStart("BuildDemMesh");
        yield return StartCoroutine(BuildDemMesh());
        DebugStepEnd("BuildDemMesh");
        yield return null;

        if (demData == null)
        {
            Debug.LogError("No se pudo generar el MeshData del DEM.");
            processCoroutine = null;
            yield break;
        }

        DebugStepStart("SplitMeshesByType");
        SplitMeshesByType();
        DebugStepEnd("SplitMeshesByType");
        yield return null;

        DebugStepStart("BuildHydrologyInput");
        yield return StartCoroutine(BuildHydrologyInput());
        DebugStepEnd("BuildHydrologyInput");
        yield return null;

        DebugStepStart("BuildVisualDem");
        yield return StartCoroutine(BuildVisualDem());
        DebugStepEnd("BuildVisualDem");
        yield return null;

        DebugStepStart("BuildProcessedMeshes");
        yield return StartCoroutine(BuildProcessedMeshes());
        DebugStepEnd("BuildProcessedMeshes");
        yield return null;

        DebugStepStart("RenderScene");
        yield return StartCoroutine(RenderScene());
        DebugStepEnd("RenderScene");
        yield return null;

        DebugStepStart("SendHydrologyInput");
        SendHydrologyInput();
        DebugStepEnd("SendHydrologyInput");

        processCoroutine = null;
    }

    private void ResetWorkingData()
    {
        lastYieldTime = Time.realtimeSinceStartup;

        geos.Clear();

        lakeMeshes.Clear();
        aquiferMeshes.Clear();
        basinMeshes.Clear();

        objetos.Clear();
        objetosProcesados.Clear();

        raster = null;
        normalizer = null;
        heightMap = null;
        demData = null;

        hasHydroInput = false;
        simplifyResult = null;
    }

    private void CollectSceneElements()
    {
        for (int i = 0; i < scene.Count; i++)
        {
            IElementMap element = scene[i];

            if (element is IGeometricElement geo)
                geos.Add(geo);

            if (element is IRasterElement r)
                raster = r;
        }
    }

    private GeometryLayerProfile GetProfileForSource(object source)
    {
        if (source == null)
            return null;

        if (profileResolver == null)
        {
            Debug.LogWarning("GeometryProccesor no tiene GeometryProfileResolver asignado.");
            return null;
        }

        string profileId = GeoVeinGeometryAdapter.ResolveProfileId(source);

        GeometryLayerProfile profile = profileResolver.GetProfile(profileId);

        if (profile == null)
            Debug.LogWarning($"No existe perfil de procesamiento para ProfileId: {profileId}");

        return profile;
    }

    private IEnumerator BuildFlatElementMeshes()
    {
        for (int i = 0; i < geos.Count; i++)
        {
            IGeometricElement geo = geos[i];

            GeometryLayerProfile profile = GetProfileForSource(geo);

            if (profile == null || !profile.ProcessGeometry)
                continue;

            List<Vector2> vertices = geo.GetVertices();

            if (vertices == null || vertices.Count < 3)
                continue;

            simplifyResult = null;

            if (profile.UseSimplification)
            {
                yield return StartCoroutine(
                    SimplifyVW_Heap.Simplify(
                        vertices,
                        profile.SimplificationThreshold,
                        simplifyCallback
                    )
                );
            }
            else
            {
                simplifyResult = vertices;
            }

            List<Vector2> simplified = simplifyResult;

            if (simplified == null || simplified.Count < 3)
                continue;

            Vector2 center = normalizer.Center(simplified);
            List<Vector2> normalized = normalizer.NormalizeLocalToVector3List(simplified);

            MeshData meshData = GenerateDataMesh.Triangulate(normalized);

            if (meshData != null)
            {
                meshData.Source = geo;
                meshData.centro = center;
                meshData.contour = normalized;

                objetos.Add(meshData);
            }

            if (ShouldYield())
                yield return null;
        }
    }

    private IEnumerator BuildDemMesh()
    {
        heightMap = raster.GetHeightMap();

        if (heightMap == null)
        {
            Debug.LogError("El raster no tiene heightmap.");
            yield break;
        }

        List<Vector3> normalizedDemVertices = normalizer.NormalizeVertices();

        MeshData hpMeshData = GenerateDataMesh.MeshDemTriangulate(
            normalizedDemVertices,
            heightMap.width,
            heightMap.height
        );

        if (hpMeshData == null)
            yield break;

        hpMeshData.Source = raster;
        hpMeshData.width = heightMap.width;
        hpMeshData.height = heightMap.height;

        demData = hpMeshData;

        if (ShouldYield())
            yield return null;
    }

    private void SplitMeshesByType()
    {
        lakeMeshes.Clear();
        aquiferMeshes.Clear();
        basinMeshes.Clear();

        for (int i = 0; i < objetos.Count; i++)
        {
            MeshData obj = objetos[i];

            if (obj == null || obj.Source == null)
                continue;

            if (obj.Source is IAcuifero)
                aquiferMeshes.Add(obj);

            if (obj.Source is ICuenca)
                basinMeshes.Add(obj);

            if (obj.Source is ILagos)
                lakeMeshes.Add(obj);
        }
    }

    private IEnumerator BuildHydrologyInput()
    {
        int[,] lakeIds = null;
        int[,] aquiferIds = null;
        int[,] basinIds = null;
        int[,] demMask = null;

        yield return IdMaskBuilder.CreateIdMaskFromMeshDataListCoroutine(
            demData,
            lakeMeshes,
            result => lakeIds = result,
            rowsPerYield: 16
        );

        yield return IdMaskBuilder.CreateIdMaskFromMeshDataListCoroutine(
            demData,
            aquiferMeshes,
            result => aquiferIds = result,
            rowsPerYield: 16
        );

        yield return IdMaskBuilder.CreateIdMaskFromMeshDataListCoroutine(
            demData,
            basinMeshes,
            result => basinIds = result,
            rowsPerYield: 16
        );

        yield return IdMaskBuilder.CreateBinaryMaskFromDemCoroutine(
            demData,
            objetos,
            result => demMask = result,
            rowsPerYield: 16
        );

        if (ShouldYield())
            yield return null;

        float[,] heightMapArray = HeightMapConverter.TextureToFloatArray(
            heightMap,
            heightScale
        );

        if (ShouldYield())
            yield return null;

        hydroInput = new HydrologyInputData
        {
            HeightMap = heightMapArray,

            DemMask = demMask,
            LakeIds = lakeIds,
            AquiferIds = aquiferIds,
            BasinIds = basinIds,

            Width = heightMap.width,
            Height = heightMap.height
        };

        hasHydroInput = true;

        if (ShouldYield())
            yield return null;
    }

    private IEnumerator BuildVisualDem()
    {
        MeshData visualDem = DemMaskProcessor.FilterDemByDataMeshes(
            demData,
            objetos,
            padding: 0
        );

        if (visualDem != null)
        {
            visualDem.Source = raster;
            objetos.Add(visualDem);
        }

        if (ShouldYield())
            yield return null;
    }

    private IEnumerator BuildProcessedMeshes()
    {
        objetosProcesados.Clear();

        for (int i = 0; i < objetos.Count; i++)
        {
            MeshData obj = objetos[i];

            if (obj == null || obj.Source == null)
                continue;

            GeometryLayerProfile profile = GetProfileForSource(obj.Source);

            if (profile == null)
                continue;

            MeshData processed = obj;

            if (profile.ProcessGeometry && profile.UseExtrusion)
            {
                processed = ExtrudeMeshes.Extrude(
                    obj,
                    profile.ExtrusionHeight
                );
            }

            if (processed != null)
                objetosProcesados.Add(processed);

            if (ShouldYield())
                yield return null;
        }
    }

    private IEnumerator RenderScene()
    {
        if (SceneRenderer.instance == null)
        {
            Debug.LogError("No existe SceneRenderer.instance.");
            yield break;
        }

        SceneRenderer.instance.SetData(objetosProcesados);

        yield return StartCoroutine(SceneRenderer.instance.Draw());
    }

    private void SendHydrologyInput()
    {
        if (!hasHydroInput)
            return;

        if (HydrologySimulator.instance == null)
        {
            Debug.LogError("No existe HydrologySimulator.instance.");
            return;
        }

        HydrologySimulator.instance.ProccessInputData(hydroInput);
    }

    private bool ShouldYield()
    {
        if (Time.realtimeSinceStartup - lastYieldTime >= frameBudget)
        {
            lastYieldTime = Time.realtimeSinceStartup;
            return true;
        }

        return false;
    }
}