using System;
using UnityEngine;

public static class LatAndLongCalculator
{
    // Bounding real del DEM (WGS84)
    private static readonly double norte = -34.840421393;
    private static readonly double sur = -36.188062753;
    private static readonly double este = -70.229362986;
    private static readonly double oeste = -72.625169849;

    private static readonly double latRange = norte - sur;
    private static readonly double lonRange = este - oeste;

    // =========================
    // PIXEL → LAT / LON
    // =========================

    public static double PixelToLat(float yPixel, float screenHeight)
    {
        return sur + (yPixel / screenHeight) * latRange;
    }

    public static double PixelToLon(float xPixel, float screenWidth)
    {
        return oeste + (xPixel / screenWidth) * lonRange;
    }

    //GDAL
    public static (double minLat, double maxLat, double minLon, double maxLon)
        GetBoundingBox(Vector2 startPixel, Vector2 endPixel, float screenWidth, float screenHeight)
    {
        double lat1 = PixelToLat(startPixel.y, screenHeight);
        double lat2 = PixelToLat(endPixel.y, screenHeight);

        double lon1 = PixelToLon(startPixel.x, screenWidth);
        double lon2 = PixelToLon(endPixel.x, screenWidth);

        double minLat = Math.Min(lat1, lat2);
        double maxLat = Math.Max(lat1, lat2);

        double minLon = Math.Min(lon1, lon2);
        double maxLon = Math.Max(lon1, lon2);

        minLat = Math.Clamp(minLat, sur, norte);
        maxLat = Math.Clamp(maxLat, sur, norte);
        minLon = Math.Clamp(minLon, oeste, este);
        maxLon = Math.Clamp(maxLon, oeste, este);

        return (minLat, maxLat, minLon, maxLon);
    }
}