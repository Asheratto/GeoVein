using System;
using UnityEngine;

public class GeneratedPlaneHydrologyAdapter : IHydrologyPlane
{
    private readonly float[,] heightMap;

    public int Rows { get; }
    public int Columns { get; }
    public float CellSize { get; }

    public GeneratedPlaneHydrologyAdapter(float[,] heightMap, float cellSize)
    {
        if (heightMap == null)
            throw new ArgumentNullException(nameof(heightMap));

        if (cellSize <= 0f)
            throw new ArgumentException("Cell size must be greater than zero.");

        this.heightMap = heightMap;

        Rows = heightMap.GetLength(0);
        Columns = heightMap.GetLength(1);
        CellSize = cellSize;
    }

    public float GetElevation(int row, int column)
    {
        if (!IsInside(row, column))
            throw new IndexOutOfRangeException($"Cell [{row}, {column}] is outside the hydrology plane.");

        return heightMap[row, column];
    }

    public bool IsInside(int row, int column)
    {
        return row >= 0 &&
               row < Rows &&
               column >= 0 &&
               column < Columns;
    }
}
