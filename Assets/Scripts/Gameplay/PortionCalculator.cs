using UnityEngine;

public static class PortionCalculator
{
    // ---------- Conversions -> grammes ----------
    public static float ToGrams(PortionPetiteUnite p) => p switch
    {
        PortionPetiteUnite.Poignee => 30f,
        PortionPetiteUnite.Verre => 200f,
        PortionPetiteUnite.Bol => 350f,
        PortionPetiteUnite.Saladier => 500f,
        PortionPetiteUnite.Cagette => 5000f,
        _ => 100f
    };

    // Conversion “unitaire” -> grammes à partir du poids moyen d’une pièce
    public static float ToGrams(PortionUnitaire p, float pieceWeightG) => p switch
    {
        PortionUnitaire.Demi => 0.5f * pieceWeightG,
        PortionUnitaire.Un => 1f * pieceWeightG,
        PortionUnitaire.Deux => 2f * pieceWeightG,
        PortionUnitaire.Trois => 3f * pieceWeightG,
        PortionUnitaire.Quatre => 4f * pieceWeightG,
        PortionUnitaire.Cinq => 5f * pieceWeightG,
        // ⚠️ "Saladier" n'a pas trop de sens en "Unitaire", garde-le dans PetiteUnite
        _ => pieceWeightG
    };

    // Hypothèse densité ~1 g/ml (eau). Si tu as des densités par aliment, ajoute un paramètre densityGPerMl.
    public static float ToGrams(PortionLiquide p, float densityGPerMl = 1f)
    {
        float ml = p switch
        {
            PortionLiquide.CuillereASoupe => 15f,
            PortionLiquide.Verre => 200f,
            PortionLiquide.Bol => 350f,
            _ => 100f
        };
        return ml * Mathf.Max(0.01f, densityGPerMl);
    }

    // Depuis une PortionSelection (si Grams déjà fixé, on le respecte)
    public static float ToGrams(PortionSelection sel, float defaultPieceWeightG = 120f, float densityGPerMl = 1f)
    {
        // On ne respecte Grams que si la portion est explicitement "ParPoids"
        if (sel.Type == FoodPortionType.ParPoids && sel.Grams > 0f)
            return sel.Grams;

        return sel.Type switch
        {
            FoodPortionType.Unitaire => sel.Unitaire.HasValue
                                            ? ToGrams(sel.Unitaire.Value, defaultPieceWeightG)
                                            : defaultPieceWeightG,

            FoodPortionType.PetiteUnite => sel.PetiteUnite.HasValue
                                            ? ToGrams(sel.PetiteUnite.Value)
                                            : 100f,

            FoodPortionType.Liquide => sel.Liquide.HasValue
                                            ? ToGrams(sel.Liquide.Value, densityGPerMl)
                                            : 100f * densityGPerMl,

            _ /* ParPoids */              => Mathf.Max(0f, sel.Grams)
        };
    }

    // ---------- Calculs nutrition ----------
    public static float GetPer100(FoodData f, QuestionSubType sub) => sub switch
    {
        QuestionSubType.Proteine => f.Proteins,
        QuestionSubType.Glucide => f.Carbohydrates,
        QuestionSubType.Lipide => f.Lipids,
        QuestionSubType.Fibres => f.Fibers,
        _ => f.Calories
    };

    public static float ComputeValue(FoodData food, float grams, QuestionSubType subType)
    {
        float per100 = GetPer100(food, subType); // kcal/g par 100 g
        return per100 * (grams / 100f);
    }




}