using System.Collections.Generic;
using UnityEngine;

public class FoodDatabase : MonoBehaviour
{
    public static FoodDatabase Instance { get; private set; }

    [Header("FullData")]
    [SerializeField] private List<FoodData> allFoods = new List<FoodData>();

    [Header("Catégories filtrées")]
    [SerializeField] private List<FoodData> withSugar = new List<FoodData>();

    private void Awake()
    {
        // Singleton simple
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetFoodData(List<FoodData> parsedList)
    {
        allFoods = parsedList;
        BuildCategories();
    }

    private void BuildCategories()
    {
        withSugar = allFoods.FindAll(f => f.Sugar > 0);
    }

    /// <summary>
    /// Récupère les aliments sucrés (Sugar > 0)
    /// </summary>
    public List<FoodData> GetFoodsWithSugar()
    {
        return withSugar;
    }

    /// <summary>
    /// Accès direct à la liste brute
    /// </summary>
    public List<FoodData> GetAllFoods()
    {
        return allFoods;
    }
}