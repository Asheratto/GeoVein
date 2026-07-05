using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WorldLabel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI typeText;

    [SerializeField] private Transform canvasTransform;
    [Header("Referencias")]
    [SerializeField] private Transform target;
    [SerializeField] private Camera mainCamera;
    //[SerializeField] private Transform.

    [Header("Config")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 8f, 0f);
    [SerializeField] private bool flip = true;

    private bool warnedTarget;
    private bool warnedCamera;


    [Header("Escala por distancia")]
    [SerializeField] private bool scaleWithDistance = true;
    [SerializeField] private float referenceDistance = 200f;
    [SerializeField] private float minScaleMultiplier = 0.8f;
    [SerializeField] private float maxScaleMultiplier = 2.5f;

    private Vector3 baseCanvasScale;

    public void Setup(Transform newTarget, string title, string type)
    {
        target = newTarget;

        if (titleText != null)
            titleText.text = title;

        if (typeText != null)
            typeText.text = $"({type})";

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (canvasTransform == null)
        {
            Canvas canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
                canvasTransform = canvas.transform;
        }

        if (canvasTransform != null)
            baseCanvasScale = canvasTransform.localScale;

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        // Seguir al objeto
        transform.position = target.position + offset;

        // Mirar hacia la cámara
        transform.LookAt(mainCamera.transform);

        if (flip)
            transform.Rotate(0f, 180f, 0f);

        // Escalar canvas según distancia
        if (scaleWithDistance && canvasTransform != null)
        {
            float distance = Vector3.Distance(mainCamera.transform.position, target.position);

            float scaleMultiplier = distance / referenceDistance;
            scaleMultiplier = Mathf.Clamp(scaleMultiplier, minScaleMultiplier, maxScaleMultiplier);

            canvasTransform.localScale = baseCanvasScale * scaleMultiplier;
        }
    }
}