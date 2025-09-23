using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MoreInfoSugarPanelUI : MoreInfoPanelBase
{
    [SerializeField] private TextMeshProUGUI foodName;
    [SerializeField] private TextMeshProUGUI realValueText;
    [SerializeField] private TextMeshProUGUI playerGuessText;

    [SerializeField] private Image fillSprite;
    [SerializeField] private Image foodImage;

    public void Show(QuestionData data, float playerGuess)
    {
        // ✅ Récupération des infos depuis QuestionData
        FoodData food = data.Aliments[0];
        PortionSelection portion = data.PortionSelections[0];

        float realValue = data.ValeursComparees[0]; // bonne réponse (nb de carrés)
        float guessValue = playerGuess;

        // ✅ Affichage aliment
        foodImage.sprite = SpriteLoader.LoadFoodSprite(food.Name);
        foodName.text = PortionTextFormatter.ToDisplayWithFood(food, portion);

        // ✅ Activation panel
        panel.SetActive(true);
        canvasGroup.alpha = 1f;
        panel.transform.localScale = Vector3.one;

        // ✅ Affiche les valeurs directement
        realValueText.text = $"x{Mathf.RoundToInt(realValue)}";
        playerGuessText.text = $"x{Mathf.RoundToInt(guessValue)}";
    }
}