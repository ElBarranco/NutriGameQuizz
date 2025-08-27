using UnityEngine;
using System.Text;
using System.Globalization;

public static class SpriteLoader
{
    public static Sprite LoadFoodSprite(string foodName)
    {
        return Load(foodName, "FoodIcon");
    }

    public static Sprite LoadSportSprite(string sportName)
    {
        return Load(sportName, "SportIcon");
    }

    public static Sprite Load(string name, string folder)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        string cleanedName = CleanName(name);
        string path = folder + "/" + cleanedName;

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning($"[SpriteLoader] Sprite introuvable : original='{name}', cleaned='{cleanedName}', path='{path}'");
        }

        return sprite;
    }

    private static string CleanName(string input)
    {
        string noAccent = RemoveDiacritics(input);
        return noAccent.ToLower()
                       .Replace(" ", "")
                       .Replace("-", "")
                       .Replace("'", "");
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < normalized.Length; i++)
        {
            char c = normalized[i];
            UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
            if (cat != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}