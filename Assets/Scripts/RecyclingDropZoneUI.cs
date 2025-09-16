using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class RecyclingDropZoneUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Feedback UI")]
    [SerializeField] private RectTransform zoneRect;

    [SerializeField] private Image highlightImage;
    [SerializeField] private Color highlightColor = new Color(1, 1, 1, 0.15f);
    [SerializeField] private Color normalColor = new Color(1, 1, 1, 0f);

    private readonly List<FoodDraggableUI> acceptedItems = new List<FoodDraggableUI>();


    private bool isPointerOver;   // état courant "over"
    private bool isDragging;      // vrai si un item est en train d'être drag
    private Canvas rootCanvas;    // pour le raycast écran → UI

    private void Awake()
    {
        if (highlightImage != null)
        {
            highlightImage.color = normalColor;
            // évite que l'image de surbrillance bloque des raycasts
            highlightImage.raycastTarget = false;
        }

        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;

        // écoute globale des débuts/fins de drag
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
        // on force un 1er refresh
        RecomputePointerOver();
    }

    private void HandleEndDrag(FoodDraggableUI _)
    {
        isDragging = false;
        // fin de drag ⇒ on coupe le highlight
        if (isPointerOver)
        {
            isPointerOver = false;
            UpdateHighlight();
        }
        else
        {
            UpdateHighlight();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // on garde ces callbacks (PC/éditeur), mais l’Update ci-dessous corrige les trous mobile
        isPointerOver = true;
        UpdateHighlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        UpdateHighlight();
    }

    private void Update()
    {
        // 🔁 Recalcule robuste du "over" en drag (mobile-friendly)
        if (isDragging)
        {
            bool before = isPointerOver;
            RecomputePointerOver();
            if (isPointerOver != before)
                UpdateHighlight();
        }
    }

    private void RecomputePointerOver()
    {
        if (zoneRect == null) { isPointerOver = false; return; }

        Camera cam = null;
        if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = rootCanvas.worldCamera;

        // Input.mousePosition fonctionne pour le premier touch sur mobile
        isPointerOver = RectTransformUtility.RectangleContainsScreenPoint(zoneRect, Input.mousePosition, cam);
    }

    private void UpdateHighlight()
    {
        if (!highlightImage) return;
        // ✅ actif SEULEMENT si on drag ET qu’on est au-dessus
        Color target = (isDragging && isPointerOver) ? highlightColor : normalColor;
        highlightImage.DOKill();
        highlightImage.DOColor(target, 0.12f);
    }

    public void OnDrop(PointerEventData eventData)
    {
        FoodConveyorItemUI dragged = eventData.pointerDrag
            ? eventData.pointerDrag.GetComponent<FoodConveyorItemUI>()
            : null;
        if (dragged == null) return;

        // ✅ L’item sait s’il est intrus
        bool isValid = !dragged.IsIntruder();

        acceptedItems.Add(dragged);

        RectTransform draggedRT = dragged.GetComponent<RectTransform>();
        draggedRT.SetParent(zoneRect, worldPositionStays: true);
        draggedRT.anchoredPosition = Vector2.zero;

        if (isValid)
            dragged.PlayCollectedAnimation();
        else
            dragged.PlayRejectedAnimation();

        FeedbackSpawner.Instance.SpawnFeedbackAtRect(zoneRect, isValid);
        ScoreManager.Instance.EnregistrerRecyclingAnswer(isValid);

        isPointerOver = false;
        isDragging = false;

        UpdateHighlight();
    }




}