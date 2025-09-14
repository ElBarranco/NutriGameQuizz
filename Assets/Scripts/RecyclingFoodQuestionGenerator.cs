using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecyclingFoodQuestionGenerator : MonoBehaviour
{
    [Header("Nombre d’aliments à générer (min / max)")]
    [SerializeField] private Vector2Int itemCountRange = new Vector2Int(8, 12);

    public QuestionData Generate(List<FoodData> foods, DifficultyLevel currentDifficulty)
    {
        System.Random rng = new System.Random();

        // 1) Tirer un sous-type aléatoire
        QuestionSubType subType = GetRandomSubType();

        // 2) Séparer les aliments selon leur catégorie principale
        List<FoodData> foodsValides = foods.Where(f => MatchesSubType(f, subType)).ToList();
        List<FoodData> foodsIntrus = foods.Where(f => !MatchesSubType(f, subType)).ToList();

        // 3) Nombre d’aliments à générer
        int itemCount = UnityEngine.Random.Range(itemCountRange.x, itemCountRange.y + 1);

        // 4) Constituer un pool équilibré (valide + intrus), mélangé
        List<FoodData> pool = foodsIntrus.Concat(foodsValides)
                                         .OrderBy(_ => rng.Next())
                                         .Take(itemCount)
                                         .ToList();

        // 5) Encoder les réponses
        List<int> encoded = new List<int>();
        List<PortionSelection> portions = new List<PortionSelection>();

        foreach (FoodData f in pool)
        {
            encoded.Add(MatchesSubType(f, subType) ? 1 : 0);

            portions.Add(new PortionSelection
            {
                Type = FoodPortionType.Simple,
                Grams = 100f,
                Value = 100f
            });
        }

        // 6) Retourner QuestionData
        return new QuestionData
        {
            Type = QuestionType.Recycling,
            SousType = subType,
            Aliments = pool,
            PortionSelections = portions,
            ValeursComparees = encoded.Select(e => (float)e).ToList(),
            Solutions = encoded
        };
    }

    /// <summary>
    /// Vérifie si l’aliment correspond au sous-type demandé
    /// (basé sur MainCategory calculé au parsing).
    /// </summary>
    private bool MatchesSubType(FoodData f, QuestionSubType subType)
    {
        switch (subType)
        {
            case QuestionSubType.Proteine: return f.MainCategory == FoodCategory.Proteine;
            case QuestionSubType.Glucide: return f.MainCategory == FoodCategory.Glucide;
            case QuestionSubType.Lipide: return f.MainCategory == FoodCategory.Lipide;
            default: return false;
        }
    }

    private QuestionSubType GetRandomSubType()
    {
        QuestionSubType[] values = {
            QuestionSubType.Proteine,
            QuestionSubType.Glucide,
            QuestionSubType.Lipide
        };
        return values[UnityEngine.Random.Range(0, values.Length)];
    }
}