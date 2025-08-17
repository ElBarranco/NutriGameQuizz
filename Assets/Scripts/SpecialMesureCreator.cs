using System.Collections.Generic;
using UnityEngine;

public class SpecialMeasureManager : MonoBehaviour
{
    [SerializeField] private List<SpecialMeasureData> funMeasures;

    public IReadOnlyList<SpecialMeasureData> FunMeasures => funMeasures;

    private void Awake()
    {
        if (funMeasures == null || funMeasures.Count == 0)
        {
            InitDefaultMeasures();
        }
    }

    private void InitDefaultMeasures()
    {
        funMeasures = new List<SpecialMeasureData>
        {
            Create("Tour Eiffel", 8000000000),
            Create("Frigo", 350),
            Create("Voiture", 3000),
            Create("Camion", 1700),
            Create("Avion", 30000),
            Create("Paquebot", 700000000),
            Create("Appartement", 80000),
            Create("Voilier", 25000),
            Create("Piscine", 2500),
            Create("Valise", 90)
        };
    }

    private SpecialMeasureData Create(string name, long volumeLitres)
    {
        SpecialMeasureData smd = ScriptableObject.CreateInstance<SpecialMeasureData>();
        smd.name = name;
        smd.VolumeLitres = volumeLitres;
        return smd;
    }

    public SpecialMeasureData GetRandomMeasure()
    {
        if (funMeasures == null || funMeasures.Count < 2)
        {
            Debug.LogError("[SpecialMeasureManager] Pas assez de mesures définies.");
            return null;
        }

        return funMeasures[Random.Range(0, funMeasures.Count)];
    }

    public static int GetCaloriesFor(FoodData food, long volumeLitres)
    {
        if (food == null)
        {
            Debug.LogError("[SpecialMeasureManager] FoodData null !");
            return 0;
        }

        if (food.Volume <= 0)
        {
            Debug.LogWarning($"[SpecialMeasureManager] Volume invalide pour {food.Name}, fallback densité = 1.");
            float fallbackWeight = volumeLitres * 1000f;
            return Mathf.RoundToInt((fallbackWeight / 100f) * food.Calories);
        }

        float density = food.Weight / (float)food.Volume; // g/mL
        float totalWeightGrams = density * volumeLitres * 1000f; // g
        float calPer100g = food.Calories;
        float totalCal = (totalWeightGrams / 100f) * calPer100g;
        return Mathf.RoundToInt(totalCal);
    }
}