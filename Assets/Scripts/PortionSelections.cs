using System;

[Serializable]
public struct PortionSelection
{
    public FoodPortionType Type;              // ParPoids / Unitaire / PetiteUnite
    public float Grams;                       // quantité finale en grammes (pour le calcul)
    public PortionUnitaire? Unitaire;         // si Type == Unitaire, la valeur tirée (Demi/Un/Deux…)
    public PortionPetiteUnite? PetiteUnite;   // si Type == PetiteUnite, la valeur tirée (Bol/Verre/…)
    public PortionLiquide? Liquide;           // si Type == Liquide 
}