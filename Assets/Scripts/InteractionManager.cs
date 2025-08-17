using UnityEngine;
using CandyCoded.HapticFeedback;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern simple
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TriggerLightVibration()
    {
        HapticFeedback.LightFeedback();
    }

    public void TriggerMediumVibration()
    {
        HapticFeedback.MediumFeedback();
    }

    public void TriggerHeavyVibration()
    {
        HapticFeedback.HeavyFeedback();
    }
}