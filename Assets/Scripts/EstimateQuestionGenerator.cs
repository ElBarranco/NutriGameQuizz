using System.Collections.Generic;
using UnityEngine;

public class EstimateQuestionGenerator : QuestionGenerator
{
    public QuestionData Generate(List<FoodData> foodList, DifficultyLevel currentDifficulty)
    {
        FoodData f = foodList[Random.Range(0, foodList.Count)];
        QuestionSubType subType = GetRandomQuestionSubTypeWithDropRates(currentDifficulty);


        PortionSelection sel = ResolvePortionSafe(f, subType);

        float grams = PortionCalculator.ToGrams(sel, f);

        float value = PortionCalculator.ComputeValue(f, grams, subType);

        return new QuestionData
        {
            Type = QuestionType.EstimateCalories,
            SousType = subType,
            Aliments = new List<FoodData> { f },
            PortionSelections = new List<PortionSelection> { sel },
            ValeursComparees = new List<float> { value },
            IndexBonneRéponse = 0
        };
    }

    private QuestionSubType GetRandomQuestionSubTypeWithDropRates(DifficultyLevel currentDifficulty)
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                return QuestionSubType.Calorie;

            case DifficultyLevel.Medium:
                return GetRandomSubType(QuestionSubType.Calorie, QuestionSubType.Proteine, QuestionSubType.Lipide, QuestionSubType.Glucide);

            case DifficultyLevel.Hard:
                return GetRandomSubType(QuestionSubType.Calorie, QuestionSubType.Proteine, QuestionSubType.Lipide, QuestionSubType.Glucide);

            default:
                return QuestionSubType.Calorie;
        }
    }

}