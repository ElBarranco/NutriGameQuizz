using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SportCaloriesDualQuestionGenerator : QuestionGenerator
{
    [Header("Dépendances")]
    [SerializeField] private SportDataParser sportDataParser;

    [Header("Sélection aliment")]
    [SerializeField] private bool pickRandomFood = true;
    [SerializeField] private int foodIndexIfNotRandom = 0;

    [Header("Durées (minutes)")]
    [SerializeField] private int minMinutes = 5;
    [SerializeField] private int maxMinutes = 180;
    [SerializeField] private int roundToMinutes = 5;

    [Header("Tirage des sports")]
    [SerializeField] private bool weightByRarity = true;
    [SerializeField] private int maxPickAttempts = 30;

    [Header("Robustesse")]
    [SerializeField] private float perfectTolerancePercent = 0.05f;

    public QuestionData Generate(List<FoodData> foodList, System.Func<FoodData, PortionSelection> resolvePortionSafe, DifficultyLevel currentDifficulty)
    {
        List<SportData> sports = sportDataParser.GetSports();

        // --- 1) Sélection aliment ---
        List<FoodData> picked = base.PickDistinctFoods(foodList, 1);
        FoodData food = picked[0];
        PortionSelection portion = base.ResolvePortion(resolvePortionSafe, food, QuestionSubType.Calorie);
        int targetCalories = Mathf.Max(0, Mathf.RoundToInt(portion.Value));

        // --- 2) Sélection des 2 sports ---
        SportData sA, sB;
        if (currentDifficulty == DifficultyLevel.Easy)
        {
            SelectEasySport(sports, out sA, out sB);
        }
        else
        {
            if (!PickTwoDistinctSports(sports, out sA, out sB))
            {
                Debug.LogWarning("[SportDual] Échec du tirage de deux sports distincts.");
                return null;
            }
        }

        // --- 3) Durée sport A (cible exacte)
        int minutesA = ComputeRoundedMinutesForCalories(targetCalories, sA.CaloriesPerHour);
        minutesA = Mathf.Clamp(minutesA, minMinutes, maxMinutes);

        // --- 4) Durée sport B (variation crédible)
        int maxDelta = (minutesA <= 60) ? 20 : 30;
        int deltaSteps = maxDelta / roundToMinutes;
        int variationSteps = Random.Range(2, deltaSteps + 1);
        int variation = variationSteps * roundToMinutes;
        if (Random.value < 0.5f) variation = -variation;

        int minutesB = Mathf.Clamp(minutesA + variation, minMinutes, maxMinutes);
        if (Mathf.Abs(minutesB - minutesA) < roundToMinutes * 2)
            minutesB += (variation > 0 ? roundToMinutes * 2 : -roundToMinutes * 2);

        // --- 5) Calcul des kcal réels
        int calA = CaloriesFor(sA.CaloriesPerHour, minutesA);
        int calB = CaloriesFor(sB.CaloriesPerHour, minutesB);

        sA.Duration = minutesA;
        sB.Duration = minutesB;
        sA.Calories = calA;
        sB.Calories = calB;

        int diffA = Mathf.Abs(targetCalories - calA);
        int diffB = Mathf.Abs(targetCalories - calB);
        int indexCorrect = diffA <= diffB ? 0 : 1;

        // --- 6) Construction finale
        return new QuestionData
        {
            Type = QuestionType.Sport,
            SousType = QuestionSubType.Calorie,
            Aliments = new List<FoodData> { food },
            PortionSelections = new List<PortionSelection> { portion },
            ValeursComparees = new List<float> { targetCalories },
            IndexBonneRéponse = indexCorrect,
            Solutions = new List<int> { minutesA, minutesB },
            SpecialMeasures = new List<SpecialMeasureData>(),
            DeltaTolerance = 0,
            SportChoices = new List<SportData> { sA, sB }
        };
    }

    private void SelectEasySport(List<SportData> sports, out SportData sA, out SportData sB)
    {
        SportData baseSport = sports[Random.Range(0, sports.Count)];
        sA = baseSport;
        sB = new SportData(baseSport); // constructeur de copie
    }

    private int ComputeRoundedMinutesForCalories(int targetCalories, int caloriesPerHour)
    {
        if (caloriesPerHour <= 0) return minMinutes;

        float minutes = (targetCalories / (float)caloriesPerHour) * 60f;
        int rounded = RoundToMultiple(Mathf.RoundToInt(minutes), roundToMinutes);

        // Empêche toute durée inférieure au minimum, même avant Clamp
        rounded = Mathf.Max(rounded, minMinutes);

        return Mathf.Clamp(rounded, minMinutes, maxMinutes);
    }
    private static int CaloriesFor(int caloriesPerHour, int minutes)
    {
        float cal = caloriesPerHour * (minutes / 60f);
        return Mathf.RoundToInt(cal);
    }

    private int RoundToMultiple(int value, int multiple)
    {
        if (multiple <= 1) return value;
        int mod = Mathf.Abs(value) % multiple;
        if (mod == 0) return value;
        int down = value - mod;
        int up = value + (multiple - mod);
        return (value - down) < (up - value) ? down : up;
    }

    private bool PickTwoDistinctSports(List<SportData> sports, out SportData a, out SportData b)
    {
        a = null;
        b = null;

        if (sports.Count < 2) return false;

        int idxA = -1;
        int idxB = -1;

        if (!weightByRarity)
        {
            idxA = Random.Range(0, sports.Count);
            int attempts = 0;
            do
            {
                idxB = Random.Range(0, sports.Count);
                attempts++;
            } while (idxB == idxA && attempts < maxPickAttempts);
        }
        else
        {
            List<int> weights = new List<int>(sports.Count);
            int total = 0;
            for (int i = 0; i < sports.Count; i++)
            {
                int w = WeightForRarity(sports[i].Rarity);
                weights.Add(w);
                total += w;
            }

            idxA = WeightedPick(weights, total);
            int attempts = 0;
            do
            {
                idxB = WeightedPick(weights, total);
                attempts++;
            } while (idxB == idxA && attempts < maxPickAttempts);
        }

        if (idxA == idxB || idxA < 0 || idxB < 0) return false;

        a = sports[idxA];
        b = sports[idxB];
        return true;
    }

    private int WeightForRarity(AlimentRarity rarity)
    {
        return rarity switch
        {
            AlimentRarity.Commun => 3,
            AlimentRarity.Rare => 2,
            AlimentRarity.TresRare => 1,
            _ => 1
        };
    }

    private int WeightedPick(List<int> weights, int total)
    {
        int r = Random.Range(0, total);
        int cum = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            cum += weights[i];
            if (r < cum) return i;
        }
        return weights.Count - 1;
    }
}