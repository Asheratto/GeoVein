using UnityEngine;

public class SurfaceFlowModel
{
    public void Update(
    HydrologyState state,
    IHydrologyPlane plane,
    FlowDirection[,] directions,
    float flowRate,
    float minWaterToFlow,
    int[,] demMask = null)
    {
        float[,] nextWater = new float[state.Rows, state.Columns];

        for (int row = 0; row < state.Rows; row++)
        {
            for (int column = 0; column < state.Columns; column++)
            {
                if (!IsActiveCell(row, column, demMask))
                    continue;

                float water = state.SurfaceWater[row, column];

                if (water <= 0f)
                    continue;

                FlowDirection direction = directions[row, column];

                if (direction.IsNone || water < minWaterToFlow)
                {
                    nextWater[row, column] += water;
                    continue;
                }

                int targetRow = row + direction.RowOffset;
                int targetColumn = column + direction.ColumnOffset;

                if (!plane.IsInside(targetRow, targetColumn))
                {
                    nextWater[row, column] += water;
                    continue;
                }

                if (!IsActiveCell(targetRow, targetColumn, demMask))
                {
                    nextWater[row, column] += water;
                    continue;
                }

                float safeFlowRate = Clamp01(flowRate);
                float movedWater = water * safeFlowRate;
                float remainingWater = water - movedWater;

                nextWater[targetRow, targetColumn] += movedWater;
                nextWater[row, column] += remainingWater;
            }
        }

        for (int row = 0; row < state.Rows; row++)
        {
            for (int column = 0; column < state.Columns; column++)
            {
                if (!IsActiveCell(row, column, demMask))
                {
                    state.SurfaceWater[row, column] = 0f;
                    continue;
                }

                state.SurfaceWater[row, column] = nextWater[row, column];
            }
        }
    }

    private bool IsActiveCell(int row, int column, int[,] demMask)
    {
        if (demMask == null)
            return true;

        return demMask[row, column] != 0;
    }

    private float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        if (value > 1f) return 1f;
        return value;
    }
}
