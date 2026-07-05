using UnityEngine;

public class HydrologyConfig
{
    public float TimeStep = 1f;

    public float RainfallIntensity = 0.01f;

    public float SurfaceFlowRate = 0.5f;

    public float MinWaterDepthToFlow = 0.001f;

    public float AquiferRechargeRate = 0.1f;

    public bool EnableRainfall = true;

    public bool EnableSurfaceFlow = true;

    public bool EnableLakeStorage = false;

    public bool EnableAquiferRecharge = false;
    
    public bool EnableBasinRouting = true;
}
