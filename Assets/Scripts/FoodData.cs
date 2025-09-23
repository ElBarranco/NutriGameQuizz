using UnityEngine;

[System.Serializable]
public class FoodData
{
    public string Name;
    public AlimentType Type;
    public FoodPortionType PortionType;
    public AlimentRarity Rarity;

    public int Weight;
    public int Volume;
    public int Calories;
    public float Proteins;
    public float Carbohydrates;
    public FoodCategory MainCategory;
    public float Lipids;
    public float Fibers;
    public int IndexGlycemique;
    public float Sugar;

    public int Quantity = 1;

    public FoodData(string name, AlimentType type, AlimentRarity rarity, FoodPortionType portionType,
        int weight, int volume, int calories, float proteins, float carbohydrates, float lipids, float fibers, int indexGlycemique, float sugar, FoodCategory mainCategory)
    {
        Name = name;
        Type = type;
        Rarity = rarity;
        PortionType = portionType;
        Weight = weight;
        Volume = volume;
        Calories = calories;
        Proteins = proteins;
        Carbohydrates = carbohydrates;
        Lipids = lipids;
        Fibers = fibers;
        IndexGlycemique = indexGlycemique;
        Sugar = sugar;
        MainCategory = mainCategory;
    }
}