using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class QuestionGenerator : MonoBehaviour
{
    [Header("Defaults / Utils")]
    [SerializeField] protected DifficultyManager difficultyManager;
    [SerializeField] protected float defaultPieceWeightG = 120f; // utilisé pour Unitaire si poids pièce inconnu

    // ---------- Helpers communs ----------

    //Selection une portion en fonction du type (unitaire etc...)
    protected PortionSelection PickRandomPortionVariant(PortionSelection sel)
    {
        switch (sel.Type)
        {
            case FoodPortionType.Unitaire:
                sel.Unitaire = (PortionUnitaire)Random.Range((int)PortionUnitaire.Demi, (int)PortionUnitaire.Cinq + 1);
                break;
            case FoodPortionType.PetiteUnite:
                sel.PetiteUnite = (PortionPetiteUnite)Random.Range(0, (int)PortionPetiteUnite.Cagette + 1);
                break;
            case FoodPortionType.Liquide:
                sel.Liquide = (PortionLiquide)Random.Range(0, (int)PortionLiquide.Bol + 1);
                break;
            case FoodPortionType.ParPoids:
            default:
                break;
        }
        return sel;
    }

    protected PortionSelection ResolvePortionSafe(FoodData food, QuestionSubType subType)
    {
        PortionSelection sel;


        sel = new PortionSelection { Type = food.PortionType, Grams = 100f };

        switch (food.PortionType)
        {
            case FoodPortionType.Unitaire:
                // Si rien déjà fixé, on tire une valeur au hasard (demi..cinq)
                if (!sel.Unitaire.HasValue)
                {
                    int min = (int)PortionUnitaire.Demi;
                    int max = (int)PortionUnitaire.Cinq + 1; // +1 car exclusif
                    sel.Unitaire = (PortionUnitaire)Random.Range(min, max);
                }
                break;

            case FoodPortionType.PetiteUnite:
                if (!sel.PetiteUnite.HasValue)
                {
                    int min = 0;
                    int max = (int)PortionPetiteUnite.Cagette + 1;
                    sel.PetiteUnite = (PortionPetiteUnite)Random.Range(min, max);
                }
                break;

            case FoodPortionType.Liquide:
                if (!sel.Liquide.HasValue)
                {
                    int min = 0;
                    int max = (int)PortionLiquide.Bol + 1;
                    sel.Liquide = (PortionLiquide)Random.Range(min, max);
                }
                break;
        }


        // 2) Paramètres physiques (poids unitaire + densité)
        float pieceWeight = (food.Weight > 0f) ? food.Weight : defaultPieceWeightG;
        float density = (food.Volume > 0f)
            ? Mathf.Max(0.01f, food.Weight / Mathf.Max(0.01f, food.Volume))
            : 1f;

        // 3) Harmonisation en grammes RÉELS selon le type de portion choisi
        //    (n'écrase pas si Grams > 0 déjà fourni)
        sel.Grams = PortionCalculator.ToGrams(sel, pieceWeight, density);

        // 4) Valeur nutritionnelle selon le sous-type (Calorie / Prot / Gluc / …)
        sel.Value = PortionCalculator.ComputeValue(food, sel.Grams, subType);

        // 🔎 Log clair et unique
        Debug.Log($"[ResolvePortionSafe] Food={food.Name}, Type={sel.Type}, Portion={(sel.Unitaire ?? (object)sel.PetiteUnite ?? sel.Liquide) ?? "N/A"}, Grams={sel.Grams:F1}, Value={sel.Value:F1} ({subType})");


        return sel;
    }

    protected List<FoodData> PickDistinctFoods(List<FoodData> source, int count)
    {
        int capacity = Mathf.Min(count, source.Count);
        List<FoodData> result = new List<FoodData>(capacity);
        List<FoodData> pool = new List<FoodData>(source);
        int n = Mathf.Min(count, pool.Count);
        for (int i = 0; i < n; i++)
        {
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return result;
    }

    protected HashSet<int> PickUniqueIndices(int pickCount, int maxExclusive)
    {
        HashSet<int> set = new HashSet<int>();
        while (set.Count < pickCount)
            set.Add(Random.Range(0, maxExclusive));
        return set;
    }

    protected float Snap(float v, float step)
    {
        if (step <= 0f) return v;
        return Mathf.Round(v / step) * step;
    }


    /// Tire un sous-type parmi (Calorie, Protéine, Glucide, éventuellement Lipide/Fibres).

    protected QuestionSubType GetRandomSubType(bool includeLipides = false, bool includeFibres = false)
    {
        List<QuestionSubType> bag = new List<QuestionSubType> { QuestionSubType.Calorie, QuestionSubType.Proteine, QuestionSubType.Glucide };
        if (includeLipides) bag.Add(QuestionSubType.Lipide);
        if (includeFibres) bag.Add(QuestionSubType.Fibres);
        int idx = Random.Range(0, bag.Count);
        return bag[idx];
    }


    /// Résout une portion via le délégué 1-arg (pour obtenir un sel de base), normalise les grammes et calcule 'Value' pour le sous-type.

    protected PortionSelection ResolvePortion(Func<FoodData, PortionSelection> resolver, FoodData food, QuestionSubType subType)
    {
        PortionSelection sel = resolver != null
            ? resolver(food)
            : new PortionSelection { Type = food.PortionType, Grams = 100f };

        // Normaliser les grammes si la portion est symbolique (Unitaire/PetiteUnite/Liquide)
        sel.Grams = PortionCalculator.ToGrams(sel, defaultPieceWeightG);
        // Calculer la valeur par portion pour le sous-type demandé
        sel.Value = PortionCalculator.ComputeValue(food, sel.Grams, subType);
        return sel;
    }




    /// Vérifie qu’aucun autre sous-ensemble que 'picked' ne matche la cible (± tol).
    protected bool IsUniqueSubsetSum(List<float> itemValues, HashSet<int> picked, float target, float tol)
    {
        int n = itemValues.Count;
        int pickedMask = 0;
        for (int i = 0; i < n; i++)
            if (picked.Contains(i))
                pickedMask |= (1 << i);

        int maxMask = 1 << n;
        for (int mask = 1; mask < maxMask; mask++)
        {
            if (mask == pickedMask) continue;
            float sum = 0f;
            for (int i = 0; i < n; i++)
                if ((mask & (1 << i)) != 0)
                    sum += itemValues[i];

            if (Mathf.Abs(Snap(sum, 1f) - target) <= tol)
                return false;
        }
        return true;
    }
}