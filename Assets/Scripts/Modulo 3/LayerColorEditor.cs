using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LayerColorEditor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text selectedNameText;
    [SerializeField] private Image colorPreview;

    [SerializeField] private Slider sliderR;
    [SerializeField] private Slider sliderG;
    [SerializeField] private Slider sliderB;
    [SerializeField] private Slider sliderA;

    private GameObject selectedObject;

    private void Start()
    {
        sliderR.onValueChanged.AddListener(_ => ApplyColor());
        sliderG.onValueChanged.AddListener(_ => ApplyColor());
        sliderB.onValueChanged.AddListener(_ => ApplyColor());
        sliderA.onValueChanged.AddListener(_ => ApplyColor());
    }

    public void SetTarget(GameObject target)
    {
        selectedObject = target;

        if (selectedNameText != null)
            selectedNameText.text = target.name;

        Color currentColor = GetCurrentColor(target);

        sliderR.SetValueWithoutNotify(currentColor.r);
        sliderG.SetValueWithoutNotify(currentColor.g);
        sliderB.SetValueWithoutNotify(currentColor.b);
        sliderA.SetValueWithoutNotify(currentColor.a);

        UpdatePreview(currentColor);
    }

    private void ApplyColor()
    {
        if (selectedObject == null)
            return;

        Color color = new Color(
            sliderR.value,
            sliderG.value,
            sliderB.value,
            sliderA.value
        );

        MeshRenderer[] renderers = selectedObject.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in renderers)
        {
            Material mat = renderer.material;
            ApplyMaterialColor(mat, color);
        }

        UpdatePreview(color);
    }

    private Color GetCurrentColor(GameObject target)
    {
        MeshRenderer renderer = target.GetComponentInChildren<MeshRenderer>();

        if (renderer == null || renderer.material == null)
            return Color.white;

        Material mat = renderer.material;

        if (mat.HasProperty("_BaseColor"))
            return mat.GetColor("_BaseColor");

        if (mat.HasProperty("_Color"))
            return mat.GetColor("_Color");

        return Color.white;
    }

    private void UpdatePreview(Color color)
    {
        if (colorPreview != null)
            colorPreview.color = color;
    }

    private void ApplyMaterialColor(Material mat, Color color)
    {
        if (mat == null)
            return;

        // Color principal
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);

        // Color de borde
        if (mat.HasProperty("_EdgeColor"))
            mat.SetColor("_EdgeColor", color);

        // Alpha separado
        if (mat.HasProperty("_BaseAlpha"))
        {
            mat.SetFloat("_BaseAlpha", color.a);
            mat.SetFloat("_EdgeStrength", color.a);
        }
            

        // Alpha alternativo, por si tu shader usa _Alpha
        if (mat.HasProperty("_Alpha"))
            mat.SetFloat("_Alpha", color.a);

    }
}
