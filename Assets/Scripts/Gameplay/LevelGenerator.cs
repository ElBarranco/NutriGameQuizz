using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : QuestionGenerator
{
    private List<FoodData> foodList;

    // Configuration interne
    [Header("Drop Rates - Type de question")]
    [SerializeField] private float dropRateCaloriesDual = 0.7f;
    [SerializeField] private float dropRateEstimate = 0.2f;
    [SerializeField] private float dropRateFunMeasure = 0.1f;
    [SerializeField] private float dropRateMealComposition = 0.2f;
    [SerializeField] private float dropRateSportDual = 1f;
    [SerializeField] private float dropRateSort = 0.5f;
    [SerializeField] private float dropRateIntru = 0.5f;
    [SerializeField] private float dropRateRecycling = 0.5f; 

    [Header("Drop Rates - Sous-type de question")]
    [SerializeField] private float dropRateCalories = 0.5f;
    [SerializeField] private float dropRateProteine = 0.3f;
    [SerializeField] private float dropRateSucre = 0.2f;

    [Header("Types de questions")]
    [SerializeField] private bool useCaloriesDual = true;
    [SerializeField] private bool useEstimateCalories = true;
    [SerializeField] private bool useFunMeasure = true;
    [SerializeField] private bool useMealComposition = true;
    [SerializeField] private bool useSportDual = true;
    [SerializeField] private bool useSort = true;
    [SerializeField] private bool useIntrus = true;
    [SerializeField] private bool useRecycling = true; 

    [Header("Contraintes de génération")]
    [SerializeField] private int minCaloriesDelta = 20;

    [Header("Meal Composition")]
    [SerializeField] private int mealFoodsCount = 6;

    [SerializeField] private SpecialMeasureManager specialMeasureManager;
    [SerializeField] private MealCompositionQuestionGenerator mealCompositionQuestionGenerator;
    [SerializeField] private SportCaloriesDualQuestionGenerator sportCaloriesDualQuestionGenerator;
    [SerializeField] private SortFoodQuestionGenerator sortFoodQuestionGenerator;
    [SerializeField] private IntruderFoodQuestionGenerator intruderFoodQuestionGenerator;
    [SerializeField] private RecyclingFoodQuestionGenerator recyclingFoodQuestionGenerator; 

    public void SetFoodDataList(List<FoodData> filteredFoodList)
    {
        foodList = filteredFoodList;
    }

    public LevelData GenerateLevel(int numberOfQuestions, DifficultyLevel currentDifficulty)
    {
        LevelData level = ScriptableObject.CreateInstance<LevelData>();
        level.TypeDeNiveau = LevelType.Normal;
        level.Difficulty = currentDifficulty;
        level.Questions = new List<QuestionData>();

        for (int i = 0; i < numberOfQuestions; i++)
        {
            QuestionType type = GetRandomQuestionTypeWithDropRates();

            switch (type)
            {
                case QuestionType.FunMeasure:
                    level.Questions.Add(GenerateFunMeasureQuestion());
                    break;

                case QuestionType.MealComposition:
                    level.Questions.Add(
                        mealCompositionQuestionGenerator.Generate(
                            foodList,
                            food => base.ResolvePortionSafe(food, QuestionSubType.Calorie)
                        )
                    );
                    break;

                case QuestionType.EstimateCalories:
                    level.Questions.Add(GenerateEstimateCaloriesQuestion());
                    break;

                case QuestionType.Sport:
                    level.Questions.Add(
                        sportCaloriesDualQuestionGenerator.Generate(
                            foodList,
                            food => base.ResolvePortionSafe(food, QuestionSubType.Calorie),
                            currentDifficulty
                        )
                    );
                    break;

                case QuestionType.Tri:
                    level.Questions.Add(
                        sortFoodQuestionGenerator.Generate(
                            foodList,
                            food => base.ResolvePortionSafe(food, QuestionSubType.Calorie),
                            currentDifficulty
                        )
                    );
                    break;

                case QuestionType.Intru:
                    level.Questions.Add(
                        intruderFoodQuestionGenerator.Generate(foodList, currentDifficulty)
                    );
                    break;

                case QuestionType.Recycling: // ✅ Nouveau case
                    level.Questions.Add(
                        recyclingFoodQuestionGenerator.Generate(foodList, currentDifficulty)
                    );
                    break;

                case QuestionType.CaloriesDual:
                default:
                    {
                        QuestionSubType subType = GetRandomQuestionSubTypeWithDropRates();

                        FoodData a;
                        FoodData b;
                        PickTwoDistinctFoods(subType, out a, out b);

                        PortionSelection selA = base.ResolvePortionSafe(a, subType);
                        PortionSelection selB = base.ResolvePortionSafe(b, subType);

                        float valueA = selA.Value;
                        float valueB = selB.Value;

                        List<float> valeurs = new List<float> { valueA, valueB };
                        int bonneReponse = valeurs[0] > valeurs[1] ? 0 : 1;

                        QuestionData q = new QuestionData
                        {
                            Type = QuestionType.CaloriesDual,
                            SousType = subType,
                            Aliments = new List<FoodData> { a, b },
                            ValeursComparees = valeurs,
                            IndexBonneRéponse = bonneReponse,
                            PortionSelections = new List<PortionSelection> { selA, selB }
                        };

                        level.Questions.Add(q);
                        break;
                    }
            }
        }

        return level;
    }

    private QuestionData GenerateEstimateCaloriesQuestion()
    {
        FoodData f = foodList[Random.Range(0, foodList.Count)];
        QuestionSubType subType = GetRandomQuestionSubTypeWithDropRates();
        PortionSelection sel = base.ResolvePortionSafe(f, subType);

        return new QuestionData
        {
            Type = QuestionType.EstimateCalories,
            SousType = QuestionSubType.Calorie,
            Aliments = new List<FoodData> { f },
            PortionSelections = new List<PortionSelection> { sel },
            ValeursComparees = new List<float> { sel.Value },
            IndexBonneRéponse = 0
        };
    }

    // ---------- Génération FunMeasure ----------
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

    // ---------- Helpers ----------
    private QuestionType GetRandomQuestionTypeWithDropRates()
    {
        float total = 0f;
        if (useCaloriesDual) total += dropRateCaloriesDual;
        if (useEstimateCalories) total += dropRateEstimate;
        if (useFunMeasure) total += dropRateFunMeasure;
        if (useMealComposition) total += dropRateMealComposition;
        if (useSportDual) total += dropRateSportDual;
        if (useSort) total += dropRateSort;
        if (useIntrus) total += dropRateIntru;
        if (useRecycling) total += dropRateRecycling; // ✅ Nouveau

        if (total <= 0f)
            return QuestionType.CaloriesDual;

        float rand = Random.Range(0f, total);
        float acc = 0f;

        if (useCaloriesDual && rand < (acc += dropRateCaloriesDual)) return QuestionType.CaloriesDual;
        if (useEstimateCalories && rand < (acc += dropRateEstimate)) return QuestionType.EstimateCalories;
        if (useFunMeasure && rand < (acc += dropRateFunMeasure)) return QuestionType.FunMeasure;
        if (useMealComposition && rand < (acc += dropRateMealComposition)) return QuestionType.MealComposition;
        if (useSportDual && rand < (acc += dropRateSportDual)) return QuestionType.Sport;
        if (useSort && rand < (acc += dropRateSort)) return QuestionType.Tri;
        if (useIntrus && rand < (acc += dropRateIntru)) return QuestionType.Intru;
        if (useRecycling && rand < (acc += dropRateRecycling)) return QuestionType.Recycling; // ✅ Nouveau

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