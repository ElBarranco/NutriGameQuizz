using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [SerializeField] public DifficultyLevel currentDifficulty = DifficultyLevel.Easy;
    public DifficultyLevel CurrentDifficulty => currentDifficulty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Optionnel si tu veux piloter en runtime
    public void SetDifficulty(DifficultyLevel level) => currentDifficulty = level;

    /// <summary>
    /// Filtrage des aliments selon la difficulté (logique existante conservée).
    /// </summary>
    public List<FoodData> FilterFoods(List<FoodData> allFoods)
    {
        return allFoods.FindAll(food =>
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:
                    return food.Rarity == AlimentRarity.Commun;

                case DifficultyLevel.Medium:
                    return food.Rarity == AlimentRarity.Commun || food.Rarity == AlimentRarity.Rare;

                case DifficultyLevel.Hard:
                    return true;

                default:
                    return true;
            }
        });
    }

    public static DifficultyLevel GetDifficulty()
    {
        return Instance != null ? Instance.CurrentDifficulty : DifficultyLevel.Easy;
    }



}