using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private RectTransform spinner;

    private bool isLoading;

    public static LoadingUI instance;


    private void Update()
    {
        if (isLoading && spinner != null)
        {
            spinner.Rotate(0f, 0f, -180f * Time.deltaTime);
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        loadingPanel.SetActive(false);
        instance = this;
    }

    public void Show()
    {
        loadingPanel.SetActive(true);
        isLoading = true;
    }

    public void Hide()
    {
        loadingPanel.SetActive(false);
        isLoading = false;
        
    }
}
