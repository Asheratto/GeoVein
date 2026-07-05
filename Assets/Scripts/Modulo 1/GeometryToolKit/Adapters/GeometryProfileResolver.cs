using System.Collections.Generic;
using UnityEngine;

public class GeometryProfileResolver : MonoBehaviour
{
    [SerializeField] private List<GeometryLayerProfile> profiles = new List<GeometryLayerProfile>();

    public GeometryLayerProfile GetProfile(string profileId)
    {
        if (string.IsNullOrEmpty(profileId))
        {
            Debug.LogWarning("ProfileId vacío o null.");
            return null;
        }

        for (int i = 0; i < profiles.Count; i++)
        {
            GeometryLayerProfile profile = profiles[i];

            if (profile == null)
                continue;

            if (profile.ProfileId == profileId)
                return profile;
        }

        Debug.LogWarning($"No existe perfil para ProfileId: {profileId}");
        return null;
    }

    public GeometryLayerProfile GetProfile(GeometryRenderItem item)
    {
        if (item == null)
            return null;

        return GetProfile(item.ProfileId);
    }

    public Material CreateMaterial(GeometryRenderItem item, GeometryLayerProfile profile)
    {
        if (item == null || profile == null || profile.Material == null)
            return null;

        Material material = new Material(profile.Material);

        if (profile.UseHeightRange)
            ApplyHeightRange(item.MeshData.vertices, material, profile.HeightOffset);

        if (profile.UseSourceColor)
            material.color = item.DisplayColor;

        if (profile.UseRandomColor)
            ApplyRandomColor(material);

        return material;
    }

    private void ApplyHeightRange(Vector3[] vertices, Material material, float heightOffset)
    {
        if (vertices == null || vertices.Length == 0 || material == null)
            return;

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            float height = vertices[i].y + heightOffset;

            if (height < minHeight)
                minHeight = height;

            if (height > maxHeight)
                maxHeight = height;
        }

        material.SetFloat("_MinHeight", minHeight);
        material.SetFloat("_MaxHeight", maxHeight);
    }

    private void ApplyRandomColor(Material material)
    {
        Color baseColor = new Color(
            Random.Range(0.2f, 0.8f),
            Random.Range(0.2f, 0.8f),
            Random.Range(0.2f, 0.8f),
            1f
        );

        Color edgeColor = new Color(
            Mathf.Clamp01(baseColor.r + 0.2f),
            Mathf.Clamp01(baseColor.g + 0.2f),
            Mathf.Clamp01(baseColor.b + 0.2f),
            1f
        );

        material.SetColor("_BaseColor", baseColor);
        material.SetColor("_EdgeColor", edgeColor);
    }

    public void ClearProfileParents()
    {
        for (int i = 0; i < profiles.Count; i++)
        {
            GeometryLayerProfile profile = profiles[i];

            if (profile == null || profile.Parent == null)
                continue;

            ClearChildren(profile.Parent);
        }
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}