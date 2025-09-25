using System.Collections.Generic;
using UnityEngine;

public class SugarQuestionGenerator : QuestionGenerator
{
    [SerializeField] private float deltaTolerance = 0.2f; // 20%
    

    public QuestionData Generate(List<FoodData> foodList, DifficultyLevel currentDifficulty)
    {
        // 1. Filtrage des aliments pertinents
        List<FoodData> validFoods = FoodDatabase.Instance.GetFoodsWithSugar();

        // 2. Tirage d'un aliment
        FoodData f = validFoods[Random.Range(0, validFoods.Count)];

        // 3. Portion
        PortionSelection sel = ResolvePortionSafe(f, QuestionSubType.Sugar);

        // 4. Poids en grammes
        float grams = PortionCalculator.ToGrams(sel, f);

        // 5. Calcul sucre ajouté (ou glucides si tu veux)
        float sugarGrams = PortionCalculator.ComputeValue(f, grams, QuestionSubType.Sugar);

        // 6. Conversion en carrés de sucre
        int sucreCubes = Mathf.RoundToInt(sugarGrams / 4f);

        // 7. Calcul delta (20% arrondi à int)
        int delta = Mathf.Max(1, Mathf.RoundToInt(sucreCubes * deltaTolerance));

        return new QuestionData
        {
            Type = QuestionType.Sugar,
            SousType = QuestionSubType.Sugar,
            Aliments = new List<FoodData> { f },
            PortionSelections = new List<PortionSelection> { sel },
            ValeursComparees = new List<float> { sucreCubes },
            IndexBonneRéponse = sucreCubes,
            DeltaTolerance = delta
        };
    }
}