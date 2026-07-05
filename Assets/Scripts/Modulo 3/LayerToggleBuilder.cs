using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LayerToggleBuilder : MonoBehaviour
{
    public static LayerToggleBuilder instance;

    [System.Serializable]
    public class LayerGroup
    {
        public string groupName;
        public Transform parent;
        public bool includeParentToggle = true;
        public bool includeChildrenToggles = true;
    }

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private Toggle togglePrefab;
    [SerializeField] private TMP_Text headerPrefab;

    [Header("Layers")]
    [SerializeField] private List<LayerGroup> groups = new();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnEnable()
    {
        Build();
    }

    public void Build()
    {
        Clear();

        foreach (LayerGroup group in groups)
        {
            if (group.parent == null)
                continue;

            CreateHeader(group.groupName);

            if (group.includeParentToggle)
            {
                CreateToggle("Todo " + group.groupName, group.parent.gameObject);
            }

            if (group.includeChildrenToggles)
            {
                foreach (Transform child in group.parent)
                {
                    CreateToggle(child.name, child.gameObject);
                }
            }
        }
    }

    private void CreateHeader(string title)
    {
        if (headerPrefab == null)
            return;

        TMP_Text header = Instantiate(headerPrefab, contentParent);
        header.text = title;
    }

    private void CreateToggle(string label, GameObject target)
    {
        if (togglePrefab == null || target == null)
            return;

        Toggle toggle = Instantiate(togglePrefab, contentParent);

        // Nombre en la jerarquía
        toggle.gameObject.name = "Toggle_" + CleanName(label);

        // Texto visible dentro del toggle
        Text text = toggle.GetComponentInChildren<Text>(true);
        if (text != null)
            text.text = label;

        toggle.SetIsOnWithoutNotify(target.activeSelf);

        toggle.onValueChanged.AddListener((visible) =>
        {
            target.SetActive(visible);
        });
    }

    private string CleanName(string text)
    {
        return text
            .Replace(" ", "_")
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ñ", "n")
            .Replace("/", "_")
            .Replace("\\", "_");
    }

    private void Clear()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}
