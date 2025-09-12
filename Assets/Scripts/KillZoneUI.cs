using UnityEngine;
using UnityEngine.EventSystems;

public class KillZoneUI : MonoBehaviour, IDropHandler
{


    public void OnDrop(PointerEventData eventData)
    {
        FoodConveyorItemUI item = eventData.pointerDrag?.GetComponent<FoodConveyorItemUI>();
        if (item == null) return;

        bool isCorrect = item.IsIntruder();
        FeedbackSpawner.Instance.SpawnFeedbackAtRect(item.GetComponent<RectTransform>(), isCorrect);
        ScoreManager.Instance.EnregistrerRecyclingAnswer(isCorrect);

        item.PlayCollectedAnimation(); // scale down puis destroy 


    }
}