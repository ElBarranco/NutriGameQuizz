using UnityEngine;
public static class NutritionCalculator
{
    public static int CaloriesForVolume(FoodData food, long volumeLitres)
    {
        float grams = volumeLitres * 1000f;
        return Mathf.RoundToInt((grams / 100f) * food.Calories);
    }
}