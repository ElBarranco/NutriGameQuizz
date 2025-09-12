using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecyclingFoodQuestionGenerator : MonoBehaviour
{
    [Header("Nombre d’aliments à générer (min / max)")]
    [SerializeField] private Vector2Int itemCountRange = new Vector2Int(8, 12);

    // Seuils par sous-type
    private const float PROTEIN_THRESHOLD = 2f;
    private const float GLUCIDE_THRESHOLD = 5f;
    private const float LIPID_THRESHOLD = 3f;

    public QuestionData Generate(List<FoodData> foods, DifficultyLevel currentDifficulty)
    {
        System.Random rng = new System.Random();

        // 1) Tirer un sous-type aléatoire (Protéine, Glucide ou Lipide)
        QuestionSubType subType = GetRandomSubType();

        // 2) Split selon le sous-type choisi
        List<FoodData> foodsIntrus = foods.Where(f => !IsValid(f, subType)).ToList();
        List<FoodData> foodsValides = foods.Where(f => IsValid(f, subType)).ToList();

        // 3) Nombre aléatoire d’aliments
        int itemCount = UnityEngine.Random.Range(itemCountRange.x, itemCountRange.y + 1);

        // 4) Mélanger et prendre itemCount aliments
        List<FoodData> pool = foodsIntrus.Concat(foodsValides)
                                         .OrderBy(_ => rng.Next())
                                         .Take(itemCount)
                                         .ToList();

        // 5) Encoder en 0/1
        List<int> encoded = new List<int>();
        List<PortionSelection> portions = new List<PortionSelection>();

        foreach (FoodData f in pool)
        {
            encoded.Add(IsValid(f, subType) ? 1 : 0);

            portions.Add(new PortionSelection
            {
                Type = FoodPortionType.Simple,
                Grams = 100f,
                Value = 100f
            });
        }

        // 6) Retour QuestionData
        return new QuestionData
        {
            Type = QuestionType.Recycling,
            SousType = subType,
            Aliments = pool,
            PortionSelections = portions,
            ValeursComparees = encoded.Select(e => (float)e).ToList(),
            Solutions = encoded // ✅ stock direct en liste 0/1
        };
    }

    private bool IsValid(FoodData f, QuestionSubType subType)
    {
        switch (subType)
        {
            case QuestionSubType.Proteine: return f.Proteins >= PROTEIN_THRESHOLD;
            case QuestionSubType.Glucide: return f.Carbohydrates >= GLUCIDE_THRESHOLD;
            case QuestionSubType.Lipide: return f.Lipids >= LIPID_THRESHOLD;
            default: return false;
        }
    }

    private QuestionSubType GetRandomSubType()
    {
        Array values = new[] {
            QuestionSubType.Proteine,
            QuestionSubType.Glucide,
            QuestionSubType.Lipide
        };
        return (QuestionSubType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }
}