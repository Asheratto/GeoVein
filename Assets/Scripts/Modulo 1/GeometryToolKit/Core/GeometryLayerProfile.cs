using UnityEngine;

[System.Serializable]
public class GeometryLayerProfile
{
    public string ProfileId;
    public string DisplayName;

    public Transform Parent;
    public Material Material;

    public float HeightOffset;

    public bool UseSourceColor;
    public bool UseRandomColor;
    public bool UseHeightRange;

    public bool CreateLabel = false;
    public bool CreateMapTarget = true;

    [Header("Posicionamiento")]
    public bool UseMeshCenterAsPosition = true;
    public Vector3 PositionOffset = Vector3.zero;

    [Header("Geometria")]
    public bool ProcessGeometry = true;
    public bool UseSimplification = true;
    public float SimplificationThreshold = 0.000001f;
    public bool UseExtrusion = true;
    public float ExtrusionHeight = 10f;
}
