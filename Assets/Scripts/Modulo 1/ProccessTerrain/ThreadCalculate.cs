using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Mesh;

public class MeshData
{
    public IElementMap Source;
    public Vector2 centro;
    public Vector3[] vertices;
    public int[] triangles;
    public List<Vector2> contour;
    public Vector2[] uvs;
    public int width;
    public int height;
}

public static class GenerateDataMesh
{
    #region Objetos
    public static MeshData Triangulate(List<Vector2> points)
    {
        if (points == null || points.Count < 3)
            return null;

        var tess = new LibTessDotNet.Tess();

        var contour = new LibTessDotNet.ContourVertex[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            contour[i].Position = new LibTessDotNet.Vec3(points[i].x, 0, points[i].y);
        }

        //tess.AddContour(contour, LibTessDotNet.ContourOrientation.Original);
        tess.AddContour(contour, LibTessDotNet.ContourOrientation.CounterClockwise);
        // ✔️ Polygons + 3 = triángulos en tu versión
        tess.Tessellate(
            LibTessDotNet.WindingRule.EvenOdd,
            LibTessDotNet.ElementType.Polygons,
            3
        );

        if (tess.Elements == null || tess.Elements.Length == 0)
            return null;

        // 🔹 Convertir vértices
        int vCount = tess.Vertices.Length;
        Vector3[] vertices = new Vector3[vCount];

        for (int i = 0; i < vCount; i++)
        {
            var v = tess.Vertices[i].Position;
            vertices[i] = new Vector3(v.X, 0, v.Z);
        }

        // 🔥 FILTRO CLAVE (esto te arregla el problema visual)
        List<int> triangles = new List<int>(tess.Elements.Length);

        for (int i = 0; i < tess.Elements.Length; i += 3)
        {
            int a = tess.Elements[i];
            int b = tess.Elements[i + 1];
            int c = tess.Elements[i + 2];

            // ❌ descartar triángulos inválidos
            if (a < 0 || b < 0 || c < 0)
                continue;

            if (a >= vCount || b >= vCount || c >= vCount)
                continue;

            // opcional: evitar degenerados (muy útil)
            if (a == b || b == c || a == c)
                continue;

            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        // seguridad extra
        if (triangles.Count == 0)
            return null;

        return new MeshData
        {
            vertices = vertices,
            triangles = triangles.ToArray(),
            contour = points
        };
    }
    #endregion

    #region DEM

    public static List<Vector2> DemUVs(int width, int height)
    {
        List<Vector2> result = new List<Vector2>();

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                result.Add(new Vector2((float)x / width, (float)z / height));
            }
        }

        return result;
    }

    public static List<int> DemTriangles(int width, int height)
    {
        List<int> triangles = new List<int>();

        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int i = z * width + x;

                triangles.Add(i);
                triangles.Add(i + width);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + width);
                triangles.Add(i + width + 1);
            }
        }
        return triangles;
    }


    


    //Genera el Mesh
    public static MeshData MeshDemTriangulate(List<Vector3> vertices, int width, int height)
    {
        return new MeshData
        {
            vertices = vertices.ToArray(),
            triangles = DemTriangles(width, height).ToArray(),
            //contour = DemContour(vertices, width, height),
            uvs = DemUVs(width, height).ToArray(),
            width= width,
            height= height

        };
       
    }
    #endregion

}
