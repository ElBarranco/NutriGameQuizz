using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class SportDataParser : CsvParserBase
{
    [Header("Source")]
    [SerializeField] private TextAsset csvFileSports;

    [Header("Debug (ReadOnly)")]
    [ReadOnly, SerializeField] private List<SportData> sportDataList;

    private void Awake()
    {
        sportDataList = ParseCSV_Sports();
    }

    public List<SportData> GetSports() => sportDataList;

    private List<SportData> ParseCSV_Sports()
    {
        if (csvFileSports == null || string.IsNullOrWhiteSpace(csvFileSports.text))
        {
            Warn("[SportParser] CSV vide ou manquant.");
            return new List<SportData>();
        }

        var dataList = new List<SportData>();

        int lineIndex = 1; // on saute l'entête, sert aux logs
        foreach (var rawLine in EnumerateCsvLines(csvFileSports.text, skipHeader: true))
        {
            lineIndex++;

            // Détection du séparateur au cas par cas
            char sep = DetectSeparator(rawLine);

            List<string> fields;
            if (sep == ',')
            {
                // Robuste : gère guillemets + virgules internes
                fields = SafeSplitCsvLine(rawLine);
            }
            else
            {
                // Simple : on split sur ';' (CSV FR)
                fields = new List<string>(rawLine.Split(';'));
            }

            // Attendu : 3 colonnes -> Sport, Calories, Rareté
            if (fields.Count < 3)
            {
                Warn($"[SportParser] Ligne invalide (attendu ≥3 colonnes) : {rawLine}");
                continue;
            }

            string name = Clean(fields[0]);
            if (string.IsNullOrEmpty(name))
            {
                Warn($"[SportParser] Nom de sport vide à la ligne {lineIndex}.");
                continue;
            }

            // Calories
            if (!TryParseAndRoundToInt(fields[1], out int calories))
            {
                Warn($"[SportParser] Calories invalides à la ligne {lineIndex} : '{fields[1]}'");
                continue;
            }

            // Rareté (tolérant à la casse et aux espaces)
            if (!TryParseEnumLoose(fields[2], out AlimentRarity rarity, ignoreSpaces: true))
            {
                Warn($"[SportParser] Rareté inconnue à la ligne {lineIndex} : '{fields[2]}'. Utilisation de Commun.");
                rarity = AlimentRarity.Commun;
            }

            var sport = new SportData(name, calories, rarity);
            dataList.Add(sport);
        }

        // Tri alpha par nom
        dataList.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        return dataList;
    }

    /// <summary>
    /// Détecte rapidement le séparateur de la ligne (',' ou ';').
    /// </summary>
    private static char DetectSeparator(string line)
    {
        int comma = 0, semi = 0;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == ',') comma++;
            else if (c == ';') semi++;
        }
        return semi > comma ? ';' : ',';
    }
}