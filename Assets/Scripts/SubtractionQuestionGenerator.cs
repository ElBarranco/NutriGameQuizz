using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SubtractionQuestionGenerator : QuestionGenerator
{
    [Header("Nombre d’aliments selon la difficulté")]
    [SerializeField] private Vector2 easyFoodRange = new Vector2(3, 3);
    [SerializeField] private Vector2 mediumFoodRange = new Vector2(3, 4);
    [SerializeField] private Vector2 hardFoodRange = new Vector2(4, 5);

    [Header("Nombre d’intrus selon la difficulté")]
    [SerializeField] private Vector2 easyIntrusRange = new Vector2(1, 1);
    [SerializeField] private Vector2 mediumIntrusRange = new Vector2(1, 2);
    [SerializeField] private Vector2 hardIntrusRange = new Vector2(1, 3);

    public QuestionData Generate(List<FoodData> foods, DifficultyLevel currentDifficulty)
    {
        System.Random rng = new System.Random();

        // 1) Nombre d’aliments à afficher
        int itemCount = GetFoodCount(currentDifficulty);

        // 2) Mélange des aliments
        List<FoodData> pickedFoods = foods.OrderBy(_ => rng.Next()).Take(itemCount).ToList();

        // 3) Nombre d’intrus à inclure
        int intrusCount = GetIntrusCount(currentDifficulty, pickedFoods.Count);

        // 4) Sélection des intrus aléatoirement
        List<int> intrusIndexes = Enumerable.Range(0, pickedFoods.Count)
                                            .OrderBy(_ => rng.Next())
                                            .Take(intrusCount)
                                            .ToList();

        // 5) Portion et valeurs calculées
        List<PortionSelection> portions = new List<PortionSelection>();
        List<float> caloriesByFood = new List<float>();

        foreach (FoodData f in pickedFoods)
        {
            PortionSelection sel = ResolvePortionSafe(f, QuestionSubType.Calorie);
            float grams = PortionCalculator.ToGrams(sel, f);
            float value = PortionCalculator.ComputeValue(f, grams, QuestionSubType.Calorie);

            portions.Add(sel);
            caloriesByFood.Add(value);
        }

        // ✅ calories à retirer = somme des intrus
        float targetCalories = intrusIndexes.Sum(idx => caloriesByFood[idx]);

        // ⚡ On stocke uniquement la cible dans ValeursComparees
        List<float> valeursComparees = new List<float> { targetCalories };

        // 6) Solutions (index +1 pour éviter le zéro)
        List<int> solutions = intrusIndexes.Select(i => i + 1).ToList();

        // 7) Encodage sous forme concaténée
        string concat = string.Concat(solutions);
        int solutionEncoded = int.Parse(concat);

        // 8) Construction de la question
        return new QuestionData
        {
            Type = QuestionType.Subtraction,
            SousType = QuestionSubType.Calorie,
            Aliments = pickedFoods,
            PortionSelections = portions,
            ValeursComparees = valeursComparees, // [ targetCalories ]
            IndexBonneRéponse = solutionEncoded,
            Solutions = solutions
        };
    }

    private int GetFoodCount(DifficultyLevel difficulty)
    {
        Vector2 range = difficulty switch
        {
            DifficultyLevel.Easy => easyFoodRange,
            DifficultyLevel.Medium => mediumFoodRange,
            DifficultyLevel.Hard => hardFoodRange,
            _ => new Vector2(3, 4)
        };

        return UnityEngine.Random.Range((int)range.x, (int)range.y + 1);
    }

    private int GetIntrusCount(DifficultyLevel difficulty, int maxFoods)
    {
        Vector2 range = difficulty switch
        {
            DifficultyLevel.Easy => easyIntrusRange,
            DifficultyLevel.Medium => mediumIntrusRange,
            DifficultyLevel.Hard => hardIntrusRange,
            _ => new Vector2(1, 1)
        };

        int count = UnityEngine.Random.Range((int)range.x, (int)range.y + 1);
        return Mathf.Clamp(count, 1, maxFoods);
    }
}