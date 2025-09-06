using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

public class FoodDataParser : CsvParserBase
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
            Warn("[Parser] CSV vide ou manquant.");
            MissingSpriteFoodList = new List<FoodData>();
            return new List<FoodData>();
        }

        var dataList = new List<FoodData>();
        MissingSpriteFoodList = new List<FoodData>();

        int lineIndex = 1; // pour logs (car on skip header)
        foreach (var rawLine in EnumerateCsvLines(csvFileFood.text, skipHeader: true))
        {
            lineIndex++;

            // NOTE: split robuste
            var values = SafeSplitCsvLine(rawLine);

            // 12 colonnes : name, type, rarity, portion, weight, volume, calories, proteins, carbs, lipids, fibers, indexGlycemique
            if (values.Count < 12)
            {
                Warn($"Invalid line format (attendu ≥12 colonnes) : {rawLine}");
                continue;
            }

            string name = values[0].Trim();

            if (!TryParseEnumLoose(values[1], out AlimentType type))
            {
                Warn($"Inconnu – AlimentType ligne {lineIndex} : '{values[1]}'");
                continue;
            }

            if (!TryParseEnumLoose(values[2], out AlimentRarity rarity))
            {
                Warn($"Inconnu – Rareté ligne {lineIndex} : '{values[2]}'");
                continue;
            }

            if (!TryParseFoodPortionType(values[3], out FoodPortionType portionType))
            {
                // Warn déjà émis dans la méthode
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
                    sprite = SpriteLoader.LoadFoodSprite(name);
                    if (sprite == null)
                    {
                        MissingSpriteFoodList.Add(foodData);
                        if (EnableDebugIcon)
                            Warn($"Sprite introuvable pour : {name}");

                        if (filterMissingSprites)
                            continue; // on exclut de la liste finale
                    }
                }

                dataList.Add(foodData);
            }
            else
            {
                Warn($"Erreur de parsing numérique à la ligne {lineIndex} : {rawLine}");
            }
        }

        // LOG global des aliments manquants
        if (MissingSpriteFoodList.Count > 0)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"[Parser] {MissingSpriteFoodList.Count} aliments sans sprite :");

            foreach (var food in MissingSpriteFoodList)
                sb.AppendLine($" - {food.Name}");

            Warn(sb.ToString());
        }

        dataList.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        return dataList;
    }

    private bool TryParseFoodPortionType(string input, out FoodPortionType portionType)
    {
        string key = NormalizeKey(input);
        switch (key)
        {
            case "liquide":
                portionType = FoodPortionType.Liquide;
                return true;
            case "unitaire":
                portionType = FoodPortionType.Unitaire;
                return true;
            case "tranche":
                portionType = FoodPortionType.Tranche;
                return true;
            case "petit":
                portionType = FoodPortionType.PetiteUnite;
                return true;
            default:
                Warn($"[Parser] PortionType non reconnu : brute='{input}', normalisé='{key}'");
                portionType = FoodPortionType.Default;
                return false;
        }
    }
}