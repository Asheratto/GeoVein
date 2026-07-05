using System;
using System.Collections.Generic;
using UnityEngine;

public class AquiferRechargePort
{
    private readonly Dictionary<int, float> rechargeByAquifer = new Dictionary<int, float>();

    public IReadOnlyDictionary<int, float> RechargeByAquifer => rechargeByAquifer;

    public void RechargeAquifers(
    HydrologyState state,
    int[,] aquiferIds,
    float rechargeRate,
    int[,] demMask = null
)
    {
        if (aquiferIds.GetLength(0) != state.Rows ||
            aquiferIds.GetLength(1) != state.Columns)
        {
            throw new ArgumentException("Aquifer grid size must match hydrology state size.");
        }

        if (demMask != null &&
            (demMask.GetLength(0) != state.Rows ||
             demMask.GetLength(1) != state.Columns))
        {
            throw new ArgumentException("DEM mask size must match hydrology state size.");
        }

        rechargeRate = Clamp01(rechargeRate);

        for (int row = 0; row < state.Rows; row++)
        {
            for (int column = 0; column < state.Columns; column++)
            {
                if (demMask != null && demMask[row, column] == 0)
                    continue;

                int aquiferId = aquiferIds[row, column];

                if (aquiferId == 0)
                    continue;

                float surfaceWater = state.SurfaceWater[row, column];

                if (surfaceWater <= 0f)
                    continue;

                float rechargeAmount = surfaceWater * rechargeRate;

                if (!rechargeByAquifer.ContainsKey(aquiferId))
                    rechargeByAquifer[aquiferId] = 0f;

                rechargeByAquifer[aquiferId] += rechargeAmount;

                state.SurfaceWater[row, column] -= rechargeAmount;
            }
        }
    }

    public float GetRecharge(int aquiferId)
    {
        if (!rechargeByAquifer.ContainsKey(aquiferId))
            return 0f;

        return rechargeByAquifer[aquiferId];
    }

    private float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        if (value > 1f) return 1f;
        return value;
    }
}
