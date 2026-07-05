using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WorldGrid : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int halfLines = 50;
    [SerializeField] private float spacing = 10f;
    [SerializeField] private float yPosition = 0f;

    [Header("Infinite feeling")]
    [SerializeField] private bool followCamera = true;
    [SerializeField] private Transform cameraTarget;

    private Mesh mesh;

    private void OnEnable()
    {
        BuildGrid();
    }

    private void OnValidate()
    {
        halfLines = Mathf.Max(1, halfLines);
        spacing = Mathf.Max(0.01f, spacing);

        BuildGrid();
    }

    private void Update()
    {
        if (!followCamera || cameraTarget == null)
        {
            transform.position = new Vector3(0f, yPosition, 0f);
            return;
        }

        Vector3 camPos = cameraTarget.position;

        float snappedX = Mathf.Round(camPos.x / spacing) * spacing;
        float snappedZ = Mathf.Round(camPos.z / spacing) * spacing;

        transform.position = new Vector3(snappedX, yPosition, snappedZ);
    }

    private void BuildGrid()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "World Grid Mesh";
            mesh.indexFormat = IndexFormat.UInt32;
        }

        int linesPerAxis = halfLines * 2 + 1;
        int totalLines = linesPerAxis * 2;

        Vector3[] vertices = new Vector3[totalLines * 2];
        int[] indices = new int[vertices.Length];

        float size = halfLines * spacing;

        int v = 0;

        for (int i = -halfLines; i <= halfLines; i++)
        {
            float coord = i * spacing;

            // Línea paralela al eje X
            vertices[v] = new Vector3(-size, 0f, coord);
            indices[v] = v;
            v++;

            vertices[v] = new Vector3(size, 0f, coord);
            indices[v] = v;
            v++;

            // Línea paralela al eje Z
            vertices[v] = new Vector3(coord, 0f, -size);
            indices[v] = v;
            v++;

            vertices[v] = new Vector3(coord, 0f, size);
            indices[v] = v;
            v++;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
