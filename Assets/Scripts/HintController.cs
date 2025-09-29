using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class HintController : MonoBehaviour
{
    [ReadOnly][SerializeField] private List<HintDetector> availableDetectors = new();

    public void RegisterDetector(HintDetector detector)
    {
        availableDetectors.Add(detector);
        NotifyAvailability();
        Debug.Log($"------- REGISTER DETECTOR -------"); 
    }

    public void ClearDetectors()
    {
        availableDetectors.Clear();
        NotifyAvailability();
        Debug.Log($"-------  Clear Detector -------"); 
    }

    public void ActivateHint()
    {
        if (availableDetectors.Count == 0)
        {
            Debug.LogWarning("[HintController] No available HintDetector.");
            return;
        }

        int index = Random.Range(0, availableDetectors.Count);
        HintDetector chosen = availableDetectors[index];

        availableDetectors.RemoveAt(index);
        chosen.ShowHint();

        NotifyAvailability();
    }

    private void NotifyAvailability()
    {
        bool hasHint = availableDetectors.Count > 0;
        PowerUpManager.Instance.UpdateHintAvailability(hasHint);
    }

    public bool HasAvailableDetectors()
    {
        return availableDetectors.Count > 0;
    }
}