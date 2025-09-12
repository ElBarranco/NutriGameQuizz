using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IntruderFoodQuestionGenerator : MonoBehaviour
{
    // ✅ Seuils constants (tu ajustes ici si besoin)
    private const float PROTEIN_THRESHOLD = 2f;
    private const float CARB_THRESHOLD = 5f;
    private const float LIPID_THRESHOLD = 3f;

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

        // 1) Random du sous-type (Prot / Glucide / Lipide)
        QuestionSubType subType = GetRandomSubType(rng);

        // 2) Split valides / intrus selon le sous-type choisi
        (List<FoodData> foodsIntrus, List<FoodData> foodsValides) = SplitFoods(foods, subType);

        // 3) Nombre total d’aliments
        int itemCount = GetFoodCount(currentDifficulty);

        // 4) Nombre d’intrus à inclure
        Vector2 intrusRange = GetIntrusRange(currentDifficulty);
        int intruderCount = UnityEngine.Random.Range((int)intrusRange.x, (int)intrusRange.y + 1);

        // Clamp pour éviter erreurs
        intruderCount = Mathf.Clamp(intruderCount, 1, Mathf.Min(itemCount - 1, foodsIntrus.Count));
        int validCount = itemCount - intruderCount;

        // 5) Pick aléatoirement
        List<FoodData> pickedIntrus = foodsIntrus.OrderBy(_ => rng.Next()).Take(intruderCount).ToList();
        List<FoodData> pickedValides = foodsValides.OrderBy(_ => rng.Next()).Take(validCount).ToList();

        List<FoodData> pickedFoods = pickedIntrus.Concat(pickedValides).OrderBy(_ => rng.Next()).ToList();

        // 6) Encodage (1 = bon, 2 = intrus)
        List<int> encoded = new List<int>();
        List<PortionSelection> portions = new List<PortionSelection>();

        foreach (FoodData f in pickedFoods)
        {
            bool isValid = IsFoodValid(f, subType);
            encoded.Add(isValid ? 1 : 2);

            portions.Add(new PortionSelection
            {
                Type = FoodPortionType.Simple,
                Grams = 100f,
                Value = 100f
            });
        }

        // 7) Encodage solution
        string concat = string.Concat(encoded);
        int solutionEncoded = int.Parse(concat);

        return new QuestionData
        {
            Type = QuestionType.Intru,
            SousType = subType, // ✅ dynamique
            Aliments = pickedFoods,
            PortionSelections = portions,
            ValeursComparees = encoded.Select(e => (float)e).ToList(),
            IndexBonneRéponse = solutionEncoded
        };
    }

    // --- Helpers ---

    private QuestionSubType GetRandomSubType(System.Random rng)
    {
        QuestionSubType[] possible = { QuestionSubType.Proteine, QuestionSubType.Glucide, QuestionSubType.Lipide };
        return possible[rng.Next(possible.Length)];
    }

    private (List<FoodData> intrus, List<FoodData> valides) SplitFoods(List<FoodData> foods, QuestionSubType subType)
    {
        switch (subType)
        {
            case QuestionSubType.Proteine:
                return (foods.Where(f => f.Proteins < PROTEIN_THRESHOLD).ToList(),
                        foods.Where(f => f.Proteins >= PROTEIN_THRESHOLD).ToList());

            case QuestionSubType.Glucide:
                return (foods.Where(f => f.Carbohydrates < CARB_THRESHOLD).ToList(),
                        foods.Where(f => f.Carbohydrates >= CARB_THRESHOLD).ToList());

            case QuestionSubType.Lipide:
                return (foods.Where(f => f.Lipids < LIPID_THRESHOLD).ToList(),
                        foods.Where(f => f.Lipids >= LIPID_THRESHOLD).ToList());

            default:
                return (new List<FoodData>(), foods);
        }
    }

    private bool IsFoodValid(FoodData f, QuestionSubType subType)
    {
        return subType switch
        {
            QuestionSubType.Proteine => f.Proteins >= PROTEIN_THRESHOLD,
            QuestionSubType.Glucide => f.Carbohydrates >= CARB_THRESHOLD,
            QuestionSubType.Lipide => f.Lipids >= LIPID_THRESHOLD,
            _ => true
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