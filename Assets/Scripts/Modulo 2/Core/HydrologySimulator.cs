using UnityEngine;

public class HydrologyInputData
{
    public float[,] HeightMap;

    public int[,] LakeIds;
    public int[,] BasinIds;
    public int[,] AquiferIds;

    public int[,] DemMask;

    public int Width;
    public int Height;
}

public class HydrologySimulator : MonoBehaviour
{
    [SerializeField] private GameObject demParent;

    public static HydrologySimulator instance;

    private HydrologyEngine engine;
    private HydrologyInputData dataInput;
    private HydrologyConfig config;
    public HydrologyWaterDebugVisualizer waterVisualizer;

    private IRainfallProvider rainfall;

    private bool isInitialized;
    private bool isRunning;

    public bool IsInitialized => isInitialized;
    public bool IsRunning => isRunning;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void DeleteData()
    {
        isRunning = false;
        isInitialized = false;

        engine = null;
        dataInput = null;
        config = null;
        rainfall = null;

        if (waterVisualizer != null)
            waterVisualizer.Clear();

        if (StatusUI.instance != null)
            StatusUI.instance.SetStatusSim(false);

        Debug.Log("Datos hidrológicos eliminados.");
    }

    public void ProccessInputData(HydrologyInputData input)
    {
        isRunning = false;
        isInitialized = false;

        engine = null;
        config = null;
        rainfall = null;

        dataInput = input;

        if (StatusUI.instance != null)
            StatusUI.instance.SetStatusSim(true);

        HydrologyMaskDebugger.ValidateInput(input);

        Debug.Log("Datos hidrológicos recibidos correctamente.");
    }

    public void InitializeEngine()
    {
        if (dataInput == null)
        {
            Debug.LogError("No hay HydrologyInputData cargado.");
            return;
        }

        isRunning = false;
        isInitialized = false;

        if (waterVisualizer != null)
            waterVisualizer.Clear();

        IHydrologyPlane plane = new GeneratedPlaneHydrologyAdapter(
            dataInput.HeightMap,
            cellSize: 1f
        );

        config = new HydrologyConfig
        {
            EnableRainfall = true,
            EnableSurfaceFlow = true,
            EnableLakeStorage = true,
            EnableAquiferRecharge = true,
            EnableBasinRouting = true,

            TimeStep = 1f,
            RainfallIntensity = 0.01f,
            SurfaceFlowRate = 0.5f,
            MinWaterDepthToFlow = 0.001f,
            AquiferRechargeRate = 0.1f
        };

        rainfall = new UniformRainfallProvider(config.RainfallIntensity);

        engine = new HydrologyEngine(
            plane,
            config,
            rainfall,
            dataInput.LakeIds,
            dataInput.AquiferIds,
            dataInput.BasinIds
        );

        MeshFilter demMeshFilter = GetGeneratedDemMeshFilter();

        if (waterVisualizer != null)
            waterVisualizer.Initialize(dataInput, engine, demMeshFilter);

        isInitialized = true;
        isRunning = false;

        Debug.Log("HydrologyEngine inicializado.");
    }

    public bool Step()
    {
        if (!isInitialized || engine == null)
        {
            Debug.LogWarning("El engine no está inicializado.");
            return false;
        }

        bool result = engine.Step();

        if (result)
        {
            DrainOpenBoundaries(engine.State.SurfaceWater);

            if (waterVisualizer != null)
                waterVisualizer.Refresh();
        }

        return result;
    }

    private void DrainOpenBoundaries(float[,] water)
    {
        int width = water.GetLength(0);
        int height = water.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            water[x, 0] = 0f;
            water[x, height - 1] = 0f;
        }

        for (int y = 0; y < height; y++)
        {
            water[0, y] = 0f;
            water[width - 1, y] = 0f;
        }
    }

    public void Play()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("No puedes iniciar la simulación sin inicializar el engine.");
            return;
        }

        isRunning = true;
    }

    public void Pause()
    {
        isRunning = false;
    }

    public void TogglePlayPause()
    {
        if (isRunning)
            Pause();
        else
            Play();
    }

    public void ResetSimulation()
    {
        if (dataInput == null)
        {
            Debug.LogWarning("No hay datos hidrológicos para reiniciar.");
            return;
        }

        InitializeEngine();

        Debug.Log("Simulación reiniciada.");
    }

    public void UpdateConfig(
        float timeStep,
        float rainfallIntensity,
        float surfaceFlowRate,
        float aquiferRechargeRate,
        bool enableRainfall,
        bool enableSurfaceFlow,
        bool enableLakeStorage,
        bool enableAquiferRecharge,
        bool enableBasinRouting
    )
    {
        if (config == null)
            return;

        config.TimeStep = timeStep;
        config.RainfallIntensity = rainfallIntensity;
        config.SurfaceFlowRate = surfaceFlowRate;
        config.AquiferRechargeRate = aquiferRechargeRate;

        config.EnableRainfall = enableRainfall;
        config.EnableSurfaceFlow = enableSurfaceFlow;
        config.EnableLakeStorage = enableLakeStorage;
        config.EnableAquiferRecharge = enableAquiferRecharge;
        config.EnableBasinRouting = enableBasinRouting;
    }

    public float GetTotalSurfaceWater()
    {
        if (engine == null)
            return 0f;

        return engine.GetTotalSurfaceWater();
    }

    public float GetStoredLakeWater(int lakeId)
    {
        if (engine == null)
            return 0f;

        return engine.GetStoredLakeWater(lakeId);
    }

    public float GetAquiferRecharge(int aquiferId)
    {
        if (engine == null)
            return 0f;

        return engine.GetAquiferRecharge(aquiferId);
    }

    public float GetBasinSurfaceWater(int basinId)
    {
        if (engine == null)
            return 0f;

        return engine.GetBasinSurfaceWater(basinId);
    }

    public float GetTimeStep()
    {
        if (config == null)
            return 0f;

        return config.TimeStep;
    }

    public string GetSimulationStateText()
    {
        if (!isInitialized)
            return "No inicializado";

        return isRunning ? "Ejecutando" : "En pausa";
    }

    private MeshFilter GetGeneratedDemMeshFilter()
    {

        if (demParent == null)
        {
            Debug.LogError("No se encontró un objeto con tag DEM.");
            return null;
        }

        MeshFilter meshFilter = demParent.GetComponentInChildren<MeshFilter>();

        if (meshFilter == null)
        {
            Debug.LogError("El objeto DEM no tiene MeshFilter en sus hijos.");
            return null;
        }

        return meshFilter;
    }
}