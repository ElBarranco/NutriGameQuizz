using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NutritionTableQuestionGenerator : QuestionGenerator
{
    public QuestionData Generate(List<FoodData> foods, DifficultyLevel currentDifficulty)
    {
        System.Random rng = new System.Random();

        // 1. Sélectionner 3 aliments distincts
        List<FoodData> pickedFoods = foods.OrderBy(_ => rng.Next()).Take(3).ToList();

        // 2. Choisir aléatoirement l'aliment de référence
        int indexBonneReponse = rng.Next(0, pickedFoods.Count);
        FoodData referenceFood = pickedFoods[indexBonneReponse];

        // 3. Encoder les valeurs nutritionnelles de référence dans ValeursComparees
        List<float> valeursComparees = new List<float>
        {
            referenceFood.Calories,
            referenceFood.Proteins,
            referenceFood.Lipids,
            referenceFood.Carbohydrates
        };

        // 4. Construction de la question
        return new QuestionData
        {
            Type = QuestionType.NutritionTable,
            SousType = QuestionSubType.Calorie,
            Aliments = pickedFoods,
            PortionSelections = null,
            ValeursComparees = valeursComparees, // [cal, prot, lip, gluc]
            IndexBonneRéponse = indexBonneReponse,
            Solutions = new List<int> { indexBonneReponse + 1 } // +1 si tu veux encoder comme avant
        };
    }
}