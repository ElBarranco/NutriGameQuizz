using UnityEngine;
using System.Text;

public static class FoodSpriteLoader
{
    public static Sprite LoadFoodSprite(string foodName)
    {
        if (string.IsNullOrWhiteSpace(foodName))
            return null;

        string cleanedName = CleanName(foodName);
        return Resources.Load<Sprite>($"FoodIcon/{cleanedName}");
    }

    private static string CleanName(string input)
    {
        string noAccent = RemoveDiacritics(input);
        return noAccent.ToLower().Replace(" ", "").Replace("-", "").Replace("'", "");
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}