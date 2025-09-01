using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotTriDrop : MonoBehaviour, IDropHandler
{
    [SerializeField] private int index;
    private TriDropZone owner;

    public void Setup(TriDropZone dropZone, int slotIndex)
    {
        owner = dropZone;
        index = slotIndex;


    }

    public void OnDrop(PointerEventData eventData)
    {
        if (owner == null || eventData.pointerDrag == null) return;

        FoodDraggableUI item = eventData.pointerDrag.GetComponent<FoodDraggableUI>();
        if (!item) return;

        owner.PlaceItemInSlot(item, index);
    }
}