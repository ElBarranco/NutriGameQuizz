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

    // --- Réfs tween pour cleanup ---
    private Sequence showSeq;          // séquence d’apparition + anims
    private Tweener revealTweener;     // DOTween.To pour la vraie valeur

    private void OnDisable() => KillAllTweens();
    private void OnDestroy() => KillAllTweens();

    public void Show(FoodData food, float playerGuess, QuestionSubType sousType)
    {
        KillAllTweens(); // évite les overlaps si Show est rappelé rapidement

        string unit = "";
        float realValue = 0f;
        float guessValue = playerGuess;

        switch (sousType)
        {
            case QuestionSubType.Calorie:  unit = "kcal"; realValue = food.Calories;      break;
            case QuestionSubType.Proteine: unit = "g";    realValue = food.Proteins;      break;
            case QuestionSubType.Glucide:  unit = "g";    realValue = food.Carbohydrates; break;
            case QuestionSubType.Lipide:   unit = "g";    realValue = food.Lipids;        break;
            case QuestionSubType.Fibres:   unit = "g";    realValue = food.Fibers;        break;
            default: Debug.LogWarning("Sous-type non géré"); break;
        }

        foodName.text = food.Name;

        panel.SetActive(true);

        // Reset état visuel
        canvasGroup.alpha = 0f;
        panel.transform.localScale = Vector3.zero;

        // Séquence principale stockée
        showSeq = DOTween.Sequence().SetTarget(this); // target = ce composant

        // Fade + scale-in
        showSeq.Append(canvasGroup.DOFade(1f, 0.3f).SetTarget(canvasGroup));
        showSeq.Join(panel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetTarget(panel.transform));

        // Remplit les valeurs et lance les anims secondaires
        AnimateFillAndValues(realValue, guessValue, unit);
    }

    private void AnimateFillAndValues(float realValue, float guessValue, string unit)
    {
        // --- Fill sprite ---
        if (fillSprite != null)
        {
            // Tuer toute anim existante sur ce target
            fillSprite.DOKill();
            fillSprite.fillAmount = 0f;

            // Joindre à la séquence principale
            showSeq.Join(
                fillSprite
                    .DOFillAmount(1f, 1f)
                    .SetEase(Ease.OutCubic)
                    .SetTarget(fillSprite)
            );
        }

        // --- Textes ---
        // Kill & reset du texte réel
        realValueText.DOKill();
        realValueText.text = base.FormatValue(0f, unit);

        // Le guess est affiché immédiatement
        playerGuessText.DOKill();
        playerGuessText.text = base.FormatValue(guessValue, unit);

        // Révélation de la vraie valeur après un délai
        showSeq.AppendInterval(0.5f);
        showSeq.AppendCallback(() =>
        {
            // Kill l’éventuel reveal précédent
            revealTweener?.Kill();

            revealTweener = DOTween
                .To(() => 0f, x => { realValueText.text = base.FormatValue(x, unit); }, realValue, 1f)
                .SetEase(Ease.OutCubic)
                .SetTarget(realValueText);
        });
    }

    private string GetLabelFromSubType(QuestionSubType subType)
    {
        return subType switch
        {
            QuestionSubType.Proteine => "Protéines",
            QuestionSubType.Glucide  => "Glucides",
            QuestionSubType.Lipide   => "Lipides",
            QuestionSubType.Fibres   => "Fibres",
            _ => "Calories"
        };
    }

    private void KillAllTweens()
    {
        // Tue séquence principale + reveal
        showSeq?.Kill();
        revealTweener?.Kill();

        // Par sûreté, tue les tweens ciblant ces objets
        canvasGroup.DOKill();
        if (panel) panel.transform.DOKill();
        if (fillSprite) fillSprite.DOKill();
        if (realValueText) realValueText.DOKill();
        if (playerGuessText) playerGuessText.DOKill();
    }
}