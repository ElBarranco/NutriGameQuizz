using System.Collections.Generic;
using UnityEngine;
using TMPro;
using NaughtyAttributes;

public class QuestionNutritionTableUI : BaseQuestionUI
{
    [Header("Références UI")]
    [SerializeField] private List<GameObject> foodSlots; // 3 max
    [SerializeField] private FoodSelectableSubtractionUI foodButtonPrefab;
    [SerializeField] private NutritionTableUI nutritionTableUI;


    [Header("Debug")]
    [SerializeField, ReadOnly] private int currentIndex = -1;

    private List<FoodSelectableSubtractionUI> spawnedItems = new();

    private void OnFoodClicked(int index, bool _)
    {
        currentIndex = index;
        guess = index;

        GameManager.Instance.OnQuestionAnswered(guess);
        base.Close();
    }

    public void Init(QuestionData q)
    {
        question = q;
        currentIndex = -1;

        // Activer uniquement les slots nécessaires
        for (int i = 0; i < foodSlots.Count; i++)
        {
            bool active = i < q.Aliments.Count;
            foodSlots[i].SetActive(active);

            if (active)
            {
                FoodData f = q.Aliments[i];
                FoodSelectableSubtractionUI item = Instantiate(foodButtonPrefab, foodSlots[i].transform, false);
                item.name = $"NUTRITABLE_{f.Name}_{i}";

                // PortionSelection inutile ici → on passe null
                item.Init(f, QuestionSubType.Calorie, i, OnFoodClicked);

                spawnedItems.Add(item);
            }
        }


        nutritionTableUI.SetValues(q.ValeursComparees);

        QuestionValidateButtonUI.Instance.DisableButton();
    }
}