using UnityEngine;
using DG.Tweening;

public class StreakRewardVisual : MonoBehaviour
{
    [SerializeField] private RectTransform rewardEffectPrefab;
    [SerializeField] private RectTransform uiCanvasRoot;
    [SerializeField] private RectTransform spawnPoint;

    public void PlayEffectTowards(RectTransform target)
    {
        RectTransform instance = Instantiate(rewardEffectPrefab, uiCanvasRoot);
        instance.position = spawnPoint.position;

        float duration = 1f;

        instance.DOMove(target.position, duration).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            Destroy(instance.gameObject);
        });
    }
}