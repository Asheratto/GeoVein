using UnityEngine;

public class HydrologyState
{
    public int Rows { get; }
    public int Columns { get; }

    public float CurrentTime { get; private set; }

    public float[,] SurfaceWater { get; }

    public HydrologyState(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        SurfaceWater = new float[rows, columns];
        CurrentTime = 0f;
    }

    //Lluvia por tiempo
    public void AddRainfall(
    IRainfallProvider rainfallProvider,
    float deltaTime,
    int[,] demMask = null
)
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                if (demMask != null && demMask[row, column] == 0)
                    continue;

                float rainfall = rainfallProvider.GetRainfallAt(
                    row,
                    column,
                    CurrentTime
                );

                SurfaceWater[row, column] += rainfall * deltaTime;
            }
        }
    }

    //Setea el tiempo
    public void AdvanceTime(float deltaTime)
    {
        CurrentTime += deltaTime;
    }

    public float GetTotalSurfaceWater()
    {
        float total = 0f;

        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                total += SurfaceWater[row, column];
            }
        }

        return total;
    }
    public float GetSurfaceWater(int row, int column)
    {
        return SurfaceWater[row, column];
    }
}
