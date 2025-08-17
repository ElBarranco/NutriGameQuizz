using UnityEngine;

public static class PortionCalculator
{
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
        PortionUnitaire.Saladier => 8f * pieceWeightG,
        _ => pieceWeightG
    };

    public static float ToGrams(PortionLiquide p)
    {
        // Conversion via volume (ml) ≈ grammes
        switch (p)
        {
            case PortionLiquide.CuillereASoupe: return 15f;   // ~15 ml -> 15 g
            case PortionLiquide.Verre: return 200f;  // ~200 ml -> 200 g
            case PortionLiquide.Bol: return 350f;  // ~350 ml -> 350 g
            default: return 100f;
        }
    }

}
