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

    [Header("Robustesse")]
    [SerializeField] private int maxUniqueAttempts = 30;
    [SerializeField] private int targetSnap = 25;
    [SerializeField] private float uniquenessTolerance = 5f;


    // ← Signature conforme à la base (pas de DifficultyLevel en paramètre)
    public QuestionData Generate(
        List<FoodData> foodList,
        Func<FoodData, PortionSelection> portionResolver, 
        DifficultyLevel difficulty
    )
    {
        // 0) Sous-type selon la difficulté
        QuestionSubType subType = GetRandomQuestionSubTypeWithDropRates(difficulty);

        // 1) Piocher des aliments distincts
        List<FoodData> foods = base.PickDistinctFoods(foodList, mealFoodsCount);

        // 2) Pré-calcul des candidats via ResolvePortionSafe
        List<(FoodData food, PortionSelection sel, float value)> candidates =
            new List<(FoodData, PortionSelection, float)>();
        foreach (FoodData f in foods)
        {
            PortionSelection sel = base.ResolvePortionSafe(f, subType);
            float valeur = sel.Value;
            if (valeur <= 0f)
            {
                continue;
            }
            candidates.Add((food: f, sel: sel, value: valeur));
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("[MealComposition] Aucun candidat valide pour " + subType);
            return null;
        }

        // 3) Recherche d’une combinaison unique
        List<(FoodData food, PortionSelection sel, float value)> picked = null;
        List<int> pickedIndices = null;
        float snappedTarget = 0f;

        for (int attempt = 0; attempt < maxUniqueAttempts; attempt++)
        {
            int minPick = Mathf.Clamp(pickedRange.x, 1, candidates.Count);
            int maxPick = Mathf.Clamp(pickedRange.y, minPick, candidates.Count);
            int pickCount = Random.Range(minPick, maxPick + 1);

            HashSet<int> chosen = base.PickUniqueIndices(pickCount, candidates.Count);

            // Construire la sélection
            List<(FoodData food, PortionSelection sel, float value)> chosenItems =
                new List<(FoodData, PortionSelection, float)>();
            foreach (int idx in chosen)
            {
                chosenItems.Add(candidates[idx]);
            }

            // Calcul du total
            float total = 0f;
            foreach ((FoodData food, PortionSelection sel, float value) item in chosenItems)
            {
                total += item.value;
            }

            snappedTarget = base.Snap(total, targetSnap);

            // Préparer la liste des valeurs
            List<float> allValues = new List<float>();
            foreach ((FoodData food, PortionSelection sel, float value) item in candidates)
            {
                allValues.Add(item.value);
            }

            if (base.IsUniqueSubsetSum(allValues, chosen, snappedTarget, uniquenessTolerance))
            {
                picked = new List<(FoodData food, PortionSelection sel, float value)>(candidates);
                pickedIndices = new List<int>(chosen);
                break;
            }
        }

        if (picked == null)
        {
            Debug.LogWarning("[MealComposition] Unicité non garantie après " + maxUniqueAttempts + " essais.");
            picked = new List<(FoodData food, PortionSelection sel, float value)>(candidates);
            pickedIndices = new List<int>();
        }

        // 4) Construction de la QuestionData
        QuestionData question = new QuestionData
        {
            Type              = QuestionType.MealComposition,
            SousType          = subType,
            Aliments          = picked.Select(entry => entry.food).ToList(),
            PortionSelections = picked.Select(entry => entry.sel).ToList(),
            ValeursComparees  = new List<float> { snappedTarget },
            IndexBonneRéponse = 0,
            Solutions         = pickedIndices,
            DeltaTolerance    = targetSnap
        };

        return question;
    }
}