using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;

public class DropZoneUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform zoneRect;
    private Canvas zoneCanvas;

    [Header("Slots")]
    [SerializeField] private List<RectTransform> slots = new List<RectTransform>();

    [Header("Capacité")]
    [SerializeField, Min(1)] private int maxItems = 6;

    [Header("Highlight")]
    [SerializeField] private Image highlightImage;                    // assigne une Image (fond de zone)
    [SerializeField] private Color highlightColor = new Color(1, 1, 1, 0.15f);
    [SerializeField] private Color normalColor = new Color(1, 1, 1, 0f);


    // mapping item -> index slot
    private readonly Dictionary<FoodDraggableUI, int> itemToSlot = new Dictionary<FoodDraggableUI, int>();

    [Header("Debug (readonly)")]
    [ReadOnly, SerializeField] private int lastAssignedSlot = -1;
    [ReadOnly, SerializeField] private int currentCalories;

    private bool isPointerOver;
    private bool isDragging;

    private void Awake()
    {
        if (zoneCanvas == null) zoneCanvas = GetComponentInParent<Canvas>()?.rootCanvas;

        // surface raycastable minimale
        if (zoneRect != null)
        {
            Image img = zoneRect.GetComponent<Image>();
            if (img == null)
            {
                img = zoneRect.gameObject.AddComponent<Image>();
                img.color = new Color(1, 1, 1, 0);
            }
            img.raycastTarget = true;
        }

        if (highlightImage != null) highlightImage.color = normalColor;
    }

    private void OnEnable()
    {
        FoodDraggableUI.OnAnyBeginDrag += HandleBeginDrag;
        FoodDraggableUI.OnAnyEndDrag += HandleEndDrag;
    }

    private void OnDisable()
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

        var dragged = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<FoodDraggableUI>() : null;
        if (dragged == null) return;

        var draggedRT = dragged.transform as RectTransform;
        if (draggedRT == null) return;

        // autoriser le rearrangement même si plein
        bool alreadyInside =
            itemToSlot.ContainsKey(dragged) ||
            (draggedRT.parent != null && (draggedRT.parent == zoneRect || draggedRT.parent.IsChildOf(zoneRect)));

        // si nouveau et plein → refus (reset auto côté draggable)
        if (!alreadyInside && itemToSlot.Count >= maxItems)
            return;

        // libère ancien slot si existant
        if (itemToSlot.ContainsKey(dragged))
            itemToSlot.Remove(dragged);

        // position locale du pointeur → choix du slot le + proche
        Camera cam = (zoneCanvas != null && zoneCanvas.renderMode != RenderMode.ScreenSpaceOverlay) ? zoneCanvas.worldCamera : null;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(zoneRect, eventData.position, cam, out Vector2 local))
            return;

        int slotIndex = FindNearestFreeSlot(local);
        draggedRT.DOKill(false);

        if (slotIndex >= 0 && slotIndex < slots.Count && slots[slotIndex] != null)
        {
            RectTransform slot = slots[slotIndex];

            // cible monde = centre visuel du slot
            Vector3 worldTarget = slot.TransformPoint(slot.rect.center);

            // reparente en gardant la pos monde (évite le saut), puis tween world → centre
            draggedRT.SetParent(slot, worldPositionStays: true);
            draggedRT.localRotation = Quaternion.identity;
            draggedRT.localScale = Vector3.one;

            draggedRT.DOMove(worldTarget, 0.25f)
                     .SetEase(Ease.OutBack)
                     .OnComplete(() => { draggedRT.anchoredPosition = Vector2.zero; });

            itemToSlot[dragged] = slotIndex;
            lastAssignedSlot = slotIndex;

            dragged.SetCurrentDropZone(this);
            dragged.NotifyDropped();

            UpdateCalories();
        }
        // sinon : refus -> reset par FoodDraggableUI
    }

    private void UpdateCalories()
    {
        float total = 0f;

        foreach (var item in itemToSlot.Keys)
        {
            if (item == null) continue;

            FoodData food = item.GetFood();
            if (food == null) continue;

            float grams = item.GetGrams();           // quantité affichée sur la tuile
                                                     // Calories est déjà un int (pour 100 g). On calcule en float puis on convertit en int.
            total += food.Calories * (grams / 100f);
        }

        currentCalories = Mathf.RoundToInt(total);   // 👈 juste un int (pas d’arrondi par 25)
    }

    public void RemoveItem(FoodDraggableUI item)
    {
        if (item == null) return;

        if (itemToSlot.ContainsKey(item))
            itemToSlot.Remove(item);

        // retour à l’origine AVEC tween (le FoodDraggableUI gère via InitialDockUI si dispo)
        item.ResetToOriginal(true);
        lastAssignedSlot = -1;
        UpdateCalories();
    }

    private int FindNearestFreeSlot(Vector2 localPos)
    {
        if (slots == null || slots.Count == 0) return -1;

        HashSet<int> occupied = new HashSet<int>(itemToSlot.Values);

        int best = -1;
        float bestD2 = float.MaxValue;
        for (int i = 0; i < slots.Count; i++)
        {
            RectTransform s = slots[i];
            if (s == null || occupied.Contains(i)) continue;

            float d2 = (s.anchoredPosition - localPos).sqrMagnitude;
            if (d2 < bestD2) { bestD2 = d2; best = i; }
        }
        return best;
    }
}