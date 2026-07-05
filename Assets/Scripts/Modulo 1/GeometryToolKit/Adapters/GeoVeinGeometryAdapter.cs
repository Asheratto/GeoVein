using System.Collections.Generic;
using UnityEngine;

public static class GeoVeinGeometryAdapter
{
    public static List<GeometryRenderItem> ToRenderItems(List<MeshData> meshes)
    {
        List<GeometryRenderItem> items = new List<GeometryRenderItem>();

        if (meshes == null)
            return items;

        for (int i = 0; i < meshes.Count; i++)
        {
            MeshData meshData = meshes[i];

            if (meshData == null || meshData.Source == null)
                continue;

            GeometryRenderItem item = new GeometryRenderItem
            {
                MeshData = meshData,

                // Nuevo: solo para que la ficha sepa qué objeto original es
                Source = meshData.Source,

                // Esto ya existía y debe mantenerse
                DisplayName = meshData.Source.Name,
                Center = meshData.Source.Center,
                DisplayColor = meshData.Source.DisplayColor(),

                ProfileId = ResolveProfileId(meshData.Source),
                LayerDisplayName = ResolveLayerDisplayName(meshData.Source)
            };

            items.Add(item);
        }

        return items;
    }

    public static string ResolveProfileId(object source)
    {
        if (source is IAcuifero)
            return "aquifer";

        if (source is ICuenca)
            return "basin";

        if (source is ILagos)
            return "lake";

        if (source is IDem || source is IRasterElement)
            return "terrain";

        return "default";
    }

    private static string ResolveLayerDisplayName(object source)
    {
        if (source is IAcuifero)
            return "Acuífero";

        if (source is ICuenca)
            return "Cuenca";

        if (source is ILagos)
            return "Lago";

        if (source is IDem || source is IRasterElement)
            return "Terreno";

        return "Elemento";
    }
}