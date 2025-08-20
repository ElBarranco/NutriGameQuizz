using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    private List<FoodData> foodList;

    // ★ NEW — références pour les portions/difficulté
    [Header("Portions & Difficulté")]
    [SerializeField] private DifficultyManager difficultyManager;   // assigne dans l’Inspector
    [SerializeField] private float defaultPieceWeightG = 120f;      // poids moyen d’1 unité si Unitaire

    // Configuration interne
    [Header("Drop Rates - Type de question")]
    [SerializeField] private float dropRateCaloriesDual = 0.7f;
    [SerializeField] private float dropRateEstimate = 0.2f;
    [SerializeField] private float dropRateFunMeasure = 0.1f;
    [SerializeField] private float dropRateMealComposition = 0.2f;

    [Header("Drop Rates - Sous-type de question")]
    [SerializeField] private float dropRateCalories = 0.5f;
    [SerializeField] private float dropRateProteine = 0.3f;
    [SerializeField] private float dropRateSucre = 0.2f;

    [Header("Types de questions")]
    [SerializeField] private bool useCaloriesDual = true;
    [SerializeField] private bool useEstimateCalories = true;
    [SerializeField] private bool useFunMeasure = true;
    [SerializeField] private bool useMealComposition = true;

    [Header("Contraintes de génération")]
    [SerializeField] private int minCaloriesDelta = 20;

    [Header("Meal Composition")]

    [SerializeField] private int mealFoodsCount = 6;

    [SerializeField] private SpecialMeasureManager specialMeasureManager;
    [SerializeField] private MealCompositionQuestionGenerator mealCompositionQuestionGenerator;

    public void SetFoodDataList(List<FoodData> filteredFoodList)
    {
        foodList = filteredFoodList;
    }

    public LevelData GenerateLevel(int numberOfQuestions = 10)
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.TypeDeNiveau = LevelType.Normal;
        level.Questions = new List<QuestionData>();

        for (int i = 0; i < numberOfQuestions; i++)
        {
            QuestionType type = GetRandomQuestionTypeWithDropRates();

            if (type == QuestionType.FunMeasure)
            {
                level.Questions.Add(GenerateFunMeasureQuestion());
                continue;
            }

            if (type == QuestionType.MealComposition)
            {
                level.Questions.Add(mealCompositionQuestionGenerator.Generate(foodList, ResolvePortionSafe));
                continue;
            }

            QuestionSubType subType = GetRandomQuestionSubTypeWithDropRates();
            FoodData a;
            FoodData b;
            PickTwoDistinctFoods(subType, out a, out b);

            // ★ NEW — Résoudre les portions selon la difficulté et le type de portion de chaque aliment
            PortionSelection selA = ResolvePortionSafe(a);
            PortionSelection selB = ResolvePortionSafe(b);

            // ★ NEW — On part du principe que les valeurs FoodData sont "pour 100 g"
            float valueA100 = GetValueBySubType(a, subType);
            float valueB100 = GetValueBySubType(b, subType);

            float valueA = valueA100 * (selA.Grams / 100f);
            float valueB = valueB100 * (selB.Grams / 100f);

            List<float> valeurs = new List<float> { valueA, valueB };
            int bonneReponse = valeurs[0] > valeurs[1] ? 0 : 1;

            QuestionData q = new QuestionData
            {
                Type = type,
                SousType = subType,
                Aliments = new List<FoodData> { a, b },
                ValeursComparees = valeurs,
                IndexBonneRéponse = bonneReponse,

                // ★ NEW — on stocke les portions pour l’UI
                PortionSelections = new List<PortionSelection> { selA, selB }
            };

            level.Questions.Add(q);
        }

        return level;
    }

    // ★ NEW — safe wrapper si le DifficultyManager n’est pas assigné
    private PortionSelection ResolvePortionSafe(FoodData food)
    {
        if (difficultyManager != null)
            return difficultyManager.ResolvePortion(food.PortionType, defaultPieceWeightG);

        // Fallback: 100 g / 1 unité / 1 bol selon le type
        var sel = new PortionSelection { Type = food.PortionType, Grams = 100f };
        switch (food.PortionType)
        {
            case FoodPortionType.Unitaire:
                sel.Unitaire = PortionUnitaire.Un;
                sel.Grams = PortionCalculator.ToGrams(PortionUnitaire.Un, defaultPieceWeightG);
                break;
            case FoodPortionType.PetiteUnite:
                sel.PetiteUnite = PortionPetiteUnite.Bol;
                sel.Grams = PortionCalculator.ToGrams(PortionPetiteUnite.Bol);
                break;
        }
        return sel;
    }

    private QuestionData GenerateFunMeasureQuestion()
    {
        List<SpecialMeasureData> measures = new List<SpecialMeasureData>(specialMeasureManager.FunMeasures);
        SpecialMeasureData m1 = measures[Random.Range(0, measures.Count)];
        SpecialMeasureData m2 = measures[Random.Range(0, measures.Count)];

        FoodData a = foodList[Random.Range(0, foodList.Count)];
        FoodData b = foodList[Random.Range(0, foodList.Count)];

        int cal1 = SpecialMeasureManager.GetCaloriesFor(a, m1.VolumeLitres);
        int cal2 = SpecialMeasureManager.GetCaloriesFor(b, m2.VolumeLitres);
        int bonneReponse = cal1 > cal2 ? 0 : 1;

        return new QuestionData
        {
            Type = QuestionType.FunMeasure,
            Aliments = new List<FoodData> { a, b },
            SpecialMeasures = new List<SpecialMeasureData> { m1, m2 },
            ValeursComparees = new List<float> { cal1, cal2 },
            IndexBonneRéponse = bonneReponse
        };
    }

    private QuestionType GetRandomQuestionTypeWithDropRates()
    {
        float total = 0f;
        if (useCaloriesDual) total += dropRateCaloriesDual;
        if (useEstimateCalories) total += dropRateEstimate;
        if (useFunMeasure) total += dropRateFunMeasure;
        if (useMealComposition) total += dropRateMealComposition;

        float rand = Random.Range(0f, total);
        float acc = 0f;

        if (useCaloriesDual && rand < (acc += dropRateCaloriesDual)) return QuestionType.CaloriesDual;
        if (useEstimateCalories && rand < (acc += dropRateEstimate)) return QuestionType.EstimateCalories;
        if (useFunMeasure && rand < (acc += dropRateFunMeasure)) return QuestionType.FunMeasure;
        if (useMealComposition && rand < (acc += dropRateMealComposition)) return QuestionType.MealComposition;

        return QuestionType.CaloriesDual;
    }
    private QuestionSubType GetRandomQuestionSubTypeWithDropRates()
    {
        float total = dropRateCalories + dropRateProteine + dropRateSucre;
        float rand = Random.Range(0f, total);

        if (rand <= dropRateCalories)
            return QuestionSubType.Calorie;
        else if (rand <= dropRateCalories + dropRateProteine)
            return QuestionSubType.Proteine;
        else
            return QuestionSubType.Glucide;
    }

    private void PickTwoDistinctFoods(QuestionSubType sousType, out FoodData a, out FoodData b)
    {
        const int maxAttempts = 50;
        int attempts = 0;
        float minDelta = GetMinDeltaBySubType(sousType);

        do
        {
            a = foodList[Random.Range(0, foodList.Count)];
            b = foodList[Random.Range(0, foodList.Count)];
            attempts++;
        }
        while ((a == b || Mathf.Abs(GetValueBySubType(a, sousType) - GetValueBySubType(b, sousType)) < minDelta) && attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning($"[LevelGenerator] Aliments trop proches pour {sousType}. Choix arbitraire.");
            a = foodList[0];
            b = foodList[1];
        }
    }

    private float GetValueBySubType(FoodData food, QuestionSubType subType)
    {
        return subType switch
        {
            QuestionSubType.Proteine => food.Proteins,
            QuestionSubType.Glucide => food.Carbohydrates,
            QuestionSubType.Lipide => food.Lipids,
            QuestionSubType.Fibres => food.Fibers,
            _ => food.Calories
        };
    }

    private float GetMinDeltaBySubType(QuestionSubType subType)
    {
        return subType switch
        {
            QuestionSubType.Proteine => 3f,
            QuestionSubType.Glucide => 5f,
            QuestionSubType.Lipide => 2f,
            QuestionSubType.Fibres => 2f,
            _ => minCaloriesDelta
        };
    }
}