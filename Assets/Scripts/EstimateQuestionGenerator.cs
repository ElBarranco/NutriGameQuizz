using System.Collections.Generic;
using UnityEngine;

public class EstimateQuestionGenerator : QuestionGenerator
{
    public QuestionData Generate(List<FoodData> foodList, DifficultyLevel currentDifficulty)
    {
        FoodData f = foodList[Random.Range(0, foodList.Count)];
        QuestionSubType subType = base.GetRandomQuestionSubTypeWithDropRates(currentDifficulty);


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

}