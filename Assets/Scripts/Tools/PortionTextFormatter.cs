using UnityEngine;
public static class PortionTextFormatter
{
    public static string ToText(PortionSelection sel)
    {
        switch (sel.Type)
        {
            case FoodPortionType.ParPoids:
                return $"{Mathf.RoundToInt(sel.Grams)} g";

            case FoodPortionType.Unitaire:
                if (sel.Unitaire.HasValue)
                {
                    switch (sel.Unitaire.Value)
                    {
                        case PortionUnitaire.Demi: return "Un demi";
                        case PortionUnitaire.Un: return "Un";
                        case PortionUnitaire.Deux: return "2";
                        case PortionUnitaire.Trois: return "3";
                        case PortionUnitaire.Quatre: return "4";
                        case PortionUnitaire.Cinq: return "5";
                        case PortionUnitaire.Saladier: return "Un saladier";
                    }
                }
                return "Un";

            case FoodPortionType.PetiteUnite:
                if (sel.PetiteUnite.HasValue)
                {
                    switch (sel.PetiteUnite.Value)
                    {
                        case PortionPetiteUnite.Poignee: return "Une poignée";
                        case PortionPetiteUnite.Verre: return "Un verre";
                        case PortionPetiteUnite.Bol: return "Un bol";
                        case PortionPetiteUnite.Saladier: return "Un saladier";
                        case PortionPetiteUnite.Cagette: return "Une cagette";
                    }
                }
                return $"{Mathf.RoundToInt(sel.Grams)} g";

            case FoodPortionType.Liquide: // NEW
                if (sel.Liquide.HasValue)
                {
                    switch (sel.Liquide.Value)
                    {
                        case PortionLiquide.CuillereASoupe: return "Une cuillère à soupe";
                        case PortionLiquide.Verre: return "Un verre";
                        case PortionLiquide.Bol: return "Un bol";
                    }
                }
                return $"{Mathf.RoundToInt(sel.Grams)} ml";

            default:
                return $"{Mathf.RoundToInt(sel.Grams)} g";
        }
    }

    public static string ToDisplayWithFood(FoodData food, PortionSelection sel)
    {
        string name = food?.Name ?? "aliment";

        switch (sel.Type)
        {
            case FoodPortionType.Unitaire:
                {
                    if (sel.Unitaire.HasValue && sel.Unitaire.Value == PortionUnitaire.Saladier)
                        return $"Un saladier {DeOrD(name)}{name}";
                    var (countText, count) = CountText(sel.Unitaire);
                    if (sel.Unitaire.HasValue && sel.Unitaire.Value == PortionUnitaire.Demi)
                        return $"Un demi {name}";
                    return $"{countText} {Pluralize(name, count)}";
                }

            case FoodPortionType.PetiteUnite:
                {
                    string cont = ToText(sel);
                    return $"{cont} {DeOrD(name)}{name}";
                }

            case FoodPortionType.Liquide: // NEW
                {
                    string cont = ToText(sel);
                    return $"{cont} {DeOrD(name)}{name}";
                }

            case FoodPortionType.ParPoids:
            default:
                {
                    string grams = $"{Mathf.RoundToInt(sel.Grams)} g";
                    return $"{grams} {DeOrD(name)}{name}";
                }
        }
    }

    // Helpers CountText / DeOrD / Pluralize (identiques à ta version actuelle)…
    private static (string text, float count) CountText(PortionUnitaire? uni)
    {
        if (!uni.HasValue) return ("Un", 1f);
        switch (uni.Value)
        {
            case PortionUnitaire.Demi: return ("Un demi", 0.5f);
            case PortionUnitaire.Un: return ("Un", 1f);
            case PortionUnitaire.Deux: return ("2", 2f);
            case PortionUnitaire.Trois: return ("3", 3f);
            case PortionUnitaire.Quatre: return ("4", 4f);
            case PortionUnitaire.Cinq: return ("5", 5f);
            default: return ("Un", 1f);
        }
    }

    private static string DeOrD(string word)
    {
        if (string.IsNullOrEmpty(word)) return "de ";
        char c = char.ToLowerInvariant(word[0]);
        return (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' || c == 'h') ? "d’" : "de ";
    }

    private static string Pluralize(string name, float count)
    {
        if (count < 2f) return name;
        if (string.IsNullOrEmpty(name)) return name;
        char last = char.ToLowerInvariant(name[name.Length - 1]);
        if (last == 's' || last == 'x' || last == 'z') return name;
        return name + "s";
    }

    public static string UnitForQuestion(QuestionSubType type)
    {
        switch (type)
        {
            case QuestionSubType.Calorie:
                return "kcal";

            case QuestionSubType.Proteine:
            case QuestionSubType.Glucide:
            case QuestionSubType.Lipide:
            case QuestionSubType.Fibres:
                return "g";

            case QuestionSubType.Libre:
            default:
                return "";
        }
    }
}