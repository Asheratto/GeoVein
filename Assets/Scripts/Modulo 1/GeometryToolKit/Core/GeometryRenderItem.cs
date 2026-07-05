using UnityEngine;

public class GeometryRenderItem
{
    public MeshData MeshData { get; set; }

    public object Source { get; set; }

    public string DisplayName { get; set; }
    public Vector2 Center { get; set; }
    public Color DisplayColor { get; set; }

    public string ProfileId { get; set; }
    public string LayerDisplayName { get; set; }
}