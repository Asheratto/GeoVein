using System.Text;
using TMPro;
using UnityEngine;

public class FichaElementoUI : MonoBehaviour
{
    public static FichaElementoUI instance;

    [Header("Textos principales")]
    [SerializeField] private TMP_Text tituloText;
    [SerializeField] private TMP_Text tipoText;
    [SerializeField] private TMP_Text capaText;
    [SerializeField] private TMP_Text contenidoText;

    private void Awake()
    {
        instance = this;
    }

    public void Mostrar(object elemento)
    {
        if (elemento == null)
        {
            Limpiar();
            return;
        }

        gameObject.SetActive(true);

        switch (elemento)
        {
            case Acuifero acuifero:
                MostrarAcuifero(acuifero);
                break;

            case Cuenca cuenca:
                MostrarCuenca(cuenca);
                break;

            case Lagos lago:
                MostrarLago(lago);
                break;

            case Dem dem:
                MostrarDem(dem);
                break;

            default:
                MostrarGenerico(elemento);
                break;
        }
    }

    private void MostrarAcuifero(Acuifero a)
    {
        SetHeader(
            $"Acuífero {Safe(a.name)}",
            "Acuífero",
            "Acuíferos"
        );

        StringBuilder sb = new StringBuilder();

        AddSection(sb, "Identificación");
        AddRow(sb, "ID", a.id.ToString());
        AddRow(sb, "SHAC", a.shac);
        AddRow(sb, "Región", a.region);

        AddSection(sb, "Geometría");
        AddRow(sb, "Centro", FormatCenter(a.Center));
        AddRow(sb, "Vértices", a.coordinates != null ? a.coordinates.Count.ToString() : "—");

        contenidoText.text = sb.ToString();
    }

    private void MostrarCuenca(Cuenca c)
    {
        SetHeader(
            $"Cuenca {Safe(c.name)}",
            "Cuenca",
            "Cuencas"
        );

        StringBuilder sb = new StringBuilder();

        AddSection(sb, "Información general");
        AddRow(sb, "ID", c.id.ToString());
        AddRow(sb, "Área", c.area);
        AddRow(sb, "Vertiente", c.vertiente);

        AddSection(sb, "Datos climáticos");
        AddRow(sb, "Temp. media", c.t_med);
        AddRow(sb, "Temp. mínima", c.t_min);
        AddRow(sb, "Temp. máxima", c.t_max);
        AddRow(sb, "Precipitación", c.pp);

        AddSection(sb, "Estaciones");
        AddRow(sb, "Pluviométricas", c.n_estac_p);
        AddRow(sb, "Fluviométricas", c.n_estac_f);

        AddSection(sb, "Geometría");
        AddRow(sb, "Centro", FormatCenter(c.Center));
        AddRow(sb, "Vértices", c.coordinates != null ? c.coordinates.Count.ToString() : "—");

        contenidoText.text = sb.ToString();
    }

    private void MostrarLago(Lagos l)
    {
        SetHeader(
            $"Lago {Safe(l.name)}",
            "Lago",
            "Lagos"
        );

        StringBuilder sb = new StringBuilder();

        AddSection(sb, "Información general");
        AddRow(sb, "ID", l.id.ToString());
        AddRow(sb, "Tipo", l.tipo);
        AddRow(sb, "Área", l.area);

        AddSection(sb, "Ubicación");
        AddRow(sb, "Provincia", l.provincia);
        AddRow(sb, "Comuna", l.comuna);

        AddSection(sb, "Geometría");
        AddRow(sb, "Centro", FormatCenter(l.Center));
        AddRow(sb, "Vértices", l.coordinates != null ? l.coordinates.Count.ToString() : "—");

        contenidoText.text = sb.ToString();
    }

    private void MostrarDem(Dem d)
    {
        SetHeader(
            Safe(d.Name, "Modelo de elevación"),
            "DEM",
            "Terreno"
        );

        StringBuilder sb = new StringBuilder();

        AddSection(sb, "Información general");
        AddRow(sb, "Nombre", d.Name);
        AddRow(sb, "Resolución", d.texture != null ? $"{d.texture.width} x {d.texture.height}" : "—");

        AddSection(sb, "Geometría");
        AddRow(sb, "Centro", FormatCenter(d.Center));

        contenidoText.text = sb.ToString();
    }

    private void MostrarGenerico(object elemento)
    {
        SetHeader("Elemento seleccionado", "Desconocido", "—");
        contenidoText.text = "No hay información disponible para este elemento.";
    }

    private void SetHeader(string titulo, string tipo, string capa)
    {
        tituloText.text = titulo;
        tipoText.text = $"Tipo: {tipo}";
        capaText.text = $"Capa: {capa}";
    }

    private void AddSection(StringBuilder sb, string title)
    {
        sb.AppendLine();
        sb.AppendLine($"<b>{title}</b>");
    }

    private void AddRow(StringBuilder sb, string label, string value)
    {
        sb.AppendLine($"<b>{label}:</b> {Safe(value)}");
    }

    private string Safe(string value, string fallback = "—")
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private string FormatCenter(Vector2 center)
    {
        return $"{center.x:0.0000}, {center.y:0.0000}";
    }

    public void Limpiar()
    {
        tituloText.text = "Ficha";
        tipoText.text = "Tipo: —";
        capaText.text = "Capa: —";
        contenidoText.text = "Selecciona un elemento para ver su información.";
    }
}