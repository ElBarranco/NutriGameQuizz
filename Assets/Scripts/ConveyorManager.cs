using UnityEngine;
using System.Collections.Generic;

public class ConveyorManager : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private ConveyorSlotUI slotPrefab;   // ton template de slot
    [SerializeField] private Transform slotParent;        // parent qui contient tous les slots instanciés
    [SerializeField] private RectTransform endPoint;
    [SerializeField] private float speed = 100f;


    private readonly List<ConveyorSlotUI> slots = new List<ConveyorSlotUI>();
    private int nextSlotIndex = 0;



    public void GenerateSlots(int count)
    {
        // Nettoyer l’existant
        foreach (var s in slots)
        {
            if (s != null) Destroy(s.gameObject);
        }
        slots.Clear();

        // Recréer les slots
        for (int i = 0; i < count; i++)
        {
            ConveyorSlotUI slot = Instantiate(slotPrefab, slotParent);
            slot.gameObject.name = $"Slot_{i}";
            slots.Add(slot);
        }

        nextSlotIndex = 0;
    }

    public void ActivateSlot(ConveyorSlotUI slot)
    {
        slot.Init(endPoint, speed);
    }

    public ConveyorSlotUI GetNextFreeSlot()
    {
        if (slots.Count == 0) return null;

        ConveyorSlotUI slot = slots[nextSlotIndex];
        nextSlotIndex = (nextSlotIndex + 1) % slots.Count; // tourne en boucle
        return slot;
    }
}