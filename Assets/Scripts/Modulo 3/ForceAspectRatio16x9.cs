using UnityEngine;

public class ForceAspectRatio16x9 : MonoBehaviour
{
    private const float TargetAspect = 16f / 9f;

    [SerializeField] private int minWidth = 960;
    [SerializeField] private int minHeight = 540;

    private int lastWidth;
    private int lastHeight;
    private bool correcting;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Screen.fullScreenMode = FullScreenMode.Windowed;
        Screen.fullScreen = false;

        Set16x9Resolution(1280, 720);
    }

    private void Update()
    {
        if (correcting)
            return;

        int currentWidth = Screen.width;
        int currentHeight = Screen.height;

        if (currentWidth == lastWidth && currentHeight == lastHeight)
            return;

        lastWidth = currentWidth;
        lastHeight = currentHeight;

        float currentAspect = (float)currentWidth / currentHeight;

        if (Mathf.Abs(currentAspect - TargetAspect) > 0.01f)
        {
            CorrectResolution(currentWidth, currentHeight);
        }
    }

    private void CorrectResolution(int width, int height)
    {
        correcting = true;

        int newWidth = width;
        int newHeight = Mathf.RoundToInt(width / TargetAspect);

        if (newHeight > height)
        {
            newHeight = height;
            newWidth = Mathf.RoundToInt(height * TargetAspect);
        }

        newWidth = Mathf.Max(newWidth, minWidth);
        newHeight = Mathf.Max(newHeight, minHeight);

        // Asegurar que quede exactamente 16:9.
        newHeight = Mathf.RoundToInt(newWidth / TargetAspect);

        Set16x9Resolution(newWidth, newHeight);

        correcting = false;
    }

    private void Set16x9Resolution(int width, int height)
    {
        Screen.SetResolution(width, height, false);

        lastWidth = width;
        lastHeight = height;
    }
}