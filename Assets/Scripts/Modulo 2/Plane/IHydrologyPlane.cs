using UnityEngine;

public interface IHydrologyPlane
{
    int Rows { get; }
    int Columns { get; }
    float CellSize { get; }

    float GetElevation(int row, int column);

    bool IsInside(int row, int column);
}