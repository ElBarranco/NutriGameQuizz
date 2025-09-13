// ConveyorManager.cs
using UnityEngine;
using System.Collections.Generic;

public class ConveyorManager : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private ConveyorSlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private RectTransform endPoint;

    [Header("Tweaks")]
    [SerializeField] private float easySpeed = 60f;
    [SerializeField] private float normalSpeed = 100f;
    [SerializeField] private float hardSpeed = 160f;

    private float speed;

    private readonly List<ConveyorSlotUI> slots = new List<ConveyorSlotUI>();
    private int nextSlotIndex = 0;

    public void Init()
    {
        // Définit la vitesse initiale selon la difficulté
        switch (DifficultyManager.Instance.CurrentDifficulty)
        {
            case DifficultyLevel.Easy:
                speed = easySpeed;
                break;
            case DifficultyLevel.Medium:
                speed = normalSpeed;
                break;
            case DifficultyLevel.Hard:
                speed = hardSpeed;
                break;
            default:
                speed = normalSpeed;
                break;
        }
    }
    public void GenerateSlots(int count)
    {
        // nettoie l’existant
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i] != null) Destroy(slots[i].gameObject);
        }
        slots.Clear();

        // recrée et place derrière (SetAsFirstSibling)
        for (int i = 0; i < count; i++)
        {
            ConveyorSlotUI slot = Instantiate(slotPrefab, slotParent);
            slot.transform.SetAsFirstSibling();   // derrière visuellement
            slot.gameObject.name = $"Slot_{i}";
            // IMPORTANT : on n’appelle PAS Init ici (il bougera quand on y mettra un item)
            slots.Add(slot);
        }
        nextSlotIndex = 0;
    }

    /// <summary>Retourne le prochain slot circulairement.</summary>
    public ConveyorSlotUI GetNextFreeSlot()
    {
        if (slots.Count == 0) return null;
        var slot = slots[nextSlotIndex];
        nextSlotIndex = (nextSlotIndex + 1) % slots.Count;
        return slot;
    }


    public void ActivateSlot(ConveyorSlotUI slot)
    {
        if (slot == null) return;
        slot.Init(endPoint, speed);
    }


    public void SetPaused(bool paused)
    {
        foreach (ConveyorSlotUI slot in slots)
        {
            slot.SetPaused(paused);
        }
    }


    public void BroadcastSpeed(float newSpeed)
    {
        speed = newSpeed;
        foreach (ConveyorSlotUI slot in slots)
        {
            slot.SetSpeed(newSpeed);
        }
    }


    private void OnEnable()
    {
        FoodConveyorItemUI.OnAnyBeginDragConveyor += HandleBeginDrag;
        FoodConveyorItemUI.OnAnyEndDragConveyor += HandleEndDrag;
    }

    private void OnDisable()
    {
        FoodConveyorItemUI.OnAnyBeginDragConveyor -= HandleBeginDrag;
        FoodConveyorItemUI.OnAnyEndDragConveyor -= HandleEndDrag;
    }

    private void HandleBeginDrag(FoodConveyorItemUI item)
    {
        if (DifficultyManager.Instance.CurrentDifficulty == DifficultyLevel.Easy)
            SetPaused(true);
    }

    private void HandleEndDrag(FoodConveyorItemUI item)
    {
        if (DifficultyManager.Instance.CurrentDifficulty == DifficultyLevel.Easy)
            SetPaused(false);
    }
}