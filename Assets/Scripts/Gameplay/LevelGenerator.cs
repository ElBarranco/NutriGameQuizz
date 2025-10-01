using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using NaughtyAttributes;

public class LevelGenerator : QuestionGenerator
{
    private List<FoodData> foodList;

    [Header("Drop Rates - Type de question")]
    [SerializeField] private float dropRateCaloriesDual    = 0.7f;
    [SerializeField] private float dropRateEstimate        = 0.2f;
    [SerializeField] private float dropRateSugar           = 0.2f;
    [SerializeField] private float dropRateFunMeasure      = 0.1f;
    [SerializeField] private float dropRateMealComposition = 0.2f;
    [SerializeField] private float dropRateSportDual       = 1f;
    [SerializeField] private float dropRateSort            = 0.5f;
    [SerializeField] private float dropRateIntru           = 0.5f;
    [SerializeField] private float dropRateRecycling       = 0.5f;
    [SerializeField] private float dropRateSubtraction     = 0.5f;
    [SerializeField] private float dropTableNutrition      = 0.5f;

    [Header("Drop Rates - Sous-type de question")]
    [SerializeField] private float dropRateCalories = 0.5f;
    [SerializeField] private float dropRateProteine = 0.3f;
    [SerializeField] private float dropRateSucre    = 0.2f;

    [Header("Types de questions")]
    [SerializeField] private bool useCaloriesDual     = true;
    [SerializeField] private bool useEstimateCalories = true;
    [SerializeField] private bool useSugar            = true;
    [SerializeField] private bool useFunMeasure       = true;
    [SerializeField] private bool useMealComposition  = true;
    [SerializeField] private bool useSportDual        = true;
    [SerializeField] private bool useSort             = true;
    [SerializeField] private bool useIntrus           = true;
    [SerializeField] private bool useRecycling        = true;
    [SerializeField] private bool useSubtraction      = true;
    [SerializeField] private bool useTableNutrition   = true;

    [Header("Contraintes de génération")]
    [SerializeField] private int minCaloriesDelta = 20;

    [Header("Références Générateurs")]
    [SerializeField] private SpecialMeasureManager              specialMeasureManager;
    [SerializeField] private MealCompositionQuestionGenerator   mealCompositionQuestionGenerator;
    [SerializeField] private SportCaloriesDualQuestionGenerator sportCaloriesDualQuestionGenerator;
    [SerializeField] private SortFoodQuestionGenerator          sortFoodQuestionGenerator;
    [SerializeField] private IntruderFoodQuestionGenerator      intruderFoodQuestionGenerator;
    [SerializeField] private RecyclingFoodQuestionGenerator     recyclingFoodQuestionGenerator;
    [SerializeField] private EstimateQuestionGenerator          estimateQuestionGenerator;
    [SerializeField] private SugarQuestionGenerator             sugarQuestionGenerator;
    [SerializeField] private SubtractionQuestionGenerator       subtractionQuestionGenerator;
    [SerializeField] private NutritionTableQuestionGenerator    nutritionTableQuestionGenerator;

    public void SetFoodDataList(List<FoodData> filteredFoodList)
    {
        foodList = filteredFoodList;
    }

