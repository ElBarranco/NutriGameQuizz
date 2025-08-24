using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MealCompositionQuestionGenerator : MonoBehaviour
{
    [Header("Params")]
    [SerializeField] private int mealFoodsCount = 6;                  
    [SerializeField] private Vector2Int pickedRange = new Vector2Int(2, 4);
    [SerializeField] private bool startNonPickedAtZero = true;        

    public QuestionData Generate(List<FoodData> foodList, System.Func<FoodData, PortionSelection> resolvePortionSafe)
    {
        if (foodList == null || foodList.Count == 0)
        {
            Debug.LogWarning("[MealCompositionQuestionFactory] foodList vide.");
            return new QuestionData
            {
                Type = QuestionType.MealComposition,
                SousType = QuestionSubType.Calorie,
                Aliments = new List<FoodData>(),
                PortionSelections = new List<PortionSelection>(),
                ValeursComparees = new List<float> { 0f },
                IndexBonneRéponse = 0
            };
        }

        // 1) Aliments distincts
        List<FoodData> foods = PickDistinctFoods(foodList, mealFoodsCount);

        // 2) Solution cachée + portions
        var portions = new List<PortionSelection>(foods.Count);
        float targetCalories = 0f;

        int minPick = Mathf.Clamp(pickedRange.x, 1, foods.Count);
        int maxPick = Mathf.Clamp(pickedRange.y, minPick, foods.Count);
        int pickCount = Random.Range(minPick, maxPick + 1);
        var picked = PickUniqueIndices(pickCount, foods.Count);

        for (int i = 0; i < foods.Count; i++)
        {
            PortionSelection sel = resolvePortionSafe != null
                ? resolvePortionSafe(foods[i])
                : new PortionSelection { Type = foods[i].PortionType, Grams = 100f };

            if (picked.Contains(i))
            {
                // 🔥 Tirage cohérent en fonction du type de portion
                sel = PickRandomPortionVariant(sel);
            }
            else
            {
                // Non choisi → 0 g
                sel.Grams = startNonPickedAtZero ? 0f : sel.Grams * 0.3f;
            }

            // Conversion + valeur réelle
            sel.Grams = PortionCalculator.ToGrams(sel);
            sel.Value = PortionCalculator.ComputeValue(foods[i], sel, QuestionSubType.Calorie);

            portions.Add(sel);
            targetCalories += sel.Value;
        }

        return new QuestionData
        {
            Type = QuestionType.MealComposition,
            SousType = QuestionSubType.Calorie,
            Aliments = foods,
            PortionSelections = portions,
            ValeursComparees = new List<float> { targetCalories },
            IndexBonneRéponse = 0
        };
    }

    // --- Helpers ---
    private static PortionSelection PickRandomPortionVariant(PortionSelection sel)
    {
        switch (sel.Type)
        {
            case FoodPortionType.Unitaire:
                sel.Unitaire = (PortionUnitaire)Random.Range((int)PortionUnitaire.Demi, (int)PortionUnitaire.Cinq + 1);
                break;

            case FoodPortionType.PetiteUnite:
                sel.PetiteUnite = (PortionPetiteUnite)Random.Range(0, (int)PortionPetiteUnite.Cagette + 1);
                break;

            case FoodPortionType.Liquide:
                sel.Liquide = (PortionLiquide)Random.Range(0, (int)PortionLiquide.Bol + 1);
                break;

            case FoodPortionType.ParPoids:
            default:
                // on garde tel quel (ex: 100 g)
                break;
        }
        return sel;
    }

    private static List<FoodData> PickDistinctFoods(List<FoodData> source, int count)
    {
        var result = new List<FoodData>(Mathf.Min(count, source.Count));
        var pool = new List<FoodData>(source);
        int n = Mathf.Min(count, pool.Count);
        for (int i = 0; i < n; i++)
        {
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return result;
    }

    private static HashSet<int> PickUniqueIndices(int pickCount, int maxExclusive)
    {
        var set = new HashSet<int>();
        while (set.Count < pickCount)
            set.Add(Random.Range(0, maxExclusive));
        return set;
    }
}