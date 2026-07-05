using UnityEngine;
using UnityEngine.Rendering;

public static class UnityMeshFactory
{
    public static Mesh Create(MeshData meshData)
    {
        if (meshData == null)
        {
            Debug.LogError("No se puede crear Mesh: MeshData es null.");
            return null;
        }

        if (meshData.vertices == null || meshData.triangles == null)
        {
            Debug.LogError($"No se puede crear Mesh: datos inválidos en {meshData.Source?.Name}.");
            return null;
        }

        Mesh mesh = new Mesh();

        if (meshData.vertices.Length > 65535)
            mesh.indexFormat = IndexFormat.UInt32;

        mesh.SetVertices(meshData.vertices);
        mesh.SetTriangles(meshData.triangles, 0);

        if (meshData.uvs != null && meshData.uvs.Length == meshData.vertices.Length)
            mesh.SetUVs(0, meshData.uvs);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}