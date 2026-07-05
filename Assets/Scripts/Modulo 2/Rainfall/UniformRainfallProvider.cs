using UnityEngine;

public class UniformRainfallProvider : IRainfallProvider
{
    private readonly float intensity;

    public UniformRainfallProvider(float intensity)
    {
        this.intensity = intensity;
    }

    public float GetRainfallAt(int row, int column, float time)
    {
        return intensity;
    }
}