    public LevelData GenerateLevel(int numberOfQuestions, DifficultyLevel currentDifficulty)
    {
        var level = ScriptableObject.CreateInstance<LevelData>();
        level.TypeDeNiveau = LevelType.Normal;
        level.Difficulty   = currentDifficulty;
        level.Questions    = new List<QuestionData>();

        for (int i = 0; i < numberOfQuestions; i++)
        {
            QuestionData q;
            QuestionType type = GetRandomQuestionTypeWithDropRates();

            switch (type)
            {
                case QuestionType.FunMeasure:
                    q = GenerateFunMeasureQuestion();
                    break;

                case QuestionType.MealComposition:
                    q = mealCompositionQuestionGenerator.Generate(
                            foodList,
                            food => base.ResolvePortionSafe(food, QuestionSubType.Calorie)
                        );
                    break;

                case QuestionType.EstimateCalories:
                    q = estimateQuestionGenerator.Generate(foodList, currentDifficulty);
                    break;

                case QuestionType.Sugar:
                    q = sugarQuestionGenerator.Generate(foodList, currentDifficulty);
                    break;

                case QuestionType.Sport:
                    q = sportCaloriesDualQuestionGenerator.Generate(
                            foodList,
                            food => base.ResolvePortionSafe(food, QuestionSubType.Calorie),
                            currentDifficulty
                        );
                    break;

                case QuestionType.Tri:
                    q = sortFoodQuestionGenerator.Generate(
                            foodList,
                            food => base.ResolvePortionSafe(food, QuestionSubType.Calorie),
                            currentDifficulty
                        );
                    break;

                case QuestionType.Intru:
                    q = intruderFoodQuestionGenerator.Generate(foodList, currentDifficulty);
                    break;

                case QuestionType.Recycling:
                    q = recyclingFoodQuestionGenerator.Generate(foodList, currentDifficulty);
                    break;

                case QuestionType.Subtraction:
                    q = subtractionQuestionGenerator.Generate(foodList, currentDifficulty);
                    break;

                case QuestionType.NutritionTable:
                    q = nutritionTableQuestionGenerator.Generate(foodList, currentDifficulty);
                    break;

                case QuestionType.CaloriesDual:
                default:
                    q = GenerateCaloriesDualQuestion();
                    break;
            }

            q.QuestionId = i;
            level.Questions.Add(q);
        }

        return level;
    }

    private QuestionData GenerateCaloriesDualQuestion()
    {
        QuestionSubType subType = GetRandomQuestionSubTypeWithDropRates();
        PickTwoDistinctFoods(subType, out var a, out var b);

        var selA = base.ResolvePortionSafe(a, subType);
        var selB = base.ResolvePortionSafe(b, subType);
        float valueA = selA.Value;
        float valueB = selB.Value;

        return new QuestionData
        {
            Type              = QuestionType.CaloriesDual,
            SousType          = subType,
            Aliments          = new List<FoodData> { a, b },
            PortionSelections = new List<PortionSelection> { selA, selB },
            ValeursComparees  = new List<float> { valueA, valueB },
            IndexBonneRéponse = valueA > valueB ? 0 : 1
        };
    }

    private QuestionData GenerateEstimateCaloriesQuestion()
    {
        var f   = foodList[Random.Range(0, foodList.Count)];
        var sel = base.ResolvePortionSafe(f, QuestionSubType.Calorie);

        return new QuestionData
        {
            Type              = QuestionType.EstimateCalories,
            SousType          = QuestionSubType.Calorie,
            Aliments          = new List<FoodData> { f },
            PortionSelections = new List<PortionSelection> { sel },
            ValeursComparees  = new List<float> { sel.Value },
            IndexBonneRéponse = 0
        };
    }

    private QuestionData GenerateFunMeasureQuestion()
    {
        var measures = new List<SpecialMeasureData>(specialMeasureManager.FunMeasures);
        var m1 = measures[Random.Range(0, measures.Count)];
        var m2 = measures[Random.Range(0, measures.Count)];
        var a  = foodList[Random.Range(0, foodList.Count)];
        var b  = foodList[Random.Range(0, foodList.Count)];

        int cal1 = SpecialMeasureManager.GetCaloriesFor(a, m1.VolumeLitres);
        int cal2 = SpecialMeasureManager.GetCaloriesFor(b, m2.VolumeLitres);

        return new QuestionData
        {
            Type              = QuestionType.FunMeasure,
            Aliments          = new List<FoodData> { a, b },
            SpecialMeasures   = new List<SpecialMeasureData> { m1, m2 },
            ValeursComparees  = new List<float> { cal1, cal2 },
            IndexBonneRéponse = cal1 > cal2 ? 0 : 1
        };
    }

