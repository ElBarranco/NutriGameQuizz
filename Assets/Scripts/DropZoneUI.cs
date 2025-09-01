using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;
using TMPro;

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

    [SerializeField] private List<PortionSelection> PortionSelections;

    // mapping item -> index slot
    private readonly Dictionary<FoodDraggableUI, int> itemToSlot = new Dictionary<FoodDraggableUI, int>();

    [Header("Debug (readonly)")]
    [SerializeField] private TextMeshProUGUI debugCaloriesText;      // affichage debug des calories
    [ReadOnly, SerializeField] private int lastAssignedSlot = -1;
    [ReadOnly, SerializeField] private int currentCalories;

    // ➕ Debug ordre pour la question de tri (indices initiaux par slot, -1 si vide)
    [Header("Debug Tri (readonly)")]
    [ReadOnly, SerializeField] private List<int> currentOrder = new List<int>();
    [SerializeField] private TextMeshProUGUI debugOrderText; // assigné dans l’inspecteur

    private bool isPointerOver;
    private bool isDragging;

    private void Awake()
    {

        if (highlightImage != null) highlightImage.color = normalColor;

        // init debug ordre à la taille des slots si déjà set dans le prefab
        ResizeCurrentOrderToSlots();
    }

    public void Init(List<PortionSelection> currentPortionSelections)
    {
        this.PortionSelections = currentPortionSelections;

        // Pour le tri : la capacité = nombre de slots
        if (slots != null && slots.Count > 0)
            maxItems = slots.Count;

        ResizeCurrentOrderToSlots();
        RebuildCurrentOrderFromMap();

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

        FoodDraggableUI dragged = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<FoodDraggableUI>() : null;
        if (dragged == null) return;

        RectTransform draggedRT = dragged.transform as RectTransform;
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
            RebuildCurrentOrderFromMap(); // ➕ met à jour l’ordre de tri
        }
        // sinon : refus -> reset par FoodDraggableUI
    }

    private void UpdateCalories()
    {
        float total = 0f;

        foreach (KeyValuePair<FoodDraggableUI, int> kvp in itemToSlot)
        {
            FoodDraggableUI item = kvp.Key;
            if (item == null) continue;

            int index = item.GetIndex();
            PortionSelection sel = PortionSelections != null && index >= 0 && index < PortionSelections.Count
                ? PortionSelections[index]
                : default;

            total += sel.Value;
        }

        currentCalories = Mathf.RoundToInt(total);
        if (debugCaloriesText != null)
            debugCaloriesText.text = $"{currentCalories}";
    }

    public int GetCurrentCalories() => currentCalories;

    public void RemoveItem(FoodDraggableUI item)
    {
        if (item == null) return;

        if (itemToSlot.ContainsKey(item))
            itemToSlot.Remove(item);

        // retour à l’origine AVEC tween (le FoodDraggableUI gère via InitialDockUI si dispo)
        item.ResetToOriginal(true);
        lastAssignedSlot = -1;
        UpdateCalories();
        RebuildCurrentOrderFromMap(); // ➕ MAJ ordre
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

    // ========= ➕ Ajouts pour la question de tri =========

    /// <summary>
    /// Renvoie l’ordre courant par slot : taille = slots.Count.
    /// Chaque entrée vaut l’index initial (FoodDraggableUI.GetIndex()) ou -1 si slot vide.
    /// </summary>
    public List<int> GetCurrentOrderIndices()
    {
        return new List<int>(currentOrder); // copie défensive
    }

    private void RebuildCurrentOrderFromMap()
    {
        ResizeCurrentOrderToSlots();

        // reset à -1
        for (int i = 0; i < currentOrder.Count; i++) currentOrder[i] = -1;

        foreach (KeyValuePair<FoodDraggableUI, int> kvp in itemToSlot)
        {
            FoodDraggableUI item = kvp.Key;
            int slotIdx = kvp.Value;
            if (item == null) continue;
            if (slotIdx < 0 || slotIdx >= currentOrder.Count) continue;

            currentOrder[slotIdx] = item.GetIndex();
        }

        // === Debug affichage textuel (ex: "2-5-3-1") ===
        if (debugOrderText != null)
        {
            string orderStr = string.Join("-", currentOrder);
            debugOrderText.text = orderStr;
        }
    }

    private void ResizeCurrentOrderToSlots()
    {
        int n = (slots != null) ? slots.Count : 0;
        if (n <= 0) { currentOrder.Clear(); return; }

        if (currentOrder == null)
        {
            currentOrder = new List<int>(n);
        }

        if (currentOrder.Count != n)
        {
            currentOrder.Clear();
            for (int i = 0; i < n; i++) currentOrder.Add(-1);
        }
    }
}