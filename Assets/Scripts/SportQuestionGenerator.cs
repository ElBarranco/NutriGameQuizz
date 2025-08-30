using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SportCaloriesDualQuestionGenerator : QuestionGenerator
{
    [Header("Dépendances")]
    [SerializeField] private SportDataParser sportDataParser;

    [Header("Sélection aliment")]
    [SerializeField] private bool pickRandomFood = true;
    [SerializeField] private int foodIndexIfNotRandom = 0; // utilisé si pickRandomFood = false

    [Header("Durées (minutes)")]
    [SerializeField] private int minMinutes = 5;
    [SerializeField] private int maxMinutes = 180;
    [SerializeField] private int roundToMinutes = 5;

    [Header("Tirage des sports")]
    [SerializeField] private bool weightByRarity = true; // pondère par la rareté (Commun plus fréquent)
    [SerializeField] private int maxPickAttempts = 30;

    [Header("Robustesse")]
    [SerializeField] private float perfectTolerancePercent = 0.05f; // isPerfect si |diff| <= 5% des kcal cibles

    public QuestionData Generate(List<FoodData> foodList, System.Func<FoodData, PortionSelection> resolvePortionSafe)
    {
        List<SportData> sports = sportDataParser.GetSports();

        // --- 0) Choisir 1 aliment ---
        List<FoodData> picked = base.PickDistinctFoods(foodList, 1);
        FoodData food = picked[0];

        // --- 1) Calculer la portion via la même méthode que tes autres générateurs ---
        // On fixe le sous-type Calories (c’est l’objectif de la question)
        PortionSelection portion = base.ResolvePortion(resolvePortionSafe, food, QuestionSubType.Calorie);

        // Valeur calorique de cette portion 
        float targetCaloriesF = portion.Value;
        Debug.Log("[SportDual] -- -- - - - - - " + targetCaloriesF);

        int targetCalories = Mathf.Max(0, Mathf.RoundToInt(targetCaloriesF));


        // --- 2) Tirer 2 sports distincts ---
        SportData sA;
        SportData sB;
        if (!PickTwoDistinctSports(sports, out sA, out sB))
        {
            Debug.LogWarning("[SportDual] Impossible de tirer 2 sports distincts.");

        }

        // --- 3) Calculer des durées équivalentes en minutes, arrondies ---
        int minutesA = ComputeRoundedMinutesForCalories(targetCalories, sA.Calories);
        int minutesB = ComputeRoundedMinutesForCalories(targetCalories, sB.Calories);

        minutesA = Mathf.Clamp(minutesA, minMinutes, maxMinutes);
        minutesB = Mathf.Clamp(minutesB, minMinutes, maxMinutes);

        // Forcer un écart lisible si trop proches
        if (Mathf.Abs(minutesA - minutesB) < roundToMinutes)
        {
            minutesB = Mathf.Clamp(minutesB + roundToMinutes * 2, minMinutes, maxMinutes);
        }

        // --- 4) Déterminer la meilleure réponse (plus proche de la cible en kcal) ---
        int calA = CaloriesFor(sA.Calories, minutesA);
        int calB = CaloriesFor(sB.Calories, minutesB);

        int diffA = Mathf.Abs(targetCalories - calA);
        int diffB = Mathf.Abs(targetCalories - calB);

        int indexCorrect = diffA <= diffB ? 0 : 1;

        // Mise à jour dans les données
        sA.Duration = minutesA;
        sA.Calories = calA;

        sB.Duration = minutesB;
        sB.Calories = calB;

        // --- 5) Construit le QuestionData dans le même esprit que MealComposition ---
        QuestionData qd = new QuestionData
        {
            Type = QuestionType.Sport,
            SousType = QuestionSubType.Calorie, // cohérent avec la logique de portion
            Aliments = new List<FoodData> { food },
            PortionSelections = new List<PortionSelection> { portion },
            ValeursComparees = new List<float> { targetCalories }, // cible en kcal
            IndexBonneRéponse = indexCorrect,
            Solutions = new List<int> { minutesA, minutesB },     // minutes par option
            SpecialMeasures = new List<SpecialMeasureData>(),      // non utilisé ici
            MealTargetTolerance = 0f,                              // pas utilisé ici
            SportChoices = new List<SportData> { sA, sB }          // références vers les 2 sports
        };

        return qd;
    }

    // --- Helpers principaux ---



    private int ComputeRoundedMinutesForCalories(int targetCalories, int caloriesPerHour)
    {
        if (caloriesPerHour <= 0) return minMinutes;
        float minutes = (targetCalories / (float)caloriesPerHour) * 60f;
        int rounded = RoundToMultiple(Mathf.RoundToInt(minutes), roundToMinutes);
        if (rounded < minMinutes) rounded = minMinutes;
        return rounded;
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
        // plus proche
        return (value - down) < (up - value) ? down : up;
    }

    private bool PickTwoDistinctSports(List<SportData> sports, out SportData a, out SportData b)
    {
        a = null;
        b = null;

        if (!weightByRarity)
        {
            int idxA = Random.Range(0, sports.Count);
            int idxB = idxA;
            int attempts = 0;
            while (idxB == idxA && attempts < maxPickAttempts)
            {
                idxB = Random.Range(0, sports.Count);
                attempts++;
            }
            if (idxB == idxA) return false;

            a = sports[idxA];
            b = sports[idxB];
            return true;
        }
        else
        {
            // Pondération simple : Commun=3, Rare=2, TresRare=1, Extreme=1
            List<int> weights = new List<int>(sports.Count);
            int total = 0;
            for (int i = 0; i < sports.Count; i++)
            {
                int w = WeightForRarity(sports[i].Rarity);
                weights.Add(w);
                total += w;
            }

            int idxA = WeightedPick(weights, total);
            int idxB = idxA;
            int attempts = 0;
            while (idxB == idxA && attempts < maxPickAttempts)
            {
                idxB = WeightedPick(weights, total);
                attempts++;
            }
            if (idxB == idxA) return false;

            a = sports[idxA];
            b = sports[idxB];
            return true;
        }
    }

    private int WeightForRarity(AlimentRarity rarity)
    {
        switch (rarity)
        {
            case AlimentRarity.Commun: return 3;
            case AlimentRarity.Rare: return 2;
            case AlimentRarity.TresRare: return 1;
            default: return 1;
        }
    }

    private int WeightedPick(List<int> weights, int total)
    {
        if (weights == null || weights.Count == 0 || total <= 0)
            return 0;

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