using UnityEngine;
using System.Collections.Generic;

public class MoreInfoNutritionTablePanelUI : MoreInfoPanelBase
{
    [Header("UI Réponse Joueur")]
    [SerializeField] private Transform playerItemParent;

    [Header("UI Bonne Réponse")]
    [SerializeField] private Transform correctItemParent;
    [SerializeField] private Transform correctTableParent;
    [SerializeField] private GameObject gameObjectCorrect;

    [Header("Prefabs")]
    [SerializeField] private FoodItemUI foodItemPrefab;
    [SerializeField] private NutritionTableUI nutritionTablePrefab;

    public void Show(QuestionData q, int userAnswer)
    {
        int playerIndex  = userAnswer;
        int correctIndex = q.IndexBonneRéponse;
        bool isCorrect = (userAnswer == q.IndexBonneRéponse);

        FoodItemResultState playerState = isCorrect
            ? FoodItemResultState.SelectedCorrect
            : FoodItemResultState.SelectedWrong;

        FoodItemResultState correctState = FoodItemResultState.MissedCorrect;

        // 🧍 Réponse du joueur
        FoodItemUI playerItem = Instantiate(foodItemPrefab, playerItemParent);
        playerItem.Init(q.Aliments[playerIndex], null, playerState, QuestionSubType.Calorie);

        // ✅ Bonne réponse (si différente du joueur)
        if (!isCorrect)
        {
            FoodItemUI correctItem = Instantiate(foodItemPrefab, correctItemParent);
            correctItem.Init(q.Aliments[correctIndex], null, correctState, QuestionSubType.Calorie);
        }
        gameObjectCorrect.SetActive(!isCorrect);

        nutritionTablePrefab.SetValues(new List<float>
        {
            q.Aliments[correctIndex].Calories,
            q.Aliments[correctIndex].Proteins,
            q.Aliments[correctIndex].Lipids,
            q.Aliments[correctIndex].Carbohydrates
        });

        base.IntroAnim();
    }
}