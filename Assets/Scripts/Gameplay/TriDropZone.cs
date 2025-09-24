using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;
using TMPro;

public class TriDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private QuestionSortUI questionSortUI;

    [Header("Zone / Visuel")]
    [SerializeField] private RectTransform zoneRect;
    [SerializeField] private Image highlightImage;
    [SerializeField] private Color highlightColor = new Color(1, 1, 1, 0.15f);
    [SerializeField] private Color normalColor = new Color(1, 1, 1, 0f);

    [Header("Slots (ordre gauche→droite)")]
    [SerializeField] private List<RectTransform> slots = new List<RectTransform>();

    [Header("Debug Tri")]
    [SerializeField] private TextMeshProUGUI debugOrderText;
    [ReadOnly, SerializeField] private List<int> currentOrder = new List<int>();
    [ReadOnly, SerializeField] private int lastAssignedSlot = -1;

    private readonly Dictionary<FoodDraggableUI, int> itemToSlot = new Dictionary<FoodDraggableUI, int>();
    private bool isPointerOver, isDragging;
    private int activeSlots = 0;

    // Évènement : vrai si tous les slots sont remplis
    public System.Action<bool> OnFilledChanged;

    // ---------- Init ----------
    public void Init(List<PortionSelection> selections)
    {
        int n = (selections != null) ? selections.Count : slots.Count;
        ApplyActiveSlots(n);
        ResizeCurrentOrderToActive();
        RebuildCurrentOrderFromMap();

        FoodDraggableUI.OnAnyBeginDrag += HandleBeginDrag;
        FoodDraggableUI.OnAnyEndDrag += HandleEndDrag;
    }

    private void OnDisable()
    {
        FoodDraggableUI.OnAnyBeginDrag -= HandleBeginDrag;
        FoodDraggableUI.OnAnyEndDrag -= HandleEndDrag;
    }

    private void ApplyActiveSlots(int count)
    {
        activeSlots = Mathf.Clamp(count, 0, slots.Count);

        for (int i = 0; i < slots.Count; i++)
            if (slots[i]) slots[i].gameObject.SetActive(false);

        for (int i = 0; i < activeSlots; i++)
        {
            RectTransform rt = slots[i];
            if (!rt) continue;
            rt.gameObject.SetActive(true);

            SlotTriDrop d = rt.GetComponent<SlotTriDrop>();
            if (!d) d = rt.gameObject.AddComponent<SlotTriDrop>();
            d.Setup(this, i);
        }
    }

    // ---------- Highlight ----------
    private void HandleBeginDrag(FoodDraggableUI _) { isDragging = true; UpdateHighlight(); }
    private void HandleEndDrag(FoodDraggableUI _) { isDragging = false; UpdateHighlight(); }
    public void OnPointerEnter(PointerEventData _) { isPointerOver = true; UpdateHighlight(); }
    public void OnPointerExit(PointerEventData _) { isPointerOver = false; UpdateHighlight(); }

    private void UpdateHighlight()
    {
        if (!highlightImage) return;
        Color target = (isDragging && isPointerOver) ? highlightColor : normalColor;
        highlightImage.DOKill();
        highlightImage.DOColor(target, 0.12f);
    }

    public void OnDrop(PointerEventData _) { }

    // ---------- API appelée par SlotTriDrop ----------
    public void PlaceItemInSlot(FoodDraggableUI dragged, int slotIndex)
    {
        if (dragged == null) return;
        if (slotIndex < 0 || slotIndex >= activeSlots)
        {
            dragged.NotifyDropped();
            dragged.ResetToOriginal(true);
            return;
        }

        RectTransform draggedRT = (RectTransform)dragged.transform;
        RectTransform slotRT = slots[slotIndex];

        // Le dragged avait-il déjà un slot dans CETTE TriDropZone ?
        bool draggedHadSlot = itemToSlot.TryGetValue(dragged, out int previousSlotIndex);

        // Qui occupe le slot cible ?
        FoodDraggableUI occupant = GetOccupantAtSlot(slotIndex);

        // ---------- SLOT OCCUPÉ ----------
        if (occupant != null && occupant != dragged)
        {
            if (!draggedHadSlot)
            {
                // ⬅️ Vient du dock initial → on libère le slot en resetant l’occupant
                if (itemToSlot.ContainsKey(occupant)) itemToSlot.Remove(occupant);
                occupant.ResetToOriginal(true);

                // (pas de return : on laisse le code plus bas PLACER le dragged dans le slot)
            }
            else
            {
                // ⬅️ Les deux items sont déjà dans la zone → SWAP (l’occupant va vers l’ancien slot du dragged)
                int occupantIndex = itemToSlot[occupant];

                itemToSlot[occupant] = previousSlotIndex;
                itemToSlot[dragged] = occupantIndex;

                RectTransform oldSlotRT = slots[previousSlotIndex];
                occupant.transform.SetParent(oldSlotRT, worldPositionStays: true);
                occupant.transform.DOKill(false);

                Vector3 oldTarget = oldSlotRT.TransformPoint(oldSlotRT.rect.center);
                occupant.transform.DOMove(oldTarget, 0.25f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => ((RectTransform)occupant.transform).anchoredPosition = Vector2.zero);
            }
        }
        else
        {
            // ---------- SLOT VIDE ----------
            if (draggedHadSlot)
            {
                // Il quitte son ancien slot : libère l’entrée ; réassignée plus bas
                itemToSlot.Remove(dragged);
            }
            // Si vient du dock initial : autorisé, on continue
        }

        // ---------- Place / anime le DRAGGED dans le slot cible ----------
        draggedRT.DOKill(false);
        Vector3 targetWorld = slotRT.TransformPoint(slotRT.rect.center);

        draggedRT.SetParent(slotRT, worldPositionStays: true);
        draggedRT.localRotation = Quaternion.identity;
        draggedRT.localScale = Vector3.one;

        draggedRT.DOMove(targetWorld, 0.25f)
                 .SetEase(Ease.OutBack)
                 .OnComplete(() => draggedRT.anchoredPosition = Vector2.zero);

        // Map & état
        itemToSlot[dragged] = slotIndex;
        lastAssignedSlot = slotIndex;

        // Signaux/clean (garde ton comportement actuel)
        dragged.SetCurrentDropZone(null);
        dragged.NotifyDropped();

        questionSortUI.UpdateCurrentOrder(GetCurrentOrderIndices());

        RebuildCurrentOrderFromMap();
    }

    
    public void RemoveItem(FoodDraggableUI item)
    {
        if (itemToSlot.ContainsKey(item))
            itemToSlot.Remove(item);

        item.ResetToOriginal(true);
        lastAssignedSlot = -1;
        RebuildCurrentOrderFromMap();
    }

    public List<int> GetCurrentOrderIndices() => new List<int>(currentOrder);

    private FoodDraggableUI GetOccupantAtSlot(int slotIndex)
    {
        foreach (var kvp in itemToSlot)
            if (kvp.Value == slotIndex) return kvp.Key;
        return null;
    }

    private void RebuildCurrentOrderFromMap()
    {
        ResizeCurrentOrderToActive();

        for (int i = 0; i < activeSlots; i++)
            currentOrder[i] = 0; // vide

        foreach (var kvp in itemToSlot)
        {
            int slotIdx = kvp.Value;
            currentOrder[slotIdx] = kvp.Key.GetIndex() + 1; // +1 pour éviter 0
        }

        if (debugOrderText)
            debugOrderText.text = string.Join("-", currentOrder);

        OnFilledChanged?.Invoke(AreAllSlotsFilled());
    }

    private void ResizeCurrentOrderToActive()
    {
        if (currentOrder.Count != activeSlots)
        {
            currentOrder.Clear();
            for (int i = 0; i < activeSlots; i++) currentOrder.Add(0);
        }
    }

    public bool AreAllSlotsFilled()
    {
        for (int i = 0; i < activeSlots; i++)
            if (currentOrder[i] == 0) return false;
        return true;
    }
}