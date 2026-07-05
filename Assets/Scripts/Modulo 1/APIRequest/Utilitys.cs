using System;
using System.Collections.Generic;
using UnityEngine;



public class Dem : IDem
{
    public Texture2D texture;

    public string Name { get; set; }
    public Vector2 Center { get; private set; }

    public Dem(byte[] jsonresponse, Vector2 center)
    {
        Center = center;

        texture = new Texture2D(2, 2);

        bool loaded = texture.LoadImage(jsonresponse);

        if (!loaded)
        {
            Debug.LogError("No se pudo cargar la imagen");
        }
    }

    public Color DisplayColor()
    {
        return Color.darkGreen;
    }

    public Texture2D GetHeightMap()
    {
        return texture;
    }
}
public class Acuifero : IAcuifero
{
    public string name;
    public int id;
    public string shac;
    public string region;
    public List<GeoVector> coordinates;

    public Vector2 Center { get; private set; }

    public Acuifero(AcuiferoRaw raw)
    {
        id = raw.id;
        name = raw.name;
        shac = raw.shac;
        region = raw.region;

        coordinates = JsonHelper.ParseCoordinates(raw.geom);

        Center = GeoLocationHelper.CalculateCenterText(coordinates);
    }

    public string Name => "Acuifero " + name;

    public Color DisplayColor()
    {
        return Color.aliceBlue;
    }

    public List<Vector2> GetVertices()
    {
        List<Vector2> verts = new List<Vector2>();

        foreach (var p in coordinates)
        {
            verts.Add(new Vector2((float)p.x, (float)p.y));
        }

        return verts;
    }
}

[System.Serializable]
public class AcuiferosResponse
{
    public int version;
    public List<AcuiferoRaw> acuiferos;
}


[Serializable]
public class AcuiferoRaw
{
    public int id;
    public string name;
    public string shac;
    public string region;
    public string geom; // ← GeoJSON como
                        // 
}
public class CuencaRaw
{
    public int id;
    public string nombre { get; set; }
    public string t_med;
    public string t_min;
    public string t_max;
    public string vertiente;
    public string pp;
    public string n_estac_p;
    public string n_estac_f;
    public string area;
    public string geom;
}
public class Cuenca : ICuenca
{
    public int id;
    public string name;
    public string t_med;
    public string t_min;
    public string t_max;
    public string vertiente;
    public string pp;
    public string n_estac_p;
    public string n_estac_f;
    public string area;
    public List<GeoVector> coordinates;

    private Color displayColor;

    public Vector2 Center { get; private set; }

    public Cuenca(CuencaRaw raw)
    {
        id = raw.id;
        name = raw.nombre;
        t_med = raw.t_med;
        t_min = raw.t_min;
        t_max = raw.t_max;
        vertiente = raw.vertiente;
        pp = raw.pp;
        n_estac_p = raw.n_estac_p;
        n_estac_f = raw.n_estac_f;
        area = raw.area;

        coordinates = JsonHelper.ParseCoordinates(raw.geom);

        Center = GeoLocationHelper.CalculateCenterText(coordinates);

        displayColor = RandomCuencaColor();
    }

    public string Name => "Cuenca " + name;

    public Color DisplayColor()
    {
        return displayColor;
    }

    private Color RandomCuencaColor()
    {
        return UnityEngine.Random.ColorHSV(
            0f, 1f,
            0.35f, 0.75f,
            0.65f, 0.95f
        );
    }

    public List<Vector2> GetVertices()
    {
        List<Vector2> verts = new List<Vector2>();

        foreach (var p in coordinates)
        {
            verts.Add(new Vector2((float)p.x, (float)p.y));
        }

        return verts;
    }
}


public class LagosRaw
{
    public int id;
    public string nombre;
    public string area;
    public string geom;
    public string tipo;
    public string provincia;
    public string comuna;

}
public class Lagos : ILagos
{
    public int id;
    public string name;
    public string area;
    public List<GeoVector> coordinates;
    public string tipo;
    public string provincia;
    public string comuna;

    public Vector2 Center { get; private set; }
    private Color displayColor;
    public Lagos(LagosRaw raw)
    {
        id = raw.id;
        name = raw.nombre;
        area = raw.area;
        coordinates = JsonHelper.ParseCoordinates(raw.geom);
        tipo = raw.tipo;
        provincia = raw.provincia;
        comuna = raw.comuna;

        Name = name;
        Center = GeoLocationHelper.CalculateCenterText(coordinates);
        displayColor = RandomCuencaColor();
    }

    public string Name { get; set; }

    public Color DisplayColor()
    {
        return displayColor;
    }

    private Color RandomCuencaColor()
    {
        return UnityEngine.Random.ColorHSV(
            0f, 1f,
            0.35f, 0.75f,
            0.65f, 0.95f
        );
    }

    public List<Vector2> GetVertices()
    {
        List<Vector2> verts = new List<Vector2>();

        foreach (var p in coordinates)
        {
            verts.Add(new Vector2((float)p.x, (float)p.y));
        }

        return verts;
    }
}


