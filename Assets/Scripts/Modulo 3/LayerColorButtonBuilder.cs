using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LayerColorButtonBuilder : MonoBehaviour
{
    [System.Serializable]
    public class LayerGroup
    {
        public string groupName;
        public Transform parent;
    }

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private TMP_Text headerPrefab;
    [SerializeField] private LayerColorEditor colorEditor;

    [Header("Layers")]
    [SerializeField] private List<LayerGroup> groups = new();

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

            foreach (Transform child in group.parent)
            {
                CreateButton(child.name, child.gameObject);
            }
        }
    }

    private void CreateHeader(string title)
    {
        TMP_Text header = Instantiate(headerPrefab, contentParent);
        header.text = title;
        header.gameObject.name = "Header_Color_" + title;
    }

    private void CreateButton(string label, GameObject target)
    {
        Button button = Instantiate(buttonPrefab, contentParent);
        button.gameObject.name = "Button_Color_" + label.Replace(" ", "_");

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
            text.text = label;

        button.onClick.AddListener(() =>
        {
            colorEditor.SetTarget(target);
        });
    }

    private void Clear()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}