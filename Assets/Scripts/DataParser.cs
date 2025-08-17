using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;
using System.Globalization;

public class FoodDataParser : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private TextAsset csvFileFood;

    [Header("Options")]
    [SerializeField] private bool filterMissingSprites = false;
    [SerializeField] private bool EnableDebugIcon = false;

    [Header("Debug (ReadOnly)")]
    [ReadOnly, SerializeField] private List<FoodData> foodDataList;
    [ReadOnly, SerializeField] private List<FoodData> MissingSpriteFoodList;

    private void Awake()
    {
        foodDataList = ParseCSV_food();
    }

    public List<FoodData> GetFoodData() => foodDataList;
    public IReadOnlyList<FoodData> GetMissingSpriteFoods() => MissingSpriteFoodList;

    private List<FoodData> ParseCSV_food()
    {
        if (csvFileFood == null || string.IsNullOrWhiteSpace(csvFileFood.text))
        {
            Debug.LogWarning("[Parser] CSV vide ou manquant.");
            MissingSpriteFoodList = new List<FoodData>();
            return new List<FoodData>();
        }

        string[] lines = csvFileFood.text.Split('\n');
        var dataList = new List<FoodData>();
        MissingSpriteFoodList = new List<FoodData>();

        for (int i = 1; i < lines.Length; i++) // skip header
        {
            var rawLine = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            var values = rawLine.Split(',');

            // 12 colonnes : name, type, rarity, portion, weight, volume, calories, proteins, carbs, lipids, fibers, indexGlycemique
            if (values.Length < 12)
            {
                Debug.LogWarning($"Invalid line format (attendu ≥12 colonnes) : {rawLine}");
                continue;
            }

            string name = values[0].Trim();

            if (!TryParseAlimentType(values[1], out AlimentType type))
            {
                Debug.LogWarning($"Inconnu – AlimentType ligne {i} : '{values[1]}'");
                continue;
            }

            if (!TryParseAlimentRarity(values[2], out AlimentRarity rarity))
            {
                Debug.LogWarning($"Inconnu – Rareté ligne {i} : '{values[2]}'");
                continue;
            }

            if (!TryParseFoodPortionType(values[3], out FoodPortionType portionType))
            {
                continue;
            }

            // Numériques
            if (TryParseAndRoundToInt(values[4], out int weight)
                && TryParseAndRoundToInt(values[5], out int volume)
                && TryParseAndRoundToInt(values[6], out int calories)
                && TryParseFloatFlexible(values[7], out float proteins)
                && TryParseFloatFlexible(values[8], out float carbohydrates)
                && TryParseFloatFlexible(values[9], out float lipids)
                && TryParseFloatFlexible(values[10], out float fibers)
                && TryParseAndRoundToInt(values[11], out int indexGlycemique))
            {
                var foodData = new FoodData(
                    name, type, rarity, portionType,
                    weight, volume, calories,
                    proteins, carbohydrates, lipids, fibers,
                    indexGlycemique
                );

                // Vérif sprite (une seule fois)
                Sprite sprite = null;
                if (filterMissingSprites || EnableDebugIcon)
                {
                    sprite = FoodSpriteLoader.LoadFoodSprite(name);
                    if (sprite == null)
                    {
                        MissingSpriteFoodList.Add(foodData);
                        if (EnableDebugIcon)
                            Debug.LogWarning($"Sprite introuvable pour : {name}");

                        if (filterMissingSprites)
                            continue; // on exclut de la liste finale
                    }
                }

                dataList.Add(foodData);
            }
            else
            {
                Debug.LogWarning($"Erreur de parsing numérique à la ligne {i} : {rawLine}");
            }
        }

        // LOG global des aliments manquants
        if (MissingSpriteFoodList.Count > 0)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"[Parser] {MissingSpriteFoodList.Count} aliments sans sprite :");

            foreach (var food in MissingSpriteFoodList)
                sb.AppendLine($" - {food.Name}");

            Debug.LogWarning(sb.ToString());
        }


        dataList.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        return dataList;
    }

    private bool TryParseFoodPortionType(string input, out FoodPortionType portionType)
    {
        string key = input.Trim().ToLowerInvariant();
        switch (key)
        {
            case "liquide":
                // NOTE: si ton enum prévoit ParVolume pour les liquides, remplace par ParVolume ici.
                portionType = FoodPortionType.ParPoids;
                return true;
            case "unitaire":
                portionType = FoodPortionType.Unitaire;
                return true;
            case "petit":
                portionType = FoodPortionType.PetiteUnite;
                return true;
            default:
                Debug.LogWarning($"[Parser] PortionType non reconnu : brute='{input}', normalisé='{key}'");
                portionType = FoodPortionType.Default;
                return false;
        }
    }

    private bool TryParseAlimentType(string input, out AlimentType type)
    {
        bool success = Enum.TryParse(input.Trim(), true, out type);
        if (!success)
            Debug.LogWarning($"[Parser] Type d'aliment non reconnu : \"{input}\"");
        return success;
    }

    private bool TryParseAlimentRarity(string input, out AlimentRarity rarity)
    {
        bool success = Enum.TryParse(input.Replace(" ", "").Trim(), true, out rarity);
        if (!success)
            Debug.LogWarning($"[Parser] Rareté d'aliment non reconnue : \"{input}\"");
        return success;
    }

    private bool TryParseAndRoundToInt(string input, out int result)
    {
        input = input.Trim().TrimEnd('\r');
        if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            return true;

        if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
        {
            result = Mathf.RoundToInt(f);
            return true;
        }

        result = 0;
        return false;
    }

    private bool TryParseFloatFlexible(string input, out float result)
    {
        input = input.Trim().TrimEnd('\r');
        if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            return true;

        var swapped = input.Replace(',', '.');
        if (float.TryParse(swapped, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            return true;

        result = 0f;
        return false;
    }
}