    private QuestionType GetRandomQuestionTypeWithDropRates()
    {
        float total = 0f;
        if (useCaloriesDual)     total += dropRateCaloriesDual;
        if (useEstimateCalories) total += dropRateEstimate;
        if (useSugar)            total += dropRateSugar;
        if (useFunMeasure)       total += dropRateFunMeasure;
        if (useMealComposition)  total += dropRateMealComposition;
        if (useSportDual)        total += dropRateSportDual;
        if (useSort)             total += dropRateSort;
        if (useIntrus)           total += dropRateIntru;
        if (useRecycling)        total += dropRateRecycling;
        if (useSubtraction)      total += dropRateSubtraction;
        if (useTableNutrition)   total += dropTableNutrition;

        if (total <= 0f)
            return QuestionType.CaloriesDual;

        float rand = Random.Range(0f, total);
        float acc  = 0f;

        if (useCaloriesDual     && rand < (acc += dropRateCaloriesDual))    return QuestionType.CaloriesDual;
        if (useEstimateCalories && rand < (acc += dropRateEstimate))        return QuestionType.EstimateCalories;
        if (useSugar            && rand < (acc += dropRateSugar))           return QuestionType.Sugar;
        if (useFunMeasure       && rand < (acc += dropRateFunMeasure))      return QuestionType.FunMeasure;
        if (useMealComposition  && rand < (acc += dropRateMealComposition)) return QuestionType.MealComposition;
        if (useSportDual        && rand < (acc += dropRateSportDual))       return QuestionType.Sport;
        if (useSort             && rand < (acc += dropRateSort))            return QuestionType.Tri;
        if (useIntrus           && rand < (acc += dropRateIntru))           return QuestionType.Intru;
        if (useRecycling        && rand < (acc += dropRateRecycling))       return QuestionType.Recycling;
        if (useSubtraction      && rand < (acc += dropRateSubtraction))     return QuestionType.Subtraction;
        if (useTableNutrition   && rand < (acc += dropTableNutrition))      return QuestionType.NutritionTable;

        return QuestionType.CaloriesDual;
    }

    private QuestionSubType GetRandomQuestionSubTypeWithDropRates()
    {
        float total = dropRateCalories + dropRateProteine + dropRateSucre;
        float rand  = Random.Range(0f, total);

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
        while ((a == b || Mathf.Abs(GetValueBySubType(a, sousType) - GetValueBySubType(b, sousType)) < minDelta)
               && attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning($"[LevelGenerator] Aliments trop proches pour {sousType}. Choix arbitraire.");
            a = foodList[0];
            b = foodList[1];
        }
    }

    private float GetValueBySubType(FoodData food, QuestionSubType subType) =>
        subType switch
        {
            QuestionSubType.Proteine => food.Proteins,
            QuestionSubType.Glucide  => food.Carbohydrates,
            QuestionSubType.Lipide   => food.Lipids,
            QuestionSubType.Fibres   => food.Fibers,
            _                        => food.Calories
        };

    private float GetMinDeltaBySubType(QuestionSubType subType) =>
        subType switch
        {
            QuestionSubType.Proteine => 3f,
            QuestionSubType.Glucide  => 5f,
            QuestionSubType.Lipide   => 2f,
            QuestionSubType.Fibres   => 2f,
            _                        => minCaloriesDelta
        };

    [Button("Toggle All")]
    private void ToggleAll()
    {
        bool allEnabled = useCaloriesDual
                          && useEstimateCalories
                          && useSugar
                          && useFunMeasure
                          && useMealComposition
                          && useSportDual
                          && useSort
                          && useIntrus
                          && useRecycling
                          && useSubtraction
                          && useTableNutrition;

        bool newValue = !allEnabled;

        useCaloriesDual      = newValue;
        useEstimateCalories  = newValue;
        useSugar             = newValue;
        useFunMeasure        = newValue;
        useMealComposition   = newValue;
        useSportDual         = newValue;
        useSort              = newValue;
        useIntrus            = newValue;
        useRecycling         = newValue;
        useSubtraction       = newValue;
        useTableNutrition    = newValue;
    }
}