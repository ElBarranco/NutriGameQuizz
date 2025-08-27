using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MealCompositionQuestionGenerator : QuestionGenerator
{
    [Header("Params")]
    [SerializeField] private int mealFoodsCount = 6;
    [SerializeField] private Vector2Int pickedRange = new Vector2Int(2, 4);
    [SerializeField] private bool startNonPickedAtZero = true;

    [Header("Sous-type")]
    [SerializeField] private bool randomSubType = true;
    [SerializeField] private QuestionSubType forcedSubType = QuestionSubType.Calorie; // utilisé si randomSubType = false

    [Header("Robustesse")]
    [SerializeField] private int maxUniqueAttempts = 30;    // nb d’essais si collision de somme
    [SerializeField] private float targetSnap = 25f;        // arrondi de la cible
    [SerializeField] private float uniquenessTolerance = 5f; // tolérance pour comparer les sommes

    public QuestionData Generate(List<FoodData> foodList, System.Func<FoodData, PortionSelection> resolvePortionSafe)
    {
        if (foodList == null || foodList.Count == 0)
        {
            Debug.LogWarning("[MealComposition] foodList vide.");
            return new QuestionData
            {
                Type = QuestionType.MealComposition,
                SousType = QuestionSubType.Calorie,
                Aliments = new List<FoodData>(),
                PortionSelections = new List<PortionSelection>(),
                ValeursComparees = new List<float> { 0f },
                IndexBonneRéponse = 0,
                Solutions = new List<int>(),
                MealTargetTolerance = targetSnap
            };
        }

        // 0) Choisir le sous-type ici (random ou forcé)
        QuestionSubType subType = randomSubType ? GetRandomMealSubType() : forcedSubType;

        // 1) Aliments distincts
        List<FoodData> foods = PickDistinctFoods(foodList, mealFoodsCount);

        // Mémos pour fallback si unicité échoue
        List<PortionSelection> lastPortions = null;
        List<int> lastSolutions = null;
        float lastSnappedTarget = 0f;

        // 2) Chercher une solution unique
        for (int attempt = 0; attempt < maxUniqueAttempts; attempt++)
        {
            int minPick = Mathf.Clamp(pickedRange.x, 1, foods.Count);
            int maxPick = Mathf.Clamp(pickedRange.y, minPick, foods.Count);
            int pickCount = Random.Range(minPick, maxPick + 1);
            HashSet<int> picked = PickUniqueIndices(pickCount, foods.Count);

            List<PortionSelection> portions = new List<PortionSelection>(foods.Count);
            List<float> itemValues = new List<float>(foods.Count);
            float targetValue = 0f;

            for (int i = 0; i < foods.Count; i++)
            {
                // Portion déjà normalisée et valuée par base.ResolvePortion(...)
                PortionSelection sel = base.ResolvePortion(resolvePortionSafe, foods[i], subType);

                // (Option) si tu veux diversifier la QUANTITÉ des non‑picked
                if (!picked.Contains(i))
                {
                    // Réduction ou zéro -> il faut alors recalculer la Value une seule fois
                    sel.Grams = startNonPickedAtZero ? 0f : sel.Grams * 0.3f;
                    sel.Value = PortionCalculator.ComputeValue(foods[i], sel.Grams, subType);
                }

                itemValues.Add(sel.Value);
                if (picked.Contains(i)) targetValue += sel.Value;

                portions.Add(sel);
            }

            float snappedTarget = Snap(targetValue, targetSnap);

            lastPortions = portions;
            lastSnappedTarget = snappedTarget;
            lastSolutions = new List<int>(picked);

            // Unicité : aucun autre sous-ensemble ne doit matcher la cible
            if (IsUniqueSubsetSum(itemValues, picked, snappedTarget, uniquenessTolerance))
            {
                return new QuestionData
                {
                    Type = QuestionType.MealComposition,
                    SousType = subType, // ✅ on stocke le sous-type tiré
                    Aliments = foods,
                    PortionSelections = portions,
                    ValeursComparees = new List<float> { snappedTarget },
                    IndexBonneRéponse = 0,
                    Solutions = new List<int>(picked),
                    MealTargetTolerance = targetSnap
                };
            }
        }

        Debug.LogWarning("[MealComposition] Unicité non garantie après maxUniqueAttempts — on renvoie la dernière génération.");
        return new QuestionData
        {
            Type = QuestionType.MealComposition,
            SousType = randomSubType ? GetRandomMealSubType() : forcedSubType, // au cas où tu veux indiquer un sous-type
            Aliments = foods,
            PortionSelections = lastPortions ?? new List<PortionSelection>(),
            ValeursComparees = new List<float> { lastSnappedTarget },
            IndexBonneRéponse = 0,
            Solutions = lastSolutions ?? new List<int>(),
            MealTargetTolerance = targetSnap
        };
    }

    // --- Sous-type aléatoire (simple : Calories / Protéines / Glucides) ---
    private static QuestionSubType GetRandomMealSubType()
    {
        // Adapte si tu veux inclure Lipides/Fibres/etc.
        int r = Random.Range(0, 3); // 0..2
        switch (r)
        {
            case 0: return QuestionSubType.Calorie;
            case 1: return QuestionSubType.Proteine;
            default: return QuestionSubType.Glucide;
        }
    }

    // --- Unicité ---
    private static bool IsUniqueSubsetSum(List<float> itemValues, HashSet<int> picked, float target, float tol)
    {
        int n = itemValues.Count;
        int pickedMask = 0;
        for (int i = 0; i < n; i++)
            if (picked.Contains(i))
                pickedMask |= (1 << i);

        int maxMask = 1 << n;
        for (int mask = 1; mask < maxMask; mask++)
        {
            if (mask == pickedMask) continue;
            float sum = 0f;
            for (int i = 0; i < n; i++)
                if ((mask & (1 << i)) != 0)
                    sum += itemValues[i];

            if (Mathf.Abs(Snap(sum, 1f) - target) <= tol)
                return false;
        }
        return true;
    }

    // --- Helpers locaux ---


    private static float Snap(float v, float step)
    {
        if (step <= 0f) return v;
        return Mathf.Round(v / step) * step;
    }

    private static List<FoodData> PickDistinctFoods(List<FoodData> source, int count)
    {
        int capacity = Mathf.Min(count, source.Count);
        List<FoodData> result = new List<FoodData>(capacity);
        List<FoodData> pool = new List<FoodData>(source);
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
        HashSet<int> set = new HashSet<int>();
        while (set.Count < pickCount)
            set.Add(Random.Range(0, maxExclusive));
        return set;
    }
}