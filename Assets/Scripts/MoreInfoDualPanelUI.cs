using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class MoreInfoDualPanelUI : MoreInfoPanelBase
{
    [SerializeField] private TextMeshProUGUI foodAName;
    [SerializeField] private TextMeshProUGUI foodAValue;          // valeur PAR PORTION
    [SerializeField] private TextMeshProUGUI foodAValuePer100;    // valeur POUR 100 g
    [SerializeField] private TextMeshProUGUI foodBName;
    [SerializeField] private TextMeshProUGUI foodBValue;          // valeur PAR PORTION
    [SerializeField] private TextMeshProUGUI foodBValuePer100;    // valeur POUR 100 g


    [SerializeField] private Image imageA;
    [SerializeField] private Image imageB;

    [Header("Highlights")]
    [SerializeField] private Image foodAHighlight;
    [SerializeField] private Image foodBHighlight;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color wrongColor = new Color(1f, 0.25f, 0.25f);

    [Header("Icônes résultat")]
    [SerializeField] private Image resultIconA;            // icône côté A
    [SerializeField] private Image resultIconB;            // icône côté B
    [SerializeField] private Sprite iconCorrect;           // ✅
    [SerializeField] private Sprite iconWrong;             // ❌

    [Header("Fonds (couleur uniquement)")]
    [SerializeField] private Image bgA;
    [SerializeField] private Image bgB;
    [SerializeField] private Color bgDefaultColor = Color.white;

    // ======= surcharge avec portions =======
    public void Show(FoodData a, FoodData b, PortionSelection portionA, PortionSelection portionB,
                     int indexBonneReponse, QuestionSubType sousType, int userAnswerIndex = -1)
    {
        // Valeurs pour 100 g depuis FoodData
        float per100A = GetValueBySubType(a, sousType);
        float per100B = GetValueBySubType(b, sousType);

        // Unité d'affichage
        string unit = TextFormatter.GetUnitForSubType(sousType);

        // Noms
        foodAName.text = PortionTextFormatter.ToDisplayWithFood(a, portionA);
        foodBName.text = PortionTextFormatter.ToDisplayWithFood(b, portionB);


        // Valeurs finales (PAR PORTION)
        foodAValue.text = base.FormatValue(portionA.Value, unit);
        foodBValue.text = base.FormatValue(portionB.Value, unit);

        // Valeurs POUR 100 g
        foodAValuePer100.text = $"{Mathf.RoundToInt(per100A)} {unit}";
        foodBValuePer100.text = $"{Mathf.RoundToInt(per100B)} {unit}";

        // Images
        imageA.sprite = SpriteLoader.LoadFoodSprite(a.Name);
        imageB.sprite = SpriteLoader.LoadFoodSprite(b.Name);

        // Visuels (highlights, icônes, fonds)
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
        // reset highlights
        if (foodAHighlight) foodAHighlight.color = defaultColor;
        if (foodBHighlight) foodBHighlight.color = defaultColor;

        // reset icônes
        if (resultIconA) { resultIconA.enabled = false; resultIconA.sprite = null; }
        if (resultIconB) { resultIconB.enabled = false; resultIconB.sprite = null; }

        // reset fonds (couleur)
        if (bgA) bgA.color = bgDefaultColor;
        if (bgB) bgB.color = bgDefaultColor;

        // Pas de réponse utilisateur -> highlight + BG sur le bon côté
        if (userAnswerIndex < 0)
        {
            if (indexBonneReponse == 0)
            {
                if (foodAHighlight) foodAHighlight.color = correctColor;
                if (bgA) bgA.color = correctColor;
            }
            else
            {
                if (foodBHighlight) foodBHighlight.color = correctColor;
                if (bgB) bgB.color = correctColor;
            }
            return;
        }

        bool userPickedA = userAnswerIndex == 0;
        bool aIsCorrect = indexBonneReponse == 0;
        bool bIsCorrect = indexBonneReponse == 1;

        // 1) Toujours colorer le BG du côté correct en vert
        if (aIsCorrect && bgA) bgA.color = correctColor;
        if (bIsCorrect && bgB) bgB.color = correctColor;

        // 2) Si l'utilisateur s'est trompé, colorer son choix en rouge (écrase la couleur du point 1 sur le côté choisi)
        bool isCorrectPick = userAnswerIndex == indexBonneReponse;
        if (!isCorrectPick)
        {
            if (userPickedA && bgA) bgA.color = wrongColor;
            if (!userPickedA && bgB) bgB.color = wrongColor;
        }

        // Highlights
        if (userPickedA)
        {
            if (foodAHighlight) foodAHighlight.color = aIsCorrect ? correctColor : wrongColor;
            if (foodBHighlight) foodBHighlight.color = bIsCorrect ? correctColor : defaultColor;
        }
        else
        {
            if (foodBHighlight) foodBHighlight.color = bIsCorrect ? correctColor : wrongColor;
            if (foodAHighlight) foodAHighlight.color = aIsCorrect ? correctColor : defaultColor;
        }

        // Icônes sur l’élément sélectionné
        if (userPickedA && resultIconA)
        {
            resultIconA.enabled = true;
            if (iconCorrect && iconWrong)
                resultIconA.sprite = aIsCorrect ? iconCorrect : iconWrong;
        }
        else if (!userPickedA && resultIconB)
        {
            resultIconB.enabled = true;
            if (iconCorrect && iconWrong)
                resultIconB.sprite = bIsCorrect ? iconCorrect : iconWrong;
        }
    }

    // ----- Helpers locaux -----

    private static float GetValueBySubType(FoodData food, QuestionSubType subType)
    {
        switch (subType)
        {
            case QuestionSubType.Proteine: return food.Proteins;
            case QuestionSubType.Glucide: return food.Carbohydrates;
            case QuestionSubType.Lipide: return food.Lipids;
            case QuestionSubType.Fibres: return food.Fibers;
            default: return food.Calories;
        }
    }
}