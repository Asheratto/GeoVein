using UnityEngine;

public readonly struct FlowDirection
{
    public readonly int RowOffset;
    public readonly int ColumnOffset;

    public FlowDirection(int rowOffset, int columnOffset)
    {
        RowOffset = rowOffset;
        ColumnOffset = columnOffset;
    }

    public bool IsNone => RowOffset == 0 && ColumnOffset == 0;

    public static FlowDirection None => new FlowDirection(0, 0);
}

public class D8FlowDirectionModel
{
    private readonly int[] rowOffsets =
    {
            -1, -1, -1,
             0,      0,
             1,  1,  1
        };

    private readonly int[] columnOffsets =
    {
            -1, 0, 1,
            -1,    1,
            -1, 0, 1
        };

    public FlowDirection[,] Compute(IHydrologyPlane plane)
    {
        FlowDirection[,] directions = new FlowDirection[plane.Rows, plane.Columns];

        for (int row = 0; row < plane.Rows; row++)
        {
            for (int column = 0; column < plane.Columns; column++)
            {
                directions[row, column] = FindLowestNeighborDirection(plane, row, column);
            }
        }

        return directions;
    }

    private FlowDirection FindLowestNeighborDirection(
        IHydrologyPlane plane,
        int row,
        int column)
    {
        float currentElevation = plane.GetElevation(row, column);
        float lowestElevation = currentElevation;

        FlowDirection bestDirection = FlowDirection.None;

        for (int i = 0; i < rowOffsets.Length; i++)
        {
            int neighborRow = row + rowOffsets[i];
            int neighborColumn = column + columnOffsets[i];

            if (!plane.IsInside(neighborRow, neighborColumn))
                continue;

            float neighborElevation = plane.GetElevation(neighborRow, neighborColumn);

            if (neighborElevation < lowestElevation)
            {
                lowestElevation = neighborElevation;

                bestDirection = new FlowDirection(
                    rowOffsets[i],
                    columnOffsets[i]
                );
            }
        }

        return bestDirection;
    }
}
