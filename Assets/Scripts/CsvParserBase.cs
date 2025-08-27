using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class CsvParserBase : MonoBehaviour
{
    /// <summary>
    /// Trim standard + enlève \r final éventuel.
    /// </summary>
    protected static string Clean(string input)
    {
        if (input == null) return string.Empty;
        return input.Trim().TrimEnd('\r');
    }

    /// <summary>
    /// Normalisation basique pour clés (lower-invariant, no accent handling ici).
    /// </summary>
    protected static string NormalizeKey(string input)
    {
        return Clean(input).ToLowerInvariant();
    }

    /// <summary>
    /// Parse int avec fallback float->round (InvariantCulture). 
    /// </summary>
    protected static bool TryParseAndRoundToInt(string input, out int result)
    {
        input = Clean(input);

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

    /// <summary>
    /// Parse float en acceptant '.' et ',' (InvariantCulture).
    /// </summary>
    protected static bool TryParseFloatFlexible(string input, out float result)
    {
        input = Clean(input);

        if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            return true;

        var swapped = input.Replace(',', '.');
        if (float.TryParse(swapped, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            return true;

        result = 0f;
        return false;
    }


    protected static bool TryParseEnumLoose<TEnum>(string input, out TEnum value, bool ignoreSpaces = true)
        where TEnum : struct
    {
        string raw = Clean(input);
        if (ignoreSpaces) raw = raw.Replace(" ", "");

        bool success = Enum.TryParse(raw, true, out value);
        return success;
    }


    protected static List<string> SafeSplitCsvLine(string line)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(line))
        {
            result.Add(string.Empty);
            return result;
        }

        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Double guillemet -> guillemet échappé
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Length = 0;
            }
            else
            {
                sb.Append(c);
            }
        }

        result.Add(sb.ToString());
        return result;
    }

    /// <summary>
    /// Découpe le CSV (par lignes) en ignorant l’en-tête si demandé.
    /// </summary>
    protected static IEnumerable<string> EnumerateCsvLines(string csvText, bool skipHeader)
    {
        if (string.IsNullOrWhiteSpace(csvText))
            yield break;

        string[] lines = csvText.Split('\n');
        int start = skipHeader ? 1 : 0;

        for (int i = start; i < lines.Length; i++)
        {
            string raw = lines[i].Trim();
            if (!string.IsNullOrWhiteSpace(raw))
                yield return raw;
        }
    }

    /// <summary>
    /// Utilitaire de log warning formaté.
    /// </summary>
    protected static void Warn(string message)
    {
        Debug.LogWarning(message);
    }
}