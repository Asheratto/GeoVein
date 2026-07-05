using System;
using System.Collections.Generic;
using UnityEngine;

public class LakeStorageModel
{
    private readonly Dictionary<int, float> storedWaterByLake = new Dictionary<int, float>();

    public IReadOnlyDictionary<int, float> StoredWaterByLake => storedWaterByLake;

    public void StoreWaterInLakes(
    HydrologyState state,
    int[,] lakeIds,
    int[,] demMask = null
)
    {
        if (lakeIds.GetLength(0) != state.Rows ||
            lakeIds.GetLength(1) != state.Columns)
        {
            throw new ArgumentException("Lake grid size must match hydrology state size.");
        }

        if (demMask != null &&
            (demMask.GetLength(0) != state.Rows ||
             demMask.GetLength(1) != state.Columns))
        {
            throw new ArgumentException("DEM mask size must match hydrology state size.");
        }

        for (int row = 0; row < state.Rows; row++)
        {
            for (int column = 0; column < state.Columns; column++)
            {
                if (demMask != null && demMask[row, column] == 0)
                    continue;

                int lakeId = lakeIds[row, column];

                if (lakeId == 0)
                    continue;

                float water = state.SurfaceWater[row, column];

                if (water <= 0f)
                    continue;

                if (!storedWaterByLake.ContainsKey(lakeId))
                    storedWaterByLake[lakeId] = 0f;

                storedWaterByLake[lakeId] += water;

                state.SurfaceWater[row, column] = 0f;
            }
        }
    }

    public float GetStoredWater(int lakeId)
    {
        if (!storedWaterByLake.ContainsKey(lakeId))
            return 0f;

        return storedWaterByLake[lakeId];
    }
}
