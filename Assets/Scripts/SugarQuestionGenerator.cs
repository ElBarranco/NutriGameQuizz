using System.Collections.Generic;
using UnityEngine;

public class SugarQuestionGenerator : QuestionGenerator
{
    [SerializeField] private int maxCalories = 800; // 🔧 limite arbitraire

    public QuestionData Generate(List<FoodData> foodList, DifficultyLevel currentDifficulty)
    {
        // 1. Filtrage des aliments pertinents
        List<FoodData> validFoods = FoodDatabase.Instance.GetFoodsWithSugar();

        // 2. Tirage d'un aliment
        FoodData f = validFoods[Random.Range(0, validFoods.Count)];

        // 3. Portion
        PortionSelection sel = ResolvePortionSafe(f, QuestionSubType.Glucide);

        // 4. Poids en grammes
        float grams = PortionCalculator.ToGrams(sel, f);

        // 5. Calcul glucides
        float glucides = PortionCalculator.ComputeValue(f, grams, QuestionSubType.Glucide);

        // 6. Conversion en carrés de sucre (arrondi)
        float sucreCubes = glucides / 4f;

        return new QuestionData
        {
            Type = QuestionType.Sugar,
            SousType = QuestionSubType.Glucide,
            Aliments = new List<FoodData> { f },
            PortionSelections = new List<PortionSelection> { sel },
            ValeursComparees = new List<float> { sucreCubes },
            IndexBonneRéponse = 0
        };
    }
}