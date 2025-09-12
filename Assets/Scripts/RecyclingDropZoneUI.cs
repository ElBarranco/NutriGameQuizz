using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class RecyclingDropZoneUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Capacité")]
    [SerializeField, Min(1)] private int maxItems = 20;

    [Header("Feedback UI")]
    [SerializeField] private RectTransform zoneRect;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private Image highlightImage;                  // assigne une Image (fond semi-transparent)
    [SerializeField] private Color highlightColor = new Color(1, 1, 1, 0.15f);
    [SerializeField] private Color normalColor = new Color(1, 1, 1, 0f);

    // Liste des items acceptés
    private readonly List<FoodDraggableUI> acceptedItems = new List<FoodDraggableUI>();

    // Résultat encodé (0 = intrus, 1 = valide)
    [ReadOnly, SerializeField] private List<int> currentAnswers = new List<int>();

    private bool isPointerOver;
    private bool isDragging;

    private void Awake()
    {
        if (highlightImage != null) highlightImage.color = normalColor;

        // on écoute les événements globaux de drag
        FoodDraggableUI.OnAnyBeginDrag += HandleBeginDrag;
        FoodDraggableUI.OnAnyEndDrag += HandleEndDrag;
    }

    private void OnDestroy()
    {
        FoodDraggableUI.OnAnyBeginDrag -= HandleBeginDrag;
        FoodDraggableUI.OnAnyEndDrag -= HandleEndDrag;
    }

    private void HandleBeginDrag(FoodDraggableUI _)
    {
        isDragging = true;
        UpdateHighlight();
    }

    private void HandleEndDrag(FoodDraggableUI _)
    {
        isDragging = false;
        UpdateHighlight();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        UpdateHighlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        if (!highlightImage) return;
        Color target = (isDragging && isPointerOver) ? highlightColor : normalColor;
        highlightImage.DOKill();
        highlightImage.DOColor(target, 0.12f);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (zoneRect == null) return;

        FoodDraggableUI dragged = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<FoodDraggableUI>() : null;
        if (dragged == null) return;

        if (acceptedItems.Count >= maxItems) return; // limite atteinte

        // Vérifie si intrus ou pas
        bool isValid = IsFoodValid(dragged.GetFood());

        // Enregistre la réponse
        acceptedItems.Add(dragged);
        currentAnswers.Add(isValid ? 1 : 0);

        // Reparenter visuellement dans la zone
        RectTransform draggedRT = dragged.GetComponent<RectTransform>();
        draggedRT.SetParent(zoneRect, worldPositionStays: true);
        draggedRT.anchoredPosition = Vector2.zero;

        if (isValid)
        {
            // ✅ si bon aliment : animation de disparition
            if (dragged is FoodConveyorItemUI conveyorItem)
                conveyorItem.PlayCollectedAnimation();
            else
                Destroy(dragged.gameObject);

            // Spawn feedback correct

        }
        else
        {
            // ❌ si intrus : pour l’instant on le garde
            // Exemple : (dragged as FoodConveyorItemUI)?.PlayRejectedAnimation();

            // Spawn feedback faux

        }
        FeedbackSpawner.Instance.SpawnFeedbackAtRect(zoneRect, isValid);
        ScoreManager.Instance.EnregistrerRecyclingAnswer(isValid);
        UpdateDebug();
        UpdateHighlight();
    }

    private void UpdateDebug()
    {
        if (debugText != null)
        {
            debugText.text = "Réponses: " + string.Join(",", currentAnswers);
        }
    }

    public List<int> GetAnswers()
    {
        return new List<int>(currentAnswers); // copie défensive
    }

    // Ici tu choisis la logique (ex: selon Proteines, Glucides, Lipides)
    private bool IsFoodValid(FoodData food)
    {
        return food.Proteins >= 2f; // ex: règle protéines
    }
}