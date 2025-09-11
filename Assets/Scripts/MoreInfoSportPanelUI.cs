using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class MoreInfoSportPanelUI : MoreInfoPanelBase
{
    [SerializeField] private TextMeshProUGUI sportAName;
    [SerializeField] private TextMeshProUGUI sportAValue;
    [SerializeField] private TextMeshProUGUI sportAValuePerHour;
    [SerializeField] private TextMeshProUGUI sportBName;
    [SerializeField] private TextMeshProUGUI sportBValue;
    [SerializeField] private TextMeshProUGUI sportBValuePerHour;
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
    [SerializeField] private GameObject CheckGoodAnswerA;
    [SerializeField] private GameObject CheckGoodAnswerB;
    [SerializeField] private GameObject HoverA;
    [SerializeField] private GameObject HoverB;

    [Header("Fonds (couleur uniquement)")]
    [SerializeField] private Image bgA;
    [SerializeField] private Image bgB;
    [SerializeField] private Color bgDefaultColor = Color.white;

    public void Show(int targetCalorie, FoodData aliment, SportData a, SportData b, int indexBonneReponse, int userAnswerIndex = -1)
    {
        sportAName.text = a.Name;
        sportBName.text = b.Name;

        sportADuration.text = TextFormatter.ToDisplayDuration(a.Duration) + " de " + a.Name;
        sportBDuration.text = TextFormatter.ToDisplayDuration(b.Duration) + " de " + b.Name;

        sportAValue.text = a.Calories + " kcal";
        sportBValue.text = b.Calories + " kcal";

        sportAValuePerHour.text = a.CaloriesPerHour + " kcal/h";
        sportBValuePerHour.text = b.CaloriesPerHour + " kcal/h";

        imageA.sprite = SpriteLoader.LoadSportSprite(a.Name);
        imageB.sprite = SpriteLoader.LoadSportSprite(b.Name);

        UpdateAnswerVisuals(indexBonneReponse, userAnswerIndex);

        panel.SetActive(true);
        canvasGroup.alpha = 0f;
        panel.transform.localScale = Vector3.zero;
        DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, 0.3f))
            .Join(panel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
    }

    private void UpdateAnswerVisuals(int indexBonneReponse, int userAnswerIndex)
    {
        sportAHighlight.color = defaultColor;
        sportBHighlight.color = defaultColor;



        bgA.color = bgDefaultColor;
        bgB.color = bgDefaultColor;

        if (userAnswerIndex < 0)
        {
            if (indexBonneReponse == 0) bgA.color = sportAHighlight.color = correctColor;
            else bgB.color = sportBHighlight.color = correctColor;
            return;
        }

        bool userPickedA = userAnswerIndex == 0;
        bool aIsCorrect = indexBonneReponse == 0;
        bool bIsCorrect = indexBonneReponse == 1;


        HoverA.SetActive(userPickedA);
        HoverB.SetActive(!userPickedA);

        if (aIsCorrect) bgA.color = correctColor;
        if (bIsCorrect) bgB.color = correctColor;

        if (userPickedA)
        {
            sportAHighlight.color = aIsCorrect ? correctColor : wrongColor;
            sportBHighlight.color = bIsCorrect ? correctColor : defaultColor;
            bgA.color = aIsCorrect ? correctColor : wrongColor;

        }
        else
        {
            sportBHighlight.color = bIsCorrect ? correctColor : wrongColor;
            sportAHighlight.color = aIsCorrect ? correctColor : defaultColor;
            bgB.color = bIsCorrect ? correctColor : wrongColor;

        }

        CheckGoodAnswerA.SetActive(userPickedA && aIsCorrect);
        CheckGoodAnswerB.SetActive(!userPickedA && bIsCorrect);
    }
}