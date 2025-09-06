using UnityEngine;
using System;

[System.Serializable]
public class SportData
{
    public string Name;                // ex: "Surf"
    public int CaloriesPerHour;       // kcal / heure (arrondi)
    [NonSerialized] public int Duration;
    [NonSerialized] public int Calories;
    public AlimentRarity Rarity;      // Commun/Rare/TresRare/Extreme

    public SportData(string name, int calories, AlimentRarity rarity)
    {
        Name = name;
        CaloriesPerHour = calories;
        Rarity = rarity;
    }

    // ✅ Constructeur de copie
    public SportData(SportData other)
    {
        this.Name = other.Name;
        this.CaloriesPerHour = other.CaloriesPerHour;
        this.Rarity = other.Rarity;
        this.Duration = other.Duration;
        this.Calories = other.Calories;
    }
}