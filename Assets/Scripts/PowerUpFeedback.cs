using UnityEngine;
using TMPro;
using DG.Tweening;

public class PowerUpFeedback : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;


    private Vector3 initialPosition;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        canvasGroup.alpha = 0;
    }

    public void Play(int amount)
    {
        text.text = $"+{amount}";

        transform.localPosition = initialPosition;
        canvasGroup.alpha = 1f;

        canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.OutQuad);

        transform.DOLocalMoveY(initialPosition.y + 160f, 0.6f).SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                transform.localPosition = initialPosition;
            });
    }
}