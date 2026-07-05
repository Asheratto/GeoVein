using System.Text;
using UnityEngine;

public static class GeometryCardBuilder
{
    public static Card Build(GeometryRenderItem item, GeometryLayerProfile profile)
    {
        if (item == null)
        {
            return new Card(
                "Elemento",
                "Sin capa",
                "—",
                null
            );
        }

        string name = Safe(item.DisplayName, "Elemento seleccionado");
        string layer = profile != null
            ? Safe(profile.DisplayName, item.LayerDisplayName)
            : Safe(item.LayerDisplayName, "Elemento");

        string center = FormatCenter(item.Center);

        return new Card(
            name,
            layer,
            center,
            item.Source
        );
    }

    private static string BuildBody(object source, GeometryRenderItem item)
    {
        if (source is Acuifero acuifero)
            return BuildAcuiferoBody(acuifero);

        if (source is Cuenca cuenca)
            return BuildCuencaBody(cuenca);

        if (source is Lagos lago)
            return BuildLagoBody(lago);

        if (source is Dem dem)
            return BuildDemBody(dem);

        return BuildGenericBody(item);
    }

    private static string BuildAcuiferoBody(Acuifero a)
    {
        StringBuilder sb = new StringBuilder();

        AddSection(sb, "Identificación");
        AddRow(sb, "ID", a.id);
        AddRow(sb, "Nombre", a.name);
        AddRow(sb, "SHAC", a.shac);
        AddRow(sb, "Región", a.region);

        AddSection(sb, "Geometría");
        AddRow(sb, "Centro", FormatCenter(a.Center));
        AddRow(sb, "Vértices", a.coordinates != null ? a.coordinates.Count.ToString() : "—");

        return sb.ToString();
    }

    private static string BuildCuencaBody(Cuenca c)
    {
        StringBuilder sb = new StringBuilder();

        AddSection(sb, "Información general");
        AddRow(sb, "ID", c.id);
        AddRow(sb, "Nombre", c.name);
        AddRow(sb, "Área", c.area);
        AddRow(sb, "Vertiente", c.vertiente);

        AddSection(sb, "Datos climáticos");
        AddRow(sb, "Temp. media", c.t_med);
        AddRow(sb, "Temp. mínima", c.t_min);
        AddRow(sb, "Temp. máxima", c.t_max);
        AddRow(sb, "Precipitación", c.pp);

        AddSection(sb, "Estaciones");
        AddRow(sb, "Est. pluviométricas", c.n_estac_p);
        AddRow(sb, "Est. fluviométricas", c.n_estac_f);

        AddSection(sb, "Geometría");
        AddRow(sb, "Centro", FormatCenter(c.Center));
        AddRow(sb, "Vértices", c.coordinates != null ? c.coordinates.Count.ToString() : "—");

        return sb.ToString();
    }

    private static string BuildLagoBody(Lagos l)
    {
        StringBuilder sb = new StringBuilder();

        AddSection(sb, "Información general");
        AddRow(sb, "ID", l.id);
        AddRow(sb, "Nombre", l.name);
        AddRow(sb, "Tipo", l.tipo);
        AddRow(sb, "Área", l.area);

        AddSection(sb, "Ubicación");
        AddRow(sb, "Provincia", l.provincia);
        AddRow(sb, "Comuna", l.comuna);

        AddSection(sb, "Geometría");
        AddRow(sb, "Centro", FormatCenter(l.Center));
        AddRow(sb, "Vértices", l.coordinates != null ? l.coordinates.Count.ToString() : "—");

        return sb.ToString();
    }

    private static string BuildDemBody(Dem d)
    {
        StringBuilder sb = new StringBuilder();

        AddSection(sb, "Modelo de elevación");
        AddRow(sb, "Nombre", d.Name);

        if (d.texture != null)
            AddRow(sb, "Resolución", d.texture.width + " x " + d.texture.height);
        else
            AddRow(sb, "Resolución", "—");

        AddSection(sb, "Geometría");
        AddRow(sb, "Centro", FormatCenter(d.Center));

        return sb.ToString();
    }

    private static string BuildGenericBody(GeometryRenderItem item)
    {
        StringBuilder sb = new StringBuilder();

        AddSection(sb, "Información general");
        AddRow(sb, "Nombre", item.DisplayName);
        AddRow(sb, "Tipo", item.LayerDisplayName);

        AddSection(sb, "Geometría");
        AddRow(sb, "Centro", FormatCenter(item.Center));

        return sb.ToString();
    }

    private static void AddSection(StringBuilder sb, string title)
    {
        sb.AppendLine();
        sb.AppendLine("<b>" + title + "</b>");
    }

    private static void AddRow(StringBuilder sb, string label, object value)
    {
        sb.AppendLine("<b>" + label + ":</b> " + SafeValue(value));
    }

    private static string SafeValue(object value)
    {
        if (value == null)
            return "—";

        string text = value.ToString();

        if (string.IsNullOrWhiteSpace(text))
            return "—";

        return text;
    }

    private static string Safe(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        return value;
    }

    private static string FormatCenter(Vector2 center)
    {
        return center.x.ToString("0.0000") + ", " + center.y.ToString("0.0000");
    }
}