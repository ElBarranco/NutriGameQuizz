using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IntruderFoodQuestionGenerator : MonoBehaviour
{
    [Header("Paramètres")]
    [SerializeField, Range(4, 4)] private int itemCount = 4;

    [Header("Nombre d’intrus selon la difficulté")]
    [SerializeField] private Vector2 easyIntrusRange = new Vector2(1, 1);
    [SerializeField] private Vector2 mediumIntrusRange = new Vector2(1, 2);
    [SerializeField] private Vector2 hardIntrusRange = new Vector2(2, 3);

    public QuestionData Generate(List<FoodData> foods, DifficultyLevel currentDifficulty)
    {
        System.Random rng = new System.Random();

        // 1) Sélectionner 4 aliments distincts
        List<FoodData> pickedFoods = foods
            .OrderBy(_ => rng.Next())
            .Distinct()
            .Take(itemCount)
            .ToList();

        // 2) Déterminer le nombre d’intrus selon difficulté
        Vector2 range = GetIntrusRange(currentDifficulty);
        int intruderCount = rng.Next((int)range.x, (int)range.y + 1);

        HashSet<int> intruderIndexes = new HashSet<int>(
            Enumerable.Range(0, itemCount).OrderBy(_ => rng.Next()).Take(intruderCount)
        );

        // 3) Encodage avec correction (protéines < 1 forcées intrus)
        List<int> encoded = new List<int>();
        List<PortionSelection> portions = new List<PortionSelection>();

        for (int i = 0; i < pickedFoods.Count; i++)
        {
            FoodData f = pickedFoods[i];
            bool isProtein = f.Proteins >= 1f;

            if (intruderIndexes.Contains(i))
                encoded.Add(2); // intrus
            else
                encoded.Add(isProtein ? 1 : 2);

            // ✅ Portion simplifiée = toujours 100g
            portions.Add(new PortionSelection
            {
                Type = FoodPortionType.Simple,
                Grams = 100f,
                Value = 100f
            });
        }

        string concat = string.Concat(encoded);
        int solutionEncoded = int.Parse(concat);

        return new QuestionData
        {
            Type = QuestionType.Intru,
            SousType = QuestionSubType.Proteine,
            Aliments = pickedFoods,
            PortionSelections = portions, 
            ValeursComparees = encoded.Select(e => (float)e).ToList(),
            IndexBonneRéponse = solutionEncoded
        };
    }

    private Vector2 GetIntrusRange(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Easy => easyIntrusRange,
            DifficultyLevel.Medium => mediumIntrusRange,
            DifficultyLevel.Hard => hardIntrusRange,
            _ => new Vector2(1, 1)
        };
    }
}