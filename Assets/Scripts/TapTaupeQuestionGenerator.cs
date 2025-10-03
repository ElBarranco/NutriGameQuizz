using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TapTaupeQuestionGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int itemCountRange = new Vector2Int(8, 12);

    [Header("Seuils nutritionnels")]
    [SerializeField] private float kcalThreshold_LightFood = 100f;

    [Header("Sous-type TapTaupe à générer")]
    [SerializeField] private TapTaupeSubType tapTaupeSubType = TapTaupeSubType.LightFood;

    public QuestionData Generate(List<FoodData> foods, DifficultyLevel currentDifficulty)
    {
        System.Random rng = new System.Random();

        // 1) Filtrer les aliments selon le sous-type sélectionné
        List<FoodData> validFoods = foods.Where(f => MatchesSubType(f, tapTaupeSubType)).ToList();
        List<FoodData> intrusFoods = foods.Where(f => !MatchesSubType(f, tapTaupeSubType)).ToList();

        // 2) Sélectionner un nombre aléatoire total d’aliments
        int itemCount = UnityEngine.Random.Range(itemCountRange.x, itemCountRange.y + 1);

        // 3) Construire un pool d’aliments mélangés
        List<FoodData> pool = validFoods.Concat(intrusFoods)
                                        .OrderBy(_ => rng.Next())
                                        .Take(itemCount)
                                        .ToList();

        // 4) Encoder les bonnes réponses
        List<int> encoded = new List<int>();
        List<PortionSelection> portions = new List<PortionSelection>();

        foreach (FoodData f in pool)
        {
            bool isValid = MatchesSubType(f, tapTaupeSubType);
            encoded.Add(isValid ? 1 : 0);

            portions.Add(new PortionSelection
            {
                Type = FoodPortionType.Simple,
                Grams = 100f,
                Value = f.Calories
            });
        }

        // 5) Retourner QuestionData
        return new QuestionData
        {
            Type = QuestionType.TapTaupe,
            TapTaupeSubType = tapTaupeSubType,
            Aliments = pool,
            PortionSelections = portions,
            ValeursComparees = encoded.Select(e => (float)e).ToList(),
            Solutions = encoded
        };
    }

    private bool MatchesSubType(FoodData f, TapTaupeSubType subType)
    {
        switch (subType)
        {
            case TapTaupeSubType.LightFood:
                return f.Calories < kcalThreshold_LightFood;

            default:
                Debug.LogWarning($"TapTaupeSubType non géré : {subType}");
                return false;
        }
    }
}