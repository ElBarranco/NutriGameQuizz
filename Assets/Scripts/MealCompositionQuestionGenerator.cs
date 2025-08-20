using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MealCompositionQuestionGenerator : MonoBehaviour
{
    [Header("Params")]
    [SerializeField] private int mealFoodsCount = 6;                  // nb d'aliments proposés
    [SerializeField] private Vector2Int pickedRange = new Vector2Int(2, 4); // nb d'aliments utilisés dans la solution
    [SerializeField] private float snapStepG = 10f;                   // pas d’arrondi en grammes
    [SerializeField] private Vector2 solutionMultiplierRange = new Vector2(0.8f, 1.6f); // multiplicateur de la portion de base
    [SerializeField] private float maxMultiplier = 3f;                // clamp max par rapport à la base
    [SerializeField] private bool startNonPickedAtZero = true;        // les non-sélectionnés démarrent à 0 g

    /// <summary>
    /// Génère une question "MealComposition".
    /// - foodList : pool d'aliments
    /// - resolvePortionSafe : callback pour obtenir une portion de base par aliment (déjà présent dans LevelGenerator)
    /// </summary>
    public QuestionData Generate(List<FoodData> foodList, System.Func<FoodData, PortionSelection> resolvePortionSafe)
    {
        if (foodList == null || foodList.Count == 0)
        {
            Debug.LogWarning("[MealCompositionQuestionFactory] foodList vide.");
            return new QuestionData { Type = QuestionType.MealComposition, Aliments = new List<FoodData>(), PortionSelections = new List<PortionSelection>(), ValeursComparees = new List<float> { 0f }, IndexBonneRéponse = 0 };
        }

        // 1) Aliments distincts
        List<FoodData> foods = PickDistinctFoods(foodList, mealFoodsCount);

        // 2) Solution cachée + portions initiales UI
        var portions = new List<PortionSelection>(foods.Count);
        float targetCalories = 0f;

        int minPick = Mathf.Clamp(pickedRange.x, 1, foods.Count);
        int maxPick = Mathf.Clamp(pickedRange.y, minPick, foods.Count);
        int pickCount = Random.Range(minPick, maxPick + 1);
        var picked = PickUniqueIndices(pickCount, foods.Count);

        for (int i = 0; i < foods.Count; i++)
        {
            PortionSelection baseSel = resolvePortionSafe != null
                ? resolvePortionSafe(foods[i])
                : new PortionSelection { Grams = 100f, Type = foods[i].PortionType }; // fallback

            float grams;
            if (picked.Contains(i))
            {
                float candidate = baseSel.Grams * Random.Range(solutionMultiplierRange.x, solutionMultiplierRange.y);
                grams = Mathf.Clamp(candidate, 10f, baseSel.Grams * maxMultiplier);
            }
            else
            {
                grams = startNonPickedAtZero ? 0f : Mathf.Max(0f, baseSel.Grams * 0.3f);
            }

            grams = SnapTo(grams, snapStepG);
            baseSel.Grams = grams;
            portions.Add(baseSel);

            targetCalories += foods[i].Calories * (grams / 100f);
        }

        // 3) QuestionData — cible dans ValeursComparees[0], pas d’autres champs ajoutés
        return new QuestionData
        {
            Type = QuestionType.MealComposition,
            SousType = QuestionSubType.Calorie,
            Aliments = foods,
            PortionSelections = portions,
            ValeursComparees = new List<float> { SnapCalories(targetCalories, 25f) },
            IndexBonneRéponse = 0,
            SpecialMeasures = null
        };
    }

    // --- Helpers locaux ---
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

    private static float SnapTo(float value, float step)
    {
        if (step <= 0f) return value;
        return Mathf.Round(value / step) * step;
    }

    private static float SnapCalories(float value, float step = 25f)
    {
        if (step <= 0f) return value;
        return Mathf.Round(value / step) * step;
    }
}