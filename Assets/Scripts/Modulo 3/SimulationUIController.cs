using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HydrologySimulationUI : MonoBehaviour
{
    [Header("Configuración General")]
    [SerializeField] private TMP_InputField timeStepInput;

    [SerializeField] private Slider rainfallSlider;
    [SerializeField] private Slider surfaceFlowSlider;
    [SerializeField] private Slider infiltrationSlider;
    [SerializeField] private Slider lakeCaptureSlider;

    [Header("Textos de valores")]
    [SerializeField] private TMP_Text rainfallValueText;
    [SerializeField] private TMP_Text surfaceFlowValueText;
    [SerializeField] private TMP_Text infiltrationValueText;
    [SerializeField] private TMP_Text lakeCaptureValueText;

    [Header("Capas Activas")]
    [SerializeField] private Toggle rainToggle;
    [SerializeField] private Toggle surfaceFlowToggle;
    [SerializeField] private Toggle infiltrationToggle;
    [SerializeField] private Toggle lakeStorageToggle;

    [Header("Resultados")]
    [SerializeField] private TMP_Text timeStepResultText;
    [SerializeField] private TMP_Text simulatedTimeText;
    [SerializeField] private TMP_Text surfaceWaterText;
    [SerializeField] private TMP_Text infiltratedWaterText;
    [SerializeField] private TMP_Text lakeStorageText;
    [SerializeField] private TMP_Text basinWaterText;
    [SerializeField] private TMP_Text stateText;

    [Header("Controles")]
    [SerializeField] private TMP_Text playPauseButtonText;


    [Header("Opciones de simulación")]
    [SerializeField] private float realSecondsPerStep = 1f;
    [SerializeField] private int lakeIdToDisplay = 1;
    [SerializeField] private int aquiferIdToDisplay = 1;
    [SerializeField] private int basinIdToDisplay = 1;

    private float stepTimer;
    private float simulatedTime;

    private HydrologySimulator Simulator => HydrologySimulator.instance;

    private void Start()
    {
        RefreshSliderTexts();
        RefreshResultsUI();
        SetStateText("No inicializado");
    }

    private void Update()
    {
        if (Simulator == null)
            return;

        if (!Simulator.IsRunning)
            return;

        stepTimer += Time.deltaTime;

        if (stepTimer >= realSecondsPerStep)
        {
            stepTimer = 0f;
            RunStep();
        }
    }

    public void OnInitializeButton()
    {
        if (Simulator == null)
        {
            Debug.LogWarning("No existe HydrologySimulator en escena.");
            return;
        }

        ApplyConfigToSimulator();

        Simulator.InitializeEngine();

        simulatedTime = 0f;
        stepTimer = 0f;

        RefreshResultsUI();
        SetStateText("Inicializado");
    }

    public void OnStepButton()
    {
        RunStep();
    }

    public void OnPlayPauseButton()
    {
        if (Simulator == null)
            return;

        if (!Simulator.IsInitialized)
        {
            OnInitializeButton();
        }

        Simulator.TogglePlayPause();

        if (playPauseButtonText != null)
            playPauseButtonText.text = Simulator.IsRunning ? "||" : ">";

        SetStateText(Simulator.IsRunning ? "Ejecutando" : "En pausa");
    }

    public void OnResetButton()
    {
        if (Simulator == null)
            return;

        Simulator.ResetSimulation();

        simulatedTime = 0f;
        stepTimer = 0f;

        RefreshResultsUI();
        SetStateText("Reiniciado");

        if (playPauseButtonText != null)
            playPauseButtonText.text = "▶";
    }

    public void OnConfigChanged()
    {
        RefreshSliderTexts();

        if (Simulator == null)
            return;

        if (!Simulator.IsInitialized)
            return;

        ApplyConfigToSimulator();
        RefreshResultsUI();
    }

    private void RunStep()
    {
        if (Simulator == null)
        {
            Debug.LogWarning("No existe HydrologySimulator en escena.");
            return;
        }

        if (!Simulator.IsInitialized)
        {
            OnInitializeButton();
        }

        ApplyConfigToSimulator();

        bool result = Simulator.Step();

        if (result)
        {
            simulatedTime += GetTimeStepValue();
        }

        RefreshResultsUI();

        if (!Simulator.IsRunning)
            SetStateText(result ? "Step ejecutado" : "Step falló");
    }

    private void ApplyConfigToSimulator()
    {
        float timeStep = GetTimeStepValue();

        float rainfall = rainfallSlider != null ? rainfallSlider.value : 0f;
        float surfaceFlow = surfaceFlowSlider != null ? surfaceFlowSlider.value : 0f;
        float infiltration = infiltrationSlider != null ? infiltrationSlider.value : 0f;

        bool enableRainfall = rainToggle == null || rainToggle.isOn;
        bool enableSurfaceFlow = surfaceFlowToggle == null || surfaceFlowToggle.isOn;
        bool enableInfiltration = infiltrationToggle == null || infiltrationToggle.isOn;
        bool enableLakeStorage = lakeStorageToggle == null || lakeStorageToggle.isOn;

        Simulator.UpdateConfig(
            timeStep,
            rainfall,
            surfaceFlow,
            infiltration,
            enableRainfall,
            enableSurfaceFlow,
            enableLakeStorage,
            enableInfiltration,
            true
        );
    }

    private void RefreshResultsUI()
    {
        if (timeStepResultText != null)
            timeStepResultText.text = GetTimeStepValue().ToString("0.00") + " s";

        if (simulatedTimeText != null)
            simulatedTimeText.text = simulatedTime.ToString("0.00") + " s";

        if (Simulator == null || !Simulator.IsInitialized)
        {
            SetDefaultResults();
            return;
        }

        if (surfaceWaterText != null)
            surfaceWaterText.text = Simulator.GetTotalSurfaceWater().ToString("0.00") + " m³";

        if (infiltratedWaterText != null)
            infiltratedWaterText.text = Simulator.GetAquiferRecharge(aquiferIdToDisplay).ToString("0.00") + " m³";

        if (lakeStorageText != null)
            lakeStorageText.text = Simulator.GetStoredLakeWater(lakeIdToDisplay).ToString("0.00") + " m³";

        if (basinWaterText != null)
            basinWaterText.text = Simulator.GetBasinSurfaceWater(basinIdToDisplay).ToString("0.00") + " m³";
    }

    private void SetDefaultResults()
    {
        if (surfaceWaterText != null)
            surfaceWaterText.text = "0.00 m³";

        if (infiltratedWaterText != null)
            infiltratedWaterText.text = "0.00 m³";

        if (lakeStorageText != null)
            lakeStorageText.text = "0.00 m³";

        if (basinWaterText != null)
            basinWaterText.text = "0.00 m³";
    }

    private void RefreshSliderTexts()
    {
        if (rainfallValueText != null && rainfallSlider != null)
            rainfallValueText.text = rainfallSlider.value.ToString("0.00") + " mm/h";

        if (surfaceFlowValueText != null && surfaceFlowSlider != null)
            surfaceFlowValueText.text = (surfaceFlowSlider.value*100).ToString("0");

        if (infiltrationValueText != null && infiltrationSlider != null)
            infiltrationValueText.text = (infiltrationSlider.value*100).ToString("0");

        if (lakeCaptureValueText != null && lakeCaptureSlider != null)
            lakeCaptureValueText.text = (lakeCaptureSlider.value * 100).ToString("0");
    }

    private float GetTimeStepValue()
    {
        if (timeStepInput == null)
            return 1f;

        string rawText = timeStepInput.text;

        if (string.IsNullOrWhiteSpace(rawText))
            return 1f;

        rawText = rawText.Replace(",", ".");

        if (float.TryParse(rawText, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            return Mathf.Max(0.01f, value);
        }

        return 1f;
    }

    private void SetStateText(string state)
    {
        if (stateText != null)
            stateText.text = state;
    }
}