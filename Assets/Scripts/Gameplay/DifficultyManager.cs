using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Easy;
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


    public PortionSelection ResolvePortion(FoodPortionType type, float avgPieceWeightG = 120f)
    {
        var sel = new PortionSelection { Type = type, Grams = 100f };

        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                if (type == FoodPortionType.ParPoids)
                {
                    sel.Grams = 100f;
                }
                else if (type == FoodPortionType.Unitaire)
                {
                    sel.Unitaire = PortionUnitaire.Un;
                    sel.Grams = PortionCalculator.ToGrams(PortionUnitaire.Un, avgPieceWeightG);
                }
                else if (type == FoodPortionType.PetiteUnite)
                {
                    sel.PetiteUnite = PortionPetiteUnite.Bol;
                    sel.Grams = PortionCalculator.ToGrams(PortionPetiteUnite.Bol);
                }
                break;

            case DifficultyLevel.Medium:
            case DifficultyLevel.Hard:
                if (type == FoodPortionType.ParPoids)
                {
                    sel.Grams = (currentDifficulty == DifficultyLevel.Medium) ? 150f : 80f;
                }
                else if (type == FoodPortionType.Unitaire)
                {
                    // tirage aléatoire sur l’enum (comme demandé)
                    var values = (PortionUnitaire[])System.Enum.GetValues(typeof(PortionUnitaire));
                    var pick = values[UnityEngine.Random.Range(0, values.Length)];
                    sel.Unitaire = pick;
                    sel.Grams = PortionCalculator.ToGrams(pick, avgPieceWeightG);
                }
                else if (type == FoodPortionType.PetiteUnite)
                {
                    var pick = (currentDifficulty == DifficultyLevel.Medium) ? PortionPetiteUnite.Verre
                                                                             : PortionPetiteUnite.Poignee;
                    sel.PetiteUnite = pick;
                    sel.Grams = PortionCalculator.ToGrams(pick);
                }
                break;
        }

        return sel;
    }

    private float RandomUnitaire(float avgPieceWeightG)
    {
        // Tire une valeur aléatoire dans l'énum PortionUnitaire
        var values = (PortionUnitaire[])System.Enum.GetValues(typeof(PortionUnitaire));
        var pick = values[UnityEngine.Random.Range(0, values.Length)];
        return PortionCalculator.ToGrams(pick, avgPieceWeightG);
    }
}