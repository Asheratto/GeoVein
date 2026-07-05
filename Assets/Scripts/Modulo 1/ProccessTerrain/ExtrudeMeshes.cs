using System.Collections.Generic;
using UnityEngine;

public static class ExtrudeMeshes 
{
    private struct Edge
    {
        public int a;
        public int b;

        public Edge(int a, int b)
        {
            this.a = a;
            this.b = b;
        }
    }

    private struct EdgeInfo
    {
        public int count;
        public Edge directedEdge;
    }

    private static List<Edge> GetBoundaryEdges(int[] triangles)
    {
        Dictionary<ulong, EdgeInfo> edges = new Dictionary<ulong, EdgeInfo>();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int a = triangles[i];
            int b = triangles[i + 1];
            int c = triangles[i + 2];

            AddEdge(edges, a, b);
            AddEdge(edges, b, c);
            AddEdge(edges, c, a);
        }

        List<Edge> boundary = new List<Edge>();

        foreach (KeyValuePair<ulong, EdgeInfo> pair in edges)
        {
            if (pair.Value.count == 1)
            {
                boundary.Add(pair.Value.directedEdge);
            }
        }

        return boundary;
    }

    private static void AddEdge(Dictionary<ulong, EdgeInfo> edges, int a, int b)
    {
        ulong key = GetUndirectedEdgeKey(a, b);

        if (edges.TryGetValue(key, out EdgeInfo info))
        {
            info.count++;
            edges[key] = info;
        }
        else
        {
            info = new EdgeInfo();
            info.count = 1;
            info.directedEdge = new Edge(a, b);

            edges.Add(key, info);
        }
    }

    private static ulong GetUndirectedEdgeKey(int a, int b)
    {
        uint min = (uint)Mathf.Min(a, b);
        uint max = (uint)Mathf.Max(a, b);

        return ((ulong)min << 32) | max;
    }
    public static MeshData ExtrudeDem(MeshData data, int width, int height, float baseDepth)
    {
        Vector3[] top = data.vertices;
        int topCount = top.Length;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.AddRange(top);

        float minY = GetMinY(top);
        float bottomY = minY - baseDepth;

        // =====================================================
        // BOTTOM VERTICES
        // =====================================================

        for (int i = 0; i < topCount; i++)
        {
            Vector3 v = top[i];
            vertices.Add(new Vector3(v.x, bottomY, v.z));
        }

        int bottomOffset = topCount;

        // =====================================================
        // TOP
        // =====================================================

        triangles.AddRange(data.triangles);

        // =====================================================
        // BOTTOM
        // =====================================================

        for (int i = 0; i < data.triangles.Length; i += 3)
        {
            triangles.Add(data.triangles[i + 2] + bottomOffset);
            triangles.Add(data.triangles[i + 1] + bottomOffset);
            triangles.Add(data.triangles[i] + bottomOffset);
        }

        // =====================================================
        // SIDES DESDE BORDE REAL DE LA MÁSCARA
        // =====================================================

        List<Edge> boundaryEdges = GetBoundaryEdges(data.triangles);

        for (int i = 0; i < boundaryEdges.Count; i++)
        {
            int a = boundaryEdges[i].a;
            int b = boundaryEdges[i].b;

            int aBottom = a + bottomOffset;
            int bBottom = b + bottomOffset;

            // Cara lateral
            triangles.Add(a);
            triangles.Add(aBottom);
            triangles.Add(b);

            triangles.Add(b);
            triangles.Add(aBottom);
            triangles.Add(bBottom);

            // Opcional: cara inversa para evitar problemas de culling
            triangles.Add(b);
            triangles.Add(aBottom);
            triangles.Add(a);

            triangles.Add(bBottom);
            triangles.Add(aBottom);
            triangles.Add(b);
        }

        // =====================================================
        // UVs
        // =====================================================

        Vector2[] uvs = null;

        if (data.uvs != null && data.uvs.Length == topCount)
        {
            List<Vector2> uvList = new List<Vector2>();
            uvList.AddRange(data.uvs);
            uvList.AddRange(data.uvs);
            uvs = uvList.ToArray();
        }

        // =====================================================
        // NUEVO MESHDATA
        // =====================================================

        MeshData extruded = new MeshData();

        extruded.vertices = vertices.ToArray();
        extruded.triangles = triangles.ToArray();
        extruded.uvs = uvs;

        extruded.width = data.width;
        extruded.height = data.height;
        extruded.Source = data.Source;
        extruded.centro = data.centro;
        extruded.contour = data.contour;

        return extruded;
    }

    private static float GetMinY(Vector3[] vertices)
    {
        float min = float.MaxValue;

        foreach (Vector3 v in vertices)
        {
            if (v.y < min)
                min = v.y;
        }
        return min;
    }


    public static MeshData Extrude(MeshData data, float height)
    {
        var contour = data.contour;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // =====================================================
        // TOP
        // =====================================================


        for (int i = 0; i < data.vertices.Length; i++)
        {
            vertices.Add(data.vertices[i]);
        }

        triangles.AddRange(data.triangles);

        // =====================================================
        // BOTTOM
        // =====================================================

        int bottomOffset = vertices.Count;

        for (int i = 0; i < data.vertices.Length; i++)
        {
            vertices.Add(data.vertices[i] + Vector3.down * height);
        }

        // Bottom triangles invertidos
        for (int i = 0; i < data.triangles.Length; i += 3)
        {
            triangles.Add(bottomOffset + data.triangles[i]);
            triangles.Add(bottomOffset + data.triangles[i + 2]);
            triangles.Add(bottomOffset + data.triangles[i + 1]);
        }

        // =====================================================
        // SIDES
        // =====================================================

        int sideOffset = vertices.Count;

        // Top side vertices
        for (int i = 0; i < contour.Count; i++)
        {
            Vector2 p = contour[i];
            vertices.Add(new Vector3(p.x, 0f, p.y));
        }

        // Bottom side vertices
        for (int i = 0; i < contour.Count; i++)
        {
            Vector2 p = contour[i];
            vertices.Add(new Vector3(p.x, -height, p.y));
        }

        int n = contour.Count;

        for (int i = 0; i < n; i++)
        {
            int next = (i + 1) % n;

            int topA = sideOffset + i;
            int topB = sideOffset + next;

            int botA = sideOffset + n + i;
            int botB = sideOffset + n + next;

            triangles.Add(topA);
            triangles.Add(botA);
            triangles.Add(topB);

            triangles.Add(topB);
            triangles.Add(botA);
            triangles.Add(botB);
        }

        // =====================================================
        // UVs
        // =====================================================

        Vector2[] uvs = null;

        if (data.uvs != null && data.uvs.Length == data.vertices.Length)
        {
            List<Vector2> uvList = new List<Vector2>();

            // UVs top
            uvList.AddRange(data.uvs);

            // UVs bottom
            uvList.AddRange(data.uvs);

            // UVs lados
            for (int i = 0; i < contour.Count * 2; i++)
            {
                uvList.Add(Vector2.zero);
            }

            uvs = uvList.ToArray();
        }

        // =====================================================
        // NUEVO MESHDATA
        // =====================================================

        MeshData extruded = new MeshData();

        extruded.vertices = vertices.ToArray();
        extruded.triangles = triangles.ToArray();
        extruded.uvs = uvs;
        extruded.contour = data.contour;
        extruded.centro = data.centro;

        // Esto mantiene la referencia al elemento original
        extruded.Source = data.Source;

        return extruded;
    }

}
