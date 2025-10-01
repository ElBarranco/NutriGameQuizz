using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class HintController : MonoBehaviour
{
    [SerializeField] private RectTransform hintEffectPrefab;
    [SerializeField] private RectTransform uiCanvasRoot;

    private List<HintDetector> availableDetectors = new();

    public void RegisterDetector(HintDetector detector)
    {
        if (!availableDetectors.Contains(detector))
        {
            availableDetectors.Add(detector);
            NotifyAvailability();
        }
    }

    public void ClearDetectors()
    {
        availableDetectors.Clear();
        NotifyAvailability();
    }

    public void ActivateHint(RectTransform buttonOrigin)
    {
        if (availableDetectors.Count == 0)
        {
            Debug.LogWarning("[HintController] No available HintDetector.");
            return;
        }

        int index = Random.Range(0, availableDetectors.Count);
        HintDetector chosen = availableDetectors[index];
        availableDetectors.RemoveAt(index);
        NotifyAvailability();
        RectTransform target = chosen.GetSignHintPanel();

        // 💥 Crée l’effet visuel
        RectTransform instance = Instantiate(hintEffectPrefab, uiCanvasRoot);
        instance.position = buttonOrigin.position;

        float durationTween = 0.5f;

        // 💫 Animation vers la cible
        instance.DOMove(target.position, durationTween).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            Destroy(instance.gameObject);
            chosen.ShowHint();
        });
    }

    private void NotifyAvailability()
    {
        bool hasHint = HasAvailableDetectors();
        PowerUpManager.Instance.UpdateHintAvailability(hasHint);
    }


    public bool HasAvailableDetectors()
    {
        return availableDetectors.Count > 0;
    }
}