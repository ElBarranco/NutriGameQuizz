using UnityEngine;
using System.Collections.Generic;

public class MoreInfoNutritionTablePanelUI : MoreInfoPanelBase
{
    [Header("UI Réponse Joueur")]
    [SerializeField] private Transform playerItemParent;
    [SerializeField] private Transform playerTableParent;

    [Header("UI Bonne Réponse")]
    [SerializeField] private Transform correctItemParent;
    [SerializeField] private Transform correctTableParent;

    [Header("Prefabs")]
    [SerializeField] private FoodItemUI foodItemPrefab;
    [SerializeField] private NutritionTableUI nutritionTablePrefab;

    public void Show(QuestionData q, int userAnswer)
    {
        int playerIndex  = userAnswer;
        int correctIndex = q.IndexBonneRéponse;

        // 🧍 Réponse joueur
        FoodItemUI playerItem = Instantiate(foodItemPrefab, playerItemParent);
        playerItem.Init(q.Aliments[playerIndex], default, false, QuestionSubType.Calorie, true);

        NutritionTableUI playerTable = Instantiate(nutritionTablePrefab, playerTableParent);
        playerTable.SetValues(new List<float>
        {
            q.Aliments[playerIndex].Calories,
            q.Aliments[playerIndex].Proteins,
            q.Aliments[playerIndex].Lipids,
            q.Aliments[playerIndex].Carbohydrates
        });

        // ✅ Bonne réponse
        FoodItemUI correctItem = Instantiate(foodItemPrefab, correctItemParent);
        correctItem.Init(q.Aliments[correctIndex], default, false, QuestionSubType.Calorie, true);

        NutritionTableUI correctTable = Instantiate(nutritionTablePrefab, correctTableParent);
        correctTable.SetValues(new List<float>
        {
            q.Aliments[correctIndex].Calories,
            q.Aliments[correctIndex].Proteins,
            q.Aliments[correctIndex].Lipids,
            q.Aliments[correctIndex].Carbohydrates
        });

        base.IntroAnim();
    }
}