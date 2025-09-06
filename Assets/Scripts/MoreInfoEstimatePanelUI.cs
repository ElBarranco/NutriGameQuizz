using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class MoreInfoEstimatePanelUI : MoreInfoPanelBase
{
    [SerializeField] private TextMeshProUGUI foodName;
    [SerializeField] private TextMeshProUGUI realValueText;
    [SerializeField] private TextMeshProUGUI playerGuessText;

    [SerializeField] private Image fillSprite;
    [SerializeField] private Image foodImage;

    private Sequence showSeq;
    private Tweener revealTweener;

    private void OnDisable() => KillAllTweens();
    private void OnDestroy() => KillAllTweens();

    public void Show(FoodData food, PortionSelection portion, float playerGuess, QuestionSubType sousType)
    {
        KillAllTweens();

        float realValue = portion.Value;
        float guessValue = playerGuess;

        foodImage.sprite = SpriteLoader.LoadFoodSprite(food.Name);
        foodName.text = PortionTextFormatter.ToDisplayWithFood(food, portion);

        panel.SetActive(true);
        canvasGroup.alpha = 0f;
        panel.transform.localScale = Vector3.zero;

        showSeq = DOTween.Sequence().SetTarget(this);
        showSeq.Append(canvasGroup.DOFade(1f, 0.3f).SetTarget(canvasGroup));
        showSeq.Join(panel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetTarget(panel.transform));

        AnimateFillAndValues(realValue, guessValue, sousType);
    }

    private void AnimateFillAndValues(float realValue, float guessValue, QuestionSubType sousType)
    {
        if (fillSprite != null)
        {
            fillSprite.DOKill();
            fillSprite.fillAmount = 0f;

            showSeq.Join(
                fillSprite
                    .DOFillAmount(1f, 1f)
                    .SetEase(Ease.OutCubic)
                    .SetTarget(fillSprite)
            );
        }

        realValueText.text = TextFormatter.ToDisplayValue(sousType, 0f);

        playerGuessText.text = TextFormatter.ToDisplayValue(sousType, guessValue);

        showSeq.AppendInterval(0.25f);
        showSeq.AppendCallback(() =>
        {
            revealTweener?.Kill();

            float animVal = 0f;
            revealTweener = DOTween
                .To(() => animVal, x =>
                {
                    animVal = x;
                    realValueText.text = TextFormatter.ToDisplayValue(sousType, animVal);
                }, realValue, 1f)
                .SetEase(Ease.OutCubic)
                .SetTarget(realValueText);
        });
    }

    private void KillAllTweens()
    {
        showSeq?.Kill();
        revealTweener?.Kill();

        canvasGroup.DOKill();
        panel.transform.DOKill();
        fillSprite.DOKill();
        realValueText.DOKill();
        playerGuessText.DOKill();
    }
}