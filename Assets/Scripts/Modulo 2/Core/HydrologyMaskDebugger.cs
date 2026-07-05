using System.Collections.Generic;
using UnityEngine;

public static class HydrologyMaskDebugger
{
    public static void ValidateInput(HydrologyInputData input)
    {
        if (input == null)
        {
            Debug.LogError("HydrologyInputData es null.");
            return;
        }

        int width = input.Width;
        int height = input.Height;

        Debug.Log($"HydrologyInputData size: {width} x {height}");

        CheckFloatMap("HeightMap", input.HeightMap, width, height);

        CheckIntMap("DemMask", input.DemMask, width, height);
        CheckIntMap("LakeIds", input.LakeIds, width, height);
        CheckIntMap("AquiferIds", input.AquiferIds, width, height);
        CheckIntMap("BasinIds", input.BasinIds, width, height);

        PrintIdStats("DemMask", input.DemMask);
        PrintIdStats("LakeIds", input.LakeIds);
        PrintIdStats("AquiferIds", input.AquiferIds);
        PrintIdStats("BasinIds", input.BasinIds);

        CheckIdsOutsideDemMask("LakeIds", input.LakeIds, input.DemMask);
        CheckIdsOutsideDemMask("AquiferIds", input.AquiferIds, input.DemMask);
        CheckIdsOutsideDemMask("BasinIds", input.BasinIds, input.DemMask);
    }

    private static void CheckFloatMap(string name, float[,] map, int width, int height)
    {
        if (map == null)
        {
            Debug.LogError($"{name} es null.");
            return;
        }

        int w = map.GetLength(0);
        int h = map.GetLength(1);

        if (w != width || h != height)
            Debug.LogError($"{name} tamaño incorrecto: {w}x{h}, esperado {width}x{height}");
        else
            Debug.Log($"{name} OK: {w}x{h}");
    }

    private static void CheckIntMap(string name, int[,] map, int width, int height)
    {
        if (map == null)
        {
            Debug.LogError($"{name} es null.");
            return;
        }

        int w = map.GetLength(0);
        int h = map.GetLength(1);

        if (w != width || h != height)
            Debug.LogError($"{name} tamaño incorrecto: {w}x{h}, esperado {width}x{height}");
        else
            Debug.Log($"{name} OK: {w}x{h}");
    }

    public static void PrintIdStats(string name, int[,] map)
    {
        if (map == null)
        {
            Debug.LogError($"{name} es null.");
            return;
        }

        Dictionary<int, int> counts = new Dictionary<int, int>();

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                int id = map[x, z];

                if (!counts.ContainsKey(id))
                    counts[id] = 0;

                counts[id]++;
            }
        }

        string message = $"{name} IDs: ";

        foreach (var pair in counts)
        {
            message += $"[{pair.Key}: {pair.Value}] ";
        }

        Debug.Log(message);
    }

    private static void CheckIdsOutsideDemMask(string name, int[,] ids, int[,] demMask)
    {
        if (ids == null || demMask == null)
            return;

        int width = ids.GetLength(0);
        int height = ids.GetLength(1);

        int outsideCount = 0;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (ids[x, z] != 0 && demMask[x, z] == 0)
                    outsideCount++;
            }
        }

        if (outsideCount > 0)
            Debug.LogWarning($"{name} tiene {outsideCount} celdas con ID fuera de DemMask.");
        else
            Debug.Log($"{name}: no hay IDs fuera de DemMask.");
    }

    public static Texture2D IdMaskToTexture(int[,] map, bool invertY = false)
    {
        if (map == null)
        {
            Debug.LogError("IdMaskToTexture: map es null.");
            return null;
        }

        int width = map.GetLength(0);
        int height = map.GetLength(1);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int z = 0; z < height; z++)
        {
            int texZ = invertY ? height - 1 - z : z;

            for (int x = 0; x < width; x++)
            {
                int id = map[x, z];

                Color color;

                if (id == 0)
                {
                    color = Color.black;
                }
                else
                {
                    float hue = (id * 0.137f) % 1f;
                    color = Color.HSVToRGB(hue, 0.85f, 1f);
                }

                texture.SetPixel(x, texZ, color);
            }
        }

        texture.Apply();
        return texture;
    }
}
