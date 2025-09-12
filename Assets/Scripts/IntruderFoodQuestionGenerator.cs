using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IntruderFoodQuestionGenerator : MonoBehaviour
{
    // ✅ Constante pour le seuil de protéines
    private const float PROTEIN_THRESHOLD = 2f;

    [Header("Nombre d’aliments selon la difficulté")]
    [SerializeField] private Vector2 easyFoodRange = new Vector2(3, 3);
    [SerializeField] private Vector2 mediumFoodRange = new Vector2(3, 4);
    [SerializeField] private Vector2 hardFoodRange = new Vector2(4, 4);

    [Header("Nombre d’intrus selon la difficulté")]
    [SerializeField] private Vector2 easyIntrusRange = new Vector2(1, 1);
    [SerializeField] private Vector2 mediumIntrusRange = new Vector2(1, 2);
    [SerializeField] private Vector2 hardIntrusRange = new Vector2(2, 3);

    public QuestionData Generate(List<FoodData> foods, DifficultyLevel currentDifficulty)
    {
        System.Random rng = new System.Random();

        // 1) Split par protéines
        List<FoodData> foodsIntrus = foods.Where(f => f.Proteins < PROTEIN_THRESHOLD).ToList();
        List<FoodData> foodsValides = foods.Where(f => f.Proteins >= PROTEIN_THRESHOLD).ToList();

        // 2) Nombre total d’aliments
        int itemCount = GetFoodCount(currentDifficulty);

        // 3) Nombre d’intrus à inclure selon difficulté
        Vector2 intrusRange = GetIntrusRange(currentDifficulty);
        int intruderCount = UnityEngine.Random.Range((int)intrusRange.x, (int)intrusRange.y + 1);

        // Clamp pour éviter dépassements
        intruderCount = Mathf.Clamp(intruderCount, 1, Mathf.Min(itemCount - 1, foodsIntrus.Count));
        int validCount = itemCount - intruderCount;

        // 4) Pick aléatoirement
        List<FoodData> pickedIntrus = foodsIntrus.OrderBy(_ => rng.Next()).Take(intruderCount).ToList();
        List<FoodData> pickedValides = foodsValides.OrderBy(_ => rng.Next()).Take(validCount).ToList();

        List<FoodData> pickedFoods = pickedIntrus.Concat(pickedValides).OrderBy(_ => rng.Next()).ToList();

        // 5) Encodage (1 = bon, 2 = intrus)
        List<int> encoded = new List<int>();
        List<PortionSelection> portions = new List<PortionSelection>();

        foreach (FoodData f in pickedFoods)
        {
            encoded.Add(f.Proteins >= PROTEIN_THRESHOLD ? 1 : 2);

            portions.Add(new PortionSelection
            {
                Type = FoodPortionType.Simple,
                Grams = 100f,
                Value = 100f
            });
        }

        // 6) Encodage solution
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

    private int GetFoodCount(DifficultyLevel difficulty)
    {
        Vector2 range = difficulty switch
        {
            DifficultyLevel.Easy => easyFoodRange,
            DifficultyLevel.Medium => mediumFoodRange,
            DifficultyLevel.Hard => hardFoodRange,
            _ => new Vector2(4, 4)
        };

        return UnityEngine.Random.Range((int)range.x, (int)range.y + 1);
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