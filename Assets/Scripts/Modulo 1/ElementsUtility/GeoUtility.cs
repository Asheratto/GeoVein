using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeoVector
{
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }

    public GeoVector(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class GeoNormalizer
{
    private float minX, maxX;
    private float minZ, maxZ;

    private float sizeX, sizeZ;

    private float centerX, centerZ;

    private float worldScale;
    private float heightScale;

    private Texture2D heightMap;
    private int width;
    private int height;
    private float minHeight = float.MaxValue;
    private float maxHeight = float.MinValue;



    public GeoNormalizer(List<IGeometricElement> elementos, IRasterElement raster, float worldScale = 100f, float heightScale = 1f)
    {
        this.worldScale = worldScale;
        this.heightScale = heightScale;
        this.heightMap = raster.GetHeightMap();
        this.width = heightMap.width;
        this.height = heightMap.height;

        //Lectura de geo
        List<Vector2> todos = new List<Vector2>();

        foreach (var e in elementos)
        {
            var verts = e.GetVertices();

            if (verts != null && verts.Count > 0)
                todos.AddRange(verts);
        }

        if (todos.Count == 0)
            return;

        minX = todos.Min(v => v.x);
        maxX = todos.Max(v => v.x);

        minZ = todos.Min(v => v.y);
        maxZ = todos.Max(v => v.y);

        sizeX = maxX - minX;
        sizeZ = maxZ - minZ;

        // evitar división por cero
        if (sizeX == 0) sizeX = 1;
        if (sizeZ == 0) sizeZ = 1;

        // centro del acuífero
        centerX = (minX + maxX) * 0.5f;
        centerZ = (minZ + maxZ) * 0.5f;

        // 1. LEER ALTURAS Maximo y minimo
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = z * width + x;

                float h = heightMap.GetPixel(x, z).grayscale;

                if (h < minHeight) minHeight = h;
                if (h > maxHeight) maxHeight = h;
            }
        }
    }

    // 🔹 Normaliza y centra
    public Vector2 GeoToWorld2D(Vector2 v)
    {
        float maxSize = Mathf.Max(sizeX, sizeZ);

        float nx = (v.x - centerX) / maxSize;
        float nz = (v.y - centerZ) / maxSize;

        return new Vector2(
            nx * worldScale,
            nz * worldScale
        );
    }

    public List<Vector2> NormalizeLocalToVector3List(List<Vector2> vertices)
    {
        List<Vector2> result = new List<Vector2>();


        float elementMinX = vertices.Min(v => v.x);
        float elementMaxX = vertices.Max(v => v.x);

        float elementMinZ = vertices.Min(v => v.y);
        float elementMaxZ = vertices.Max(v => v.y);

        float elementCenterX= (elementMinX + elementMaxX) * 0.5f;
        float elementCenterZ = (elementMinZ + elementMaxZ) * 0.5f;
        float maxSize = Mathf.Max(sizeX, sizeZ);

        foreach (Vector2 v in vertices)
        {
            float nx = (v.x - elementCenterX) / maxSize;
            float nz = (v.y - elementCenterZ) / maxSize;

            result.Add(new Vector2(
                nx * worldScale,
                nz * worldScale
            ));
        }

        return result;
    }

    public Vector2 Center(List<Vector2> vertices)
    {


        float elementMinX = vertices.Min(v => v.x);
        float elementMaxX = vertices.Max(v => v.x);

        float elementMinZ = vertices.Min(v => v.y);
        float elementMaxZ = vertices.Max(v => v.y);

        Vector2 center = GeoToWorld2D(new Vector2((elementMinX + elementMaxX) * 0.5f, (elementMinZ + elementMaxZ) * 0.5f));


        return center;
    }

    //Normaliza y centra el punto
    public Vector3 NormalizeHeightmapPoint(int x, float y,int z)
    {
        float nx = (float)x / (width - 1);
        float nz = (float)z / (height - 1);

        float geoX = Mathf.Lerp(minX, maxX, nx);
        float geoZ = Mathf.Lerp(minZ, maxZ, nz);

        Vector2 normalized = GeoToWorld2D(new Vector2(geoX, geoZ));

        float h01 = Mathf.InverseLerp(minHeight, maxHeight, y);

        float finalHeight = h01 * heightScale;

        return new Vector3(
            normalized.x,
            finalHeight,
            normalized.y
        );
    }

    //Para normalizar q debo recibir Retorna los vertices normalizados
    public List<Vector3> NormalizeVertices()
    {
        List<Vector3> result = new List<Vector3>();

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = z * width + x;
                float h = heightMap.GetPixel(x, z).grayscale;
                result.Add(NormalizeHeightmapPoint(x, h, z));
            }
        }

        return result;
    }


}
