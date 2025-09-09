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

    public static float ToGrams(PortionSelection sel, FoodData food, float defaultPieceWeightG = 120f, float densityGPerMl = 1f)
    {
        switch (sel.Type)
        {
            case FoodPortionType.Unitaire:
                {
                    float pieceWeight = Mathf.Max(1f, defaultPieceWeightG);
                    if (food != null)
                    {
                        if (food.Weight > 0f) pieceWeight = food.Weight;
                        else if (food.Volume > 0f) pieceWeight = food.Volume * Mathf.Max(0.01f, densityGPerMl);
                    }

                    if (sel.Unitaire.HasValue)
                        return Mathf.Max(0f, ToGrams(sel.Unitaire.Value, pieceWeight));

                    return Mathf.Max(0f, pieceWeight);
                }

            case FoodPortionType.PetiteUnite:
                return sel.PetiteUnite.HasValue ? ToGrams(sel.PetiteUnite.Value) : 100f;

            case FoodPortionType.Liquide:
                return sel.Liquide.HasValue
                    ? ToGrams(sel.Liquide.Value, densityGPerMl)
                    : 100f * Mathf.Max(0.01f, densityGPerMl);

            case FoodPortionType.ParPoids:
            default:
                return Mathf.Max(0f, sel.Grams); // Tranche/Default: on prend le poids fourni
        }
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