using UnityEngine;

public static class HeightMapConverter
{
    public static float[,] TextureToFloatArray(
        Texture2D texture,
        float heightScale = 1f,
        float heightOffset = 0f,
        bool invertY = false
    )
    {
        if (texture == null)
        {
            Debug.LogError("TextureToFloatArray: texture es null.");
            return null;
        }

        int width = texture.width;
        int height = texture.height;

        float[,] result = new float[width, height];

        Color32[] pixels = texture.GetPixels32();

        for (int z = 0; z < height; z++)
        {
            int texZ = invertY ? height - 1 - z : z;

            for (int x = 0; x < width; x++)
            {
                int index = texZ * width + x;

                Color32 pixel = pixels[index];

                float gray = pixel.r / 255f;

                result[x, z] = gray * heightScale + heightOffset;
            }
        }

        return result;
    }

    public static int[,] TextureToDemMaskFromAlpha(
    Texture2D texture,
    byte alphaThreshold = 10,
    bool invertY = false
)
    {
        if (texture == null)
        {
            Debug.LogError("TextureToDemMaskFromAlpha: texture es null.");
            return null;
        }

        int width = texture.width;
        int height = texture.height;

        int[,] mask = new int[width, height];

        Color32[] pixels = texture.GetPixels32();

        for (int z = 0; z < height; z++)
        {
            int texZ = invertY ? height - 1 - z : z;

            for (int x = 0; x < width; x++)
            {
                int index = texZ * width + x;

                Color32 pixel = pixels[index];

                mask[x, z] = pixel.a > alphaThreshold ? 1 : 0;
            }
        }

        return mask;
    }
}