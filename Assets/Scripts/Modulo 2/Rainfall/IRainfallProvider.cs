using UnityEngine;

public interface IRainfallProvider
{
    float GetRainfallAt(int row, int column, float time);
}
