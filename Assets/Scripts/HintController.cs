using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class HintController : MonoBehaviour
{
    [SerializeField] private RectTransform hintEffectPrefab;
    [SerializeField] private RectTransform uiCanvasRoot;

    private List<HintDetector> availableDetectors = new();


    private HintDetector currentChosen;

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
        currentChosen = availableDetectors[index];
        availableDetectors.RemoveAt(index);
        NotifyAvailability();
        RectTransform target = currentChosen.GetSignHintPanel();

        // 💥 Crée l’effet visuel
        RectTransform instance = Instantiate(hintEffectPrefab, uiCanvasRoot);
        instance.position = buttonOrigin.position;

        float durationTween = 0.45f;
        Invoke(nameof(CallShotHint), durationTween - 0.1f);
        // 💫 Animation vers la cible
        instance.DOMove(target.position, durationTween).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            Destroy(instance.gameObject);
        });
    }

    private void CallShotHint()
    {
        currentChosen.ShowHint();
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