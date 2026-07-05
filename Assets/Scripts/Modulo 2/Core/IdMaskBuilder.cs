using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IdMaskBuilder
{
    private const float EPSILON = 0.0001f;

    private struct PreparedPolygon
    {
        public int id;
        public Vector2[] polygon;
        public Rect bounds;

        public int xMin;
        public int xMax;
        public int zMin;
        public int zMax;
    }

    public static int[,] CreateIdMaskFromMeshDataList(
        MeshData demData,
        List<MeshData> elements,
        bool overwriteExistingIds = false
    )
    {
        if (!ValidateDem(demData))
            return null;

        int width = demData.width;
        int height = demData.height;

        int[,] idMask = new int[width, height];

        if (elements == null || elements.Count == 0)
            return idMask;

        List<PreparedPolygon> polygons = PreparePolygons(
            demData,
            elements,
            skipRasterElements: false
        );

        FillMask(
            demData,
            idMask,
            polygons,
            binaryMask: false,
            overwriteExistingIds: overwriteExistingIds
        );

        return idMask;
    }

    public static int[,] CreateBinaryMaskFromDem(
        MeshData demData,
        List<MeshData> elements
    )
    {
        if (!ValidateDem(demData))
            return null;

        int width = demData.width;
        int height = demData.height;

        int[,] mask = new int[width, height];

        if (elements == null || elements.Count == 0)
            return mask;

        List<PreparedPolygon> polygons = PreparePolygons(
            demData,
            elements,
            skipRasterElements: true
        );

        FillMask(
            demData,
            mask,
            polygons,
            binaryMask: true,
            overwriteExistingIds: true
        );

        return mask;
    }

    public static IEnumerator CreateIdMaskFromMeshDataListCoroutine(
        MeshData demData,
        List<MeshData> elements,
        Action<int[,]> onDone,
        bool overwriteExistingIds = false,
        int rowsPerYield = 16
    )
    {
        if (!ValidateDem(demData))
        {
            onDone?.Invoke(null);
            yield break;
        }

        int[,] idMask = new int[demData.width, demData.height];

        if (elements == null || elements.Count == 0)
        {
            onDone?.Invoke(idMask);
            yield break;
        }

        List<PreparedPolygon> polygons = PreparePolygons(
            demData,
            elements,
            skipRasterElements: false
        );

        yield return FillMaskCoroutine(
            demData,
            idMask,
            polygons,
            binaryMask: false,
            overwriteExistingIds: overwriteExistingIds,
            rowsPerYield: rowsPerYield
        );

        onDone?.Invoke(idMask);
    }

    public static IEnumerator CreateBinaryMaskFromDemCoroutine(
        MeshData demData,
        List<MeshData> elements,
        Action<int[,]> onDone,
        int rowsPerYield = 16
    )
    {
        if (!ValidateDem(demData))
        {
            onDone?.Invoke(null);
            yield break;
        }

        int[,] mask = new int[demData.width, demData.height];

        if (elements == null || elements.Count == 0)
        {
            onDone?.Invoke(mask);
            yield break;
        }

        List<PreparedPolygon> polygons = PreparePolygons(
            demData,
            elements,
            skipRasterElements: true
        );

        yield return FillMaskCoroutine(
            demData,
            mask,
            polygons,
            binaryMask: true,
            overwriteExistingIds: true,
            rowsPerYield: rowsPerYield
        );

        onDone?.Invoke(mask);
    }

    private static void FillMask(
        MeshData demData,
        int[,] mask,
        List<PreparedPolygon> polygons,
        bool binaryMask,
        bool overwriteExistingIds
    )
    {
        int width = demData.width;
        Vector3[] vertices = demData.vertices;

        for (int p = 0; p < polygons.Count; p++)
        {
            PreparedPolygon prepared = polygons[p];

            for (int z = prepared.zMin; z <= prepared.zMax; z++)
            {
                int rowIndex = z * width;

                for (int x = prepared.xMin; x <= prepared.xMax; x++)
                {
                    if (!overwriteExistingIds && mask[x, z] != 0)
                        continue;

                    int vertexIndex = rowIndex + x;

                    if (vertexIndex < 0 || vertexIndex >= vertices.Length)
                        continue;

                    Vector3 v = vertices[vertexIndex];

                    if (!ContainsInclusive(prepared.bounds, v.x, v.z))
                        continue;

                    if (PointInPolygon(v.x, v.z, prepared.polygon))
                    {
                        mask[x, z] = binaryMask ? 1 : prepared.id;
                    }
                }
            }
        }
    }

    private static IEnumerator FillMaskCoroutine(
        MeshData demData,
        int[,] mask,
        List<PreparedPolygon> polygons,
        bool binaryMask,
        bool overwriteExistingIds,
        int rowsPerYield
    )
    {
        int width = demData.width;
        Vector3[] vertices = demData.vertices;

        int processedRows = 0;

        for (int p = 0; p < polygons.Count; p++)
        {
            PreparedPolygon prepared = polygons[p];

            for (int z = prepared.zMin; z <= prepared.zMax; z++)
            {
                int rowIndex = z * width;

                for (int x = prepared.xMin; x <= prepared.xMax; x++)
                {
                    if (!overwriteExistingIds && mask[x, z] != 0)
                        continue;

                    int vertexIndex = rowIndex + x;

                    if (vertexIndex < 0 || vertexIndex >= vertices.Length)
                        continue;

                    Vector3 v = vertices[vertexIndex];

                    if (!ContainsInclusive(prepared.bounds, v.x, v.z))
                        continue;

                    if (PointInPolygon(v.x, v.z, prepared.polygon))
                    {
                        mask[x, z] = binaryMask ? 1 : prepared.id;
                    }
                }

                processedRows++;

                if (processedRows >= rowsPerYield)
                {
                    processedRows = 0;
                    yield return null;
                }
            }
        }
    }

    private static List<PreparedPolygon> PreparePolygons(
        MeshData demData,
        List<MeshData> elements,
        bool skipRasterElements
    )
    {
        List<PreparedPolygon> preparedPolygons = new List<PreparedPolygon>();

        if (elements == null)
            return preparedPolygons;

        for (int i = 0; i < elements.Count; i++)
        {
            MeshData element = elements[i];

            if (element == null)
                continue;

            if (skipRasterElements && element.Source is IRasterElement)
                continue;

            Vector2[] polygon = GetPolygon(element);

            if (polygon == null || polygon.Length < 3)
                continue;

            Rect bounds = GetBounds(polygon);

            CalculateIndexBounds(
                demData,
                bounds,
                out int xMin,
                out int xMax,
                out int zMin,
                out int zMax
            );

            if (xMin > xMax || zMin > zMax)
                continue;

            PreparedPolygon prepared = new PreparedPolygon
            {
                id = i + 1,
                polygon = polygon,
                bounds = bounds,
                xMin = xMin,
                xMax = xMax,
                zMin = zMin,
                zMax = zMax
            };

            preparedPolygons.Add(prepared);
        }

        return preparedPolygons;
    }

    private static void CalculateIndexBounds(
        MeshData demData,
        Rect bounds,
        out int xMin,
        out int xMax,
        out int zMin,
        out int zMax
    )
    {
        int width = demData.width;
        int height = demData.height;
        Vector3[] vertices = demData.vertices;

        if (width < 2 || height < 2 || vertices.Length < width * height)
        {
            xMin = 0;
            xMax = width - 1;
            zMin = 0;
            zMax = height - 1;
            return;
        }

        Vector3 origin = vertices[0];

        float xStep = vertices[1].x - origin.x;
        float zStep = vertices[width].z - origin.z;

        if (Mathf.Abs(xStep) < EPSILON || Mathf.Abs(zStep) < EPSILON)
        {
            xMin = 0;
            xMax = width - 1;
            zMin = 0;
            zMax = height - 1;
            return;
        }

        WorldRangeToIndexRange(
            bounds.xMin,
            bounds.xMax,
            origin.x,
            xStep,
            width,
            out xMin,
            out xMax
        );

        WorldRangeToIndexRange(
            bounds.yMin,
            bounds.yMax,
            origin.z,
            zStep,
            height,
            out zMin,
            out zMax
        );
    }

    private static void WorldRangeToIndexRange(
        float worldMin,
        float worldMax,
        float origin,
        float step,
        int count,
        out int indexMin,
        out int indexMax
    )
    {
        float last = origin + step * (count - 1);

        float gridMin = Mathf.Min(origin, last);
        float gridMax = Mathf.Max(origin, last);

        if (worldMax < gridMin || worldMin > gridMax)
        {
            indexMin = 1;
            indexMax = 0;
            return;
        }

        float absStep = Mathf.Abs(step);

        if (step > 0f)
        {
            indexMin = Mathf.FloorToInt((worldMin - origin) / absStep) - 1;
            indexMax = Mathf.CeilToInt((worldMax - origin) / absStep) + 1;
        }
        else
        {
            indexMin = Mathf.FloorToInt((origin - worldMax) / absStep) - 1;
            indexMax = Mathf.CeilToInt((origin - worldMin) / absStep) + 1;
        }

        indexMin = Mathf.Clamp(indexMin, 0, count - 1);
        indexMax = Mathf.Clamp(indexMax, 0, count - 1);
    }

    private static bool ValidateDem(MeshData demData)
    {
        if (demData == null)
        {
            Debug.LogError("IdMaskBuilder: demData es null.");
            return false;
        }

        if (demData.vertices == null || demData.vertices.Length == 0)
        {
            Debug.LogError("IdMaskBuilder: el DEM no tiene vertices.");
            return false;
        }

        if (demData.width <= 0 || demData.height <= 0)
        {
            Debug.LogError("IdMaskBuilder: width o height inválidos.");
            return false;
        }

        return true;
    }

    private static Vector2[] GetPolygon(MeshData meshData)
    {
        if (meshData == null)
            return null;

        if (meshData.contour != null && meshData.contour.Count >= 3)
            return meshData.contour.ToArray();

        if (meshData.vertices == null || meshData.vertices.Length < 3)
            return null;

        Vector2[] polygon = new Vector2[meshData.vertices.Length];

        for (int i = 0; i < meshData.vertices.Length; i++)
        {
            Vector3 v = meshData.vertices[i];
            polygon[i] = new Vector2(v.x, v.z);
        }

        return polygon;
    }

    private static bool PointInPolygon(float px, float py, Vector2[] polygon)
    {
        bool inside = false;

        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            Vector2 a = polygon[i];
            Vector2 b = polygon[j];

            if (PointOnSegment(px, py, a, b))
                return true;

            bool intersects =
                ((a.y > py) != (b.y > py)) &&
                (px < (b.x - a.x) * (py - a.y) / ((b.y - a.y) + Mathf.Epsilon) + a.x);

            if (intersects)
                inside = !inside;
        }

        return inside;
    }

    private static bool PointOnSegment(float px, float py, Vector2 a, Vector2 b)
    {
        float cross = (py - a.y) * (b.x - a.x) - (px - a.x) * (b.y - a.y);

        if (Mathf.Abs(cross) > EPSILON)
            return false;

        float dot =
            (px - a.x) * (b.x - a.x) +
            (py - a.y) * (b.y - a.y);

        if (dot < 0f)
            return false;

        float lengthSq = (b - a).sqrMagnitude;

        if (dot > lengthSq)
            return false;

        return true;
    }

    private static Rect GetBounds(Vector2[] points)
    {
        float minX = points[0].x;
        float maxX = points[0].x;
        float minY = points[0].y;
        float maxY = points[0].y;

        for (int i = 1; i < points.Length; i++)
        {
            Vector2 p = points[i];

            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    private static bool ContainsInclusive(Rect rect, float x, float y)
    {
        return x >= rect.xMin &&
               x <= rect.xMax &&
               y >= rect.yMin &&
               y <= rect.yMax;
    }
}