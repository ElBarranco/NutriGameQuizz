using UnityEngine;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;

public class LifeSystem : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private TextMeshProUGUI lifeText;
    [SerializeField] private Transform heartIcon;
    [SerializeField] private CanvasGroup lifeLostFeedback;


    private int currentLives;

    private void Awake()
    {
        gameObject.SetActive(false); // Inactif par défaut
    }

    public void Init()
    {
        currentLives = 3;
        gameObject.SetActive(true);
        UpdateLifeUI();
        HideFeedbackImmediate();
    }


    public void LoseLife()
    {
        if (currentLives <= 0) return;

        currentLives--;
        UpdateLifeUI();
        PlayHeartShake();
        ShowLifeLostFeedback();
    }

    private void UpdateLifeUI()
    {
        lifeText.text = currentLives.ToString();
    }

    private void PlayHeartShake()
    {
        heartIcon.DOKill();
        heartIcon.DOShakeScale(0.5f, 0.5f, 10, 90f);
    }

    private void ShowLifeLostFeedback()
    {
        lifeLostFeedback.DOKill();
        lifeLostFeedback.alpha = 0f;
        lifeLostFeedback.gameObject.SetActive(true);
        lifeLostFeedback.DOFade(1f, 0.3f)
            .OnComplete(() =>
            {
                lifeLostFeedback.DOFade(0f, 0.5f)
                    .SetDelay(0.7f)
                    .OnComplete(() => lifeLostFeedback.gameObject.SetActive(false));
            });
    }

    private void HideFeedbackImmediate()
    {
        lifeLostFeedback.alpha = 0f;
        lifeLostFeedback.gameObject.SetActive(false);
    }
}