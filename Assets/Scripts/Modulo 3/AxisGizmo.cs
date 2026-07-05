using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AxisGizmo : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("UI Config")]
    [SerializeField] private float axisLength = 38f;
    [SerializeField] private float lineThickness = 4f;
    [SerializeField] private float labelDistance = 48f;
    [SerializeField] private float labelSize = 18f;

    private RectTransform rect;

    private RectTransform xLine;
    private RectTransform yLine;
    private RectTransform zLine;

    private TMP_Text xLabel;
    private TMP_Text yLabel;
    private TMP_Text zLabel;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        CreateAxis("X", Color.red, out xLine, out xLabel);
        CreateAxis("Y", Color.green, out yLine, out yLabel);
        CreateAxis("Z", Color.blue, out zLine, out zLabel);
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        UpdateAxis(Vector3.right, xLine, xLabel);
        UpdateAxis(Vector3.up, yLine, yLabel);
        UpdateAxis(Vector3.forward, zLine, zLabel);
    }

    private void CreateAxis(string axisName, Color color, out RectTransform line, out TMP_Text label)
    {
        GameObject lineObj = new GameObject("Line_" + axisName);
        lineObj.transform.SetParent(transform, false);

        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = color;

        line = lineObj.GetComponent<RectTransform>();
        line.anchorMin = new Vector2(0.5f, 0.5f);
        line.anchorMax = new Vector2(0.5f, 0.5f);
        line.pivot = new Vector2(0f, 0.5f);
        line.anchoredPosition = Vector2.zero;
        line.sizeDelta = new Vector2(axisLength, lineThickness);

        GameObject labelObj = new GameObject("Label_" + axisName);
        labelObj.transform.SetParent(transform, false);

        label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = axisName;
        label.color = color;
        label.fontSize = labelSize;
        label.alignment = TextAlignmentOptions.Center;

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = new Vector2(30f, 30f);
    }

    private void UpdateAxis(Vector3 worldAxis, RectTransform line, TMP_Text label)
    {
        Vector3 camRight = targetCamera.transform.right;
        Vector3 camUp = targetCamera.transform.up;

        Vector2 dir = new Vector2(
            Vector3.Dot(camRight, worldAxis),
            Vector3.Dot(camUp, worldAxis)
        );

        if (dir.sqrMagnitude < 0.001f)
            dir = Vector2.zero;
        else
            dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        line.anchoredPosition = Vector2.zero;
        line.localRotation = Quaternion.Euler(0f, 0f, angle);

        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchoredPosition = dir * labelDistance;
    }
}