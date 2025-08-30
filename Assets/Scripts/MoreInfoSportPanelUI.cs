using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class MoreInfoSportPanelUI : MoreInfoPanelBase
{
    [SerializeField] private TextMeshProUGUI sportAName;
    [SerializeField] private TextMeshProUGUI sportAValue;          // kcal POUR LA DURÉE proposée
    [SerializeField] private TextMeshProUGUI sportAValuePerHour;   // kcal / heure
    [SerializeField] private TextMeshProUGUI sportBName;
    [SerializeField] private TextMeshProUGUI sportBValue;          // kcal POUR LA DURÉE proposée
    [SerializeField] private TextMeshProUGUI sportBValuePerHour;   // kcal / heure

    [SerializeField] private TextMeshProUGUI sportADuration;
    [SerializeField] private TextMeshProUGUI sportBDuration;

    [SerializeField] private Image imageA;
    [SerializeField] private Image imageB;

    [Header("Highlights")]
    [SerializeField] private Image sportAHighlight;
    [SerializeField] private Image sportBHighlight;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color wrongColor = new Color(1f, 0.25f, 0.25f);

    [Header("Icônes résultat")]
    [SerializeField] private Image resultIconA;
    [SerializeField] private Image resultIconB;
    [SerializeField] private Sprite iconCorrect;
    [SerializeField] private Sprite iconWrong;

    [Header("Fonds (couleur uniquement)")]
    [SerializeField] private Image bgA;
    [SerializeField] private Image bgB;
    [SerializeField] private Color bgDefaultColor = Color.white;

    /// <summary>
    /// Affiche le panneau d’info pour la question sport.
    /// </summary>
    public void Show(int targetCalorie, FoodData Aliment, SportData a, SportData b, int indexBonneReponse, int userAnswerIndex = -1)
    {
        // Noms
        sportAName.text = a.Name;
        sportBName.text = b.Name;

        // Durées
        if (sportADuration) sportADuration.text = a.Duration + " min";
        if (sportBDuration) sportBDuration.text = b.Duration + " min";

        // kcal pour la durée proposée (déjà préparés dans SportData)
        sportAValue.text = a.Calories + " kcal";
        sportBValue.text = b.Calories + " kcal";

        // kcal / heure
        sportAValuePerHour.text = a.CaloriesPerHour + " kcal/h";
        sportBValuePerHour.text = b.CaloriesPerHour + " kcal/h";

        // Images
        imageA.sprite = SpriteLoader.LoadSportSprite(a.Name);
        imageB.sprite = SpriteLoader.LoadSportSprite(b.Name);

        // Visuels
        UpdateAnswerVisuals(indexBonneReponse, userAnswerIndex);

        // Anim
        panel.SetActive(true);
        canvasGroup.alpha = 0f;
        panel.transform.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(1f, 0.3f));
        sequence.Join(panel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
    }

    private void UpdateAnswerVisuals(int indexBonneReponse, int userAnswerIndex)
    {
        if (sportAHighlight) sportAHighlight.color = defaultColor;
        if (sportBHighlight) sportBHighlight.color = defaultColor;

        if (resultIconA) { resultIconA.enabled = false; resultIconA.sprite = null; }
        if (resultIconB) { resultIconB.enabled = false; resultIconB.sprite = null; }

        if (bgA) bgA.color = bgDefaultColor;
        if (bgB) bgB.color = bgDefaultColor;

        if (userAnswerIndex < 0)
        {
            if (indexBonneReponse == 0)
            {
                if (sportAHighlight) sportAHighlight.color = correctColor;
                if (bgA) bgA.color = correctColor;
            }
            else
            {
                if (sportBHighlight) sportBHighlight.color = correctColor;
                if (bgB) bgB.color = correctColor;
            }
            return;
        }

        bool userPickedA = userAnswerIndex == 0;
        bool aIsCorrect = indexBonneReponse == 0;
        bool bIsCorrect = indexBonneReponse == 1;

        if (aIsCorrect && bgA) bgA.color = correctColor;
        if (bIsCorrect && bgB) bgB.color = correctColor;

        if (!userPickedA && bgB && !bIsCorrect) bgB.color = wrongColor;
        if (userPickedA && bgA && !aIsCorrect) bgA.color = wrongColor;

        if (userPickedA)
        {
            if (sportAHighlight) sportAHighlight.color = aIsCorrect ? correctColor : wrongColor;
            if (sportBHighlight) sportBHighlight.color = bIsCorrect ? correctColor : defaultColor;
        }
        else
        {
            if (sportBHighlight) sportBHighlight.color = bIsCorrect ? correctColor : wrongColor;
            if (sportAHighlight) sportAHighlight.color = aIsCorrect ? correctColor : defaultColor;
        }

        if (userPickedA && resultIconA)
        {
            resultIconA.enabled = true;
            resultIconA.sprite = aIsCorrect ? iconCorrect : iconWrong;
        }
        else if (!userPickedA && resultIconB)
        {
            resultIconB.enabled = true;
            resultIconB.sprite = bIsCorrect ? iconCorrect : iconWrong;
        }
    }
}