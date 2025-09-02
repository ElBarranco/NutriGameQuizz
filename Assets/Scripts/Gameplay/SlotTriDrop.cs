using UnityEngine;
using UnityEngine.EventSystems;

public class SlotTriDrop : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int index;
    private TriDropZone owner;

    [Header("Feedback Visuel")]
    [SerializeField] private GameObject hoverFeedbackGO;

    public void Setup(TriDropZone dropZone, int slotIndex)
    {
        owner = dropZone;
        index = slotIndex;
        hoverFeedbackGO.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var item = eventData.pointerDrag.GetComponent<FoodDraggableUI>();
        owner.PlaceItemInSlot(item, index);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverFeedbackGO.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverFeedbackGO.SetActive(false);
    }
}