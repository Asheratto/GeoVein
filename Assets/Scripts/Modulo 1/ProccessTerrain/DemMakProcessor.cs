using System.Collections.Generic;
using UnityEngine;

public static class DemMaskProcessor
{
    private const float Epsilon = 0.00001f;

    private struct MaskTriangle
    {
        public Vector2 A;
        public Vector2 B;
        public Vector2 C;
        public Rect Bounds;
    }

    private class SpatialIndex
    {
        public readonly List<MaskTriangle> Triangles = new List<MaskTriangle>();
        public readonly Dictionary<Vector2Int, List<int>> Grid = new Dictionary<Vector2Int, List<int>>();

        public float CellSize;

        public SpatialIndex(float cellSize)
        {
            CellSize = Mathf.Max(0.001f, cellSize);
        }
    }

    public static MeshData FilterDemByDataMeshes( MeshData dem, List<MeshData> dataMeshes, float cellSize = 20f, float padding = 0f)
    {
        if (dem == null)
            return null;

        if (dataMeshes == null || dataMeshes.Count == 0)
            return dem;

        SpatialIndex index = BuildSpatialIndex(dataMeshes, cellSize, padding);

        if (index.Triangles.Count == 0)
            return dem;

        Vector3[] oldVertices = dem.vertices;
        int[] oldTriangles = dem.triangles;

        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();

        Dictionary<int, int> vertexMap = new Dictionary<int, int>();

        for (int i = 0; i < oldTriangles.Length; i += 3)
        {
            int ia = oldTriangles[i];
            int ib = oldTriangles[i + 1];
            int ic = oldTriangles[i + 2];

            Vector3 a = oldVertices[ia];
            Vector3 b = oldVertices[ib];
            Vector3 c = oldVertices[ic];

            Vector3 center3D = (a + b + c) / 3f;
            Vector2 centerXZ = new Vector2(center3D.x, center3D.z);

            if (!IsPointInsideSpatialIndex(centerXZ, index, padding))
                continue;

            AddTriangle(
                ia,
                ib,
                ic,
                oldVertices,
                newVertices,
                newTriangles,
                vertexMap
            );
        }

        MeshData filteredDem = new MeshData();

        filteredDem.Source = dem.Source;
        filteredDem.width = dem.width;
        filteredDem.height = dem.height;
        filteredDem.centro = dem.centro;

        filteredDem.vertices = newVertices.ToArray();
        filteredDem.triangles = newTriangles.ToArray();

        return filteredDem;
    }

    private static SpatialIndex BuildSpatialIndex(
        List<MeshData> dataMeshes,
        float cellSize,
        float padding
    )
    {
        SpatialIndex index = new SpatialIndex(cellSize);

        foreach (MeshData data in dataMeshes)
        {
            if (data == null)
                continue;

            if (!ShouldUseAsDemMask(data))
                continue;

            if (data.vertices == null || data.triangles == null)
                continue;

            Vector3[] vertices = data.vertices;
            int[] triangles = data.triangles;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector2 a = ToWorldXZ(data, vertices[triangles[i]]);
                Vector2 b = ToWorldXZ(data, vertices[triangles[i + 1]]);
                Vector2 c = ToWorldXZ(data, vertices[triangles[i + 2]]);

                Rect bounds = CreateTriangleBounds(a, b, c, padding);

                MaskTriangle triangle = new MaskTriangle
                {
                    A = a,
                    B = b,
                    C = c,
                    Bounds = bounds
                };

                int triangleIndex = index.Triangles.Count;
                index.Triangles.Add(triangle);

                AddTriangleToGrid(index, triangleIndex, bounds);
            }
        }

        return index;
    }

    private static bool ShouldUseAsDemMask(MeshData data)
    {
        if (data.Source is IRasterElement)
            return false;

        // Idealmente después lo dejas específico:
        // return data.Source is IAcuifero || data.Source is ILago;

        return true;
    }

    private static Vector2 ToWorldXZ(MeshData data, Vector3 localVertex)
    {
        return new Vector2(
            data.centro.x + localVertex.x,
            data.centro.y + localVertex.z
        );
    }

    private static void AddTriangleToGrid(
        SpatialIndex index,
        int triangleIndex,
        Rect bounds
    )
    {
        Vector2Int minCell = WorldToCell(index, new Vector2(bounds.xMin, bounds.yMin));
        Vector2Int maxCell = WorldToCell(index, new Vector2(bounds.xMax, bounds.yMax));

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);

                if (!index.Grid.TryGetValue(cell, out List<int> list))
                {
                    list = new List<int>();
                    index.Grid.Add(cell, list);
                }

                list.Add(triangleIndex);
            }
        }
    }

    private static bool IsPointInsideSpatialIndex(
        Vector2 point,
        SpatialIndex index,
        float padding
    )
    {
        Vector2Int cell = WorldToCell(index, point);

        if (!index.Grid.TryGetValue(cell, out List<int> candidateTriangles))
            return false;

        for (int i = 0; i < candidateTriangles.Count; i++)
        {
            MaskTriangle triangle = index.Triangles[candidateTriangles[i]];

            if (!triangle.Bounds.Contains(point))
                continue;

            if (PointInTriangle(point, triangle.A, triangle.B, triangle.C))
                return true;

            if (padding > 0f && PointNearTriangle(point, triangle, padding))
                return true;
        }

        return false;
    }

    private static Vector2Int WorldToCell(SpatialIndex index, Vector2 point)
    {
        return new Vector2Int(
            Mathf.FloorToInt(point.x / index.CellSize),
            Mathf.FloorToInt(point.y / index.CellSize)
        );
    }

    private static void AddTriangle(
        int ia,
        int ib,
        int ic,
        Vector3[] oldVertices,
        List<Vector3> newVertices,
        List<int> newTriangles,
        Dictionary<int, int> vertexMap
    )
    {
        newTriangles.Add(GetNewVertexIndex(ia, oldVertices, newVertices, vertexMap));
        newTriangles.Add(GetNewVertexIndex(ib, oldVertices, newVertices, vertexMap));
        newTriangles.Add(GetNewVertexIndex(ic, oldVertices, newVertices, vertexMap));
    }

    private static int GetNewVertexIndex(
        int oldIndex,
        Vector3[] oldVertices,
        List<Vector3> newVertices,
        Dictionary<int, int> vertexMap
    )
    {
        if (vertexMap.TryGetValue(oldIndex, out int newIndex))
            return newIndex;

        newIndex = newVertices.Count;
        vertexMap.Add(oldIndex, newIndex);
        newVertices.Add(oldVertices[oldIndex]);

        return newIndex;
    }

    private static Rect CreateTriangleBounds(
        Vector2 a,
        Vector2 b,
        Vector2 c,
        float padding
    )
    {
        float minX = Mathf.Min(a.x, b.x, c.x) - padding;
        float maxX = Mathf.Max(a.x, b.x, c.x) + padding;

        float minY = Mathf.Min(a.y, b.y, c.y) - padding;
        float maxY = Mathf.Max(a.y, b.y, c.y) + padding;

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    private static bool PointInTriangle(
        Vector2 p,
        Vector2 a,
        Vector2 b,
        Vector2 c
    )
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);

        bool hasNegative =
            d1 < -Epsilon ||
            d2 < -Epsilon ||
            d3 < -Epsilon;

        bool hasPositive =
            d1 > Epsilon ||
            d2 > Epsilon ||
            d3 > Epsilon;

        return !(hasNegative && hasPositive);
    }

    private static bool PointNearTriangle(
        Vector2 point,
        MaskTriangle triangle,
        float distance
    )
    {
        float sqrDistance = distance * distance;

        float d1 = DistancePointToSegmentSqr(point, triangle.A, triangle.B);
        float d2 = DistancePointToSegmentSqr(point, triangle.B, triangle.C);
        float d3 = DistancePointToSegmentSqr(point, triangle.C, triangle.A);

        return d1 <= sqrDistance || d2 <= sqrDistance || d3 <= sqrDistance;
    }

    private static float DistancePointToSegmentSqr(
        Vector2 point,
        Vector2 a,
        Vector2 b
    )
    {
        Vector2 ab = b - a;

        float abLengthSqr = ab.sqrMagnitude;

        if (abLengthSqr <= Epsilon)
            return (point - a).sqrMagnitude;

        float t = Vector2.Dot(point - a, ab) / abLengthSqr;
        t = Mathf.Clamp01(t);

        Vector2 closest = a + ab * t;

        return (point - closest).sqrMagnitude;
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return
            (p1.x - p3.x) * (p2.y - p3.y) -
            (p2.x - p3.x) * (p1.y - p3.y);
    }
}

