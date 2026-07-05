using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public static class GeoLocationHelper
{
    public static Vector2 CalculateCenterText(List<GeoVector> coordinates)
    {
        if (coordinates == null || coordinates.Count == 0)
            return Vector2.zero;

        double minX = coordinates[0].x;
        double maxX = coordinates[0].x;
        double minY = coordinates[0].y;
        double maxY = coordinates[0].y;

        foreach (var p in coordinates)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;

            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        double centerX = (minX + maxX) / 2.0;
        double centerY = (minY + maxY) / 2.0;

        // Normalmente en GeoJSON:
        // X = longitud
        // Y = latitud
        return new Vector2((float)centerX, (float)centerY);
    }
}


public static class JsonHelper
{
    public static List<GeoVector> ParseCoordinates(string geom)
    {
        if (string.IsNullOrWhiteSpace(geom))
        {
            Debug.LogWarning("geomJson es null o vacío");
            return null;
        }

        geom = geom.Replace("\\\"", "\"").Trim('"');

        var json = Newtonsoft.Json.Linq.JObject.Parse(geom);

        var coords = json["coordinates"];
        if (coords == null)
        {
            Debug.LogWarning("JSON no contiene 'coordinates'");
            return null;
        }

        List<GeoVector> points = new List<GeoVector>();
        foreach (var polygon in coords)
        {
            foreach (var ring in polygon)
            {
                foreach (var point in ring)
                {
                    double lon = point[0].ToObject<double>();
                    double lat = point[1].ToObject<double>();
                    GeoVector gv = new GeoVector(lon, lat, 0);
                    points.Add(gv);
                }
            }
        }

        return points;
    }
}