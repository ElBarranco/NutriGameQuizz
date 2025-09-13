using UnityEngine;
using UnityEngine.EventSystems;

public class KillZoneUI : MonoBehaviour, IDropHandler
{


    public void OnDrop(PointerEventData eventData)
    {
        FoodConveyorItemUI item = eventData.pointerDrag?.GetComponent<FoodConveyorItemUI>();
        if (item == null) return;

        bool isIntruder = item.IsIntruder();
        // Feedback visuel
        FeedbackSpawner.Instance.SpawnFeedbackAtRect(item.GetComponent<RectTransform>(), isIntruder);

        // Enregistrement score
        ScoreManager.Instance.EnregistrerRecyclingAnswer(isIntruder);

        // ✅ Intrus -> bonne réponse (rejet)
        if (isIntruder)
        {
            item.PlayCollectedAnimation();
        }
        else
        {
            item.PlayRejectedAnimation();
        }


    }
}