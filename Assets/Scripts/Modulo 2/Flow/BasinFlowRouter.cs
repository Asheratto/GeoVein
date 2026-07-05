using System;
using System.Collections.Generic;
using System.Text;

public class BasinFlowRouter
{
    private readonly Dictionary<int, float> surfaceWaterByBasin = new Dictionary<int, float>();
    private readonly Dictionary<int, int> cellCountByBasin = new Dictionary<int, int>();

    public void Update(
    HydrologyState state,
    int[,] basinIds,
    int[,] demMask = null
)
    {
        if (basinIds.GetLength(0) != state.Rows ||
            basinIds.GetLength(1) != state.Columns)
        {
            throw new ArgumentException("Basin grid size must match hydrology state size.");
        }

        if (demMask != null &&
            (demMask.GetLength(0) != state.Rows ||
             demMask.GetLength(1) != state.Columns))
        {
            throw new ArgumentException("DEM mask size must match hydrology state size.");
        }

        surfaceWaterByBasin.Clear();
        cellCountByBasin.Clear();

        for (int row = 0; row < state.Rows; row++)
        {
            for (int column = 0; column < state.Columns; column++)
            {
                if (demMask != null && demMask[row, column] == 0)
                    continue;

                int basinId = basinIds[row, column];

                if (basinId <= 0)
                    continue;

                float water = state.GetSurfaceWater(row, column);

                if (!surfaceWaterByBasin.ContainsKey(basinId))
                {
                    surfaceWaterByBasin[basinId] = 0f;
                    cellCountByBasin[basinId] = 0;
                }

                surfaceWaterByBasin[basinId] += water;
                cellCountByBasin[basinId]++;
            }
        }
    }

    public float GetSurfaceWater(int basinId)
    {
        if (!surfaceWaterByBasin.ContainsKey(basinId))
            return 0f;

        return surfaceWaterByBasin[basinId];
    }

    public int GetCellCount(int basinId)
    {
        if (!cellCountByBasin.ContainsKey(basinId))
            return 0;

        return cellCountByBasin[basinId];
    }

    public string GetDebugSummary()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var pair in surfaceWaterByBasin)
        {
            int basinId = pair.Key;
            float water = pair.Value;
            int cells = GetCellCount(basinId);

            sb.AppendLine($"Cuenca {basinId}: {water:0.0000} agua superficial en {cells} celdas");
        }

        return sb.ToString();
    }
}