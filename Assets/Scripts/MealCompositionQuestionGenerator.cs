using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MealCompositionQuestionGenerator : QuestionGenerator
{
    [Header("Params")]
    [SerializeField] private int mealFoodsCount = 6;
    [SerializeField] private Vector2Int pickedRange = new Vector2Int(2, 4);

    [Header("Sous-type")]
    [SerializeField] private bool randomSubType = true;
    [SerializeField] private QuestionSubType forcedSubType = QuestionSubType.Calorie; 

    [Header("Robustesse")]
    [SerializeField] private int maxUniqueAttempts = 30;     
    [SerializeField] private int targetSnap = 25;            // ✅ maintenant en int
    [SerializeField] private float uniquenessTolerance = 5f; 

    public QuestionData Generate(List<FoodData> foodList, Func<FoodData, PortionSelection> portionResolver)
    {
        // 0) Sous-type (random ou forcé)
        QuestionSubType subType = randomSubType ? GetRandomMealSubType() : forcedSubType;

        // 1) Sélectionner des aliments distincts
        List<FoodData> foods = base.PickDistinctFoods(foodList, mealFoodsCount);

        // 2) Pré-calcul des valeurs via le resolver
        List<(FoodData food, PortionSelection sel, float value)> candidates =
            new List<(FoodData, PortionSelection, float)>();

        foreach (FoodData f in foods)
        {
            try
            {
                PortionSelection sel = portionResolver != null ? portionResolver.Invoke(f) : default;
                float v = sel.Value;
                if (float.IsNaN(v) || float.IsInfinity(v) || v <= 0f) continue;

                candidates.Add((f, sel, v));
            }
            catch
            {
                // Ignore si problème
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("[MealComposition] Aucun candidat valide.");
            return null;
        }

        // 3) Tentatives pour trouver une combinaison unique
        List<(FoodData food, PortionSelection sel, float value)> picked = null;
        List<int> pickedIndices = null;
        float snappedTarget = 0f;

        for (int attempt = 0; attempt < maxUniqueAttempts; attempt++)
        {
            int minPick = Mathf.Clamp(pickedRange.x, 1, candidates.Count);
            int maxPick = Mathf.Clamp(pickedRange.y, minPick, candidates.Count);
            int pickCount = Random.Range(minPick, maxPick + 1);

            HashSet<int> chosen = base.PickUniqueIndices(pickCount, candidates.Count);

            List<(FoodData food, PortionSelection sel, float value)> chosenSet =
                candidates.Where((c, idx) => chosen.Contains(idx)).ToList();

            float targetValue = chosenSet.Sum(c => c.value);
            snappedTarget = base.Snap(targetValue, targetSnap); // ✅ int utilisé ici

            List<float> itemValues = candidates.Select(c => c.value).ToList();

            if (base.IsUniqueSubsetSum(itemValues, chosen, snappedTarget, uniquenessTolerance))
            {
                picked = candidates;
                pickedIndices = new List<int>(chosen);
                break;
            }
        }

        if (picked == null)
        {
            Debug.LogWarning("[MealComposition] Unicité non garantie après maxUniqueAttempts.");
            picked = candidates;
            pickedIndices = new List<int>();
        }

        // 4) Construction QuestionData
        return new QuestionData
        {
            Type = QuestionType.MealComposition,
            SousType = subType,
            Aliments = picked.Select(c => c.food).ToList(),
            PortionSelections = picked.Select(c => c.sel).ToList(),
            ValeursComparees = new List<float> { snappedTarget },
            IndexBonneRéponse = 0,
            Solutions = pickedIndices,
            DeltaTolerance = targetSnap // ✅ int stocké ici
        };
    }

    private static QuestionSubType GetRandomMealSubType()
    {
        int r = Random.Range(0, 3); // 0..2
        switch (r)
        {
            case 0: return QuestionSubType.Calorie;
            case 1: return QuestionSubType.Proteine;
            default: return QuestionSubType.Glucide;
        }
    }
}