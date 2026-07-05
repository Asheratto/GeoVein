using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BottomTargetNavigator : MonoBehaviour
{
    public static BottomTargetNavigator instance;

    [Header("Barra inferior")]
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Cámara")]
    [SerializeField] private SceneLikeCameraController cameraController;

    [Header("Ficha del elemento")]
    [SerializeField] private FichaElementoUI fichaUI;

    private readonly List<MapTarget> targets = new List<MapTarget>();
    private readonly List<Card> cards = new List<Card>();

    private int currentIndex = -1;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public void Clear()
    {
        foreach (MapTarget target in targets)
        {
            SetLabelActive(target, false);
        }

        targets.Clear();
        cards.Clear();
        currentIndex = -1;

        Refresh();
    }

    public void AddTarget(MapTarget target, Card card)
    {
        if (target == null)
            return;

        targets.Add(target);
        cards.Add(card);

        SetLabelActive(target, false);

        if (currentIndex == -1)
        {
            currentIndex = 0;
            Refresh();
        }
    }

    public void Next()
    {
        Move(1);
    }

    public void Previous()
    {
        Move(-1);
    }

    private void Move(int direction)
    {
        if (targets.Count == 0)
            return;

        if (currentIndex >= 0 && currentIndex < targets.Count)
            SetLabelActive(targets[currentIndex], false);

        currentIndex += direction;

        if (currentIndex >= targets.Count)
            currentIndex = 0;

        if (currentIndex < 0)
            currentIndex = targets.Count - 1;

        Refresh();
    }

    private void Refresh()
    {
        if (targets.Count == 0 || currentIndex < 0 || currentIndex >= targets.Count)
        {
            if (nameText != null)
                nameText.text = "Sin selección";

            if (fichaUI != null)
                fichaUI.Limpiar();

            return;
        }

        MapTarget currentTarget = targets[currentIndex];
        Card currentCard = cards[currentIndex];

        SetLabelActive(currentTarget, true);

        if (nameText != null)
            nameText.text = currentTarget.displayName;

        if (cameraController != null)
            cameraController.Focus(currentTarget.GetFocusPosition());

        RefreshFicha(currentCard);
    }

    private void RefreshFicha(Card card)
    {
        if (fichaUI == null)
            return;

        if (card == null || card.Source == null)
        {
            fichaUI.Limpiar();
            return;
        }

        fichaUI.Mostrar(card.Source);
    }

    private void SetLabelActive(MapTarget target, bool active)
    {
        if (target == null)
            return;

        WorldLabel label = target.GetComponentInChildren<WorldLabel>(true);

        if (label == null)
            return;

        label.gameObject.SetActive(active);
    }
}