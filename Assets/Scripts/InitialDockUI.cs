using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InitialDockUI : MonoBehaviour
{
    [SerializeField] private RectTransform dockRect;
    [SerializeField] private List<RectTransform> slots = new List<RectTransform>();

    public int SlotCount => slots?.Count ?? 0;

    public void PlaceAtIndex(FoodDraggableUI item, int index, bool tween)
    {
        if (item == null || index < 0 || index >= SlotCount || slots[index] == null) return;

        var itemRT = (RectTransform)item.transform;
        var slot = slots[index];

        itemRT.DOKill(false);

        // cible monde = centre du slot
        Vector3 worldTarget = slot.TransformPoint(Vector3.zero);

        // 1) Reparent en conservant la position monde (pas de “saut” visuel)
        itemRT.SetParent(slot, worldPositionStays: true);
        itemRT.localRotation = Quaternion.identity;
        itemRT.localScale = Vector3.one;

        if (tween)
        {
            // 2) Tween en world-space vers le centre du slot
            itemRT.DOMove(worldTarget, 0.25f)
                  .SetEase(Ease.OutBack)
                  .OnComplete(() =>
                  {
                      // 3) Verrouille en local pour éviter les dérives d’arrondi
                      itemRT.anchoredPosition = Vector2.zero;
                  });
        }
        else
        {
            itemRT.position = worldTarget;
            itemRT.anchoredPosition = Vector2.zero;
        }
    }
}