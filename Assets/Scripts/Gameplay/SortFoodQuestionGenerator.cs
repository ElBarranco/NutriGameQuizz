using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Génère une question de type TRI : trier des aliments du - au + calorique (ou autre sous-type).
/// Génération ONLY (pas d’UI).
/// </summary>
public class SortFoodQuestionGenerator : QuestionGenerator
{
    [Header("Paramètres")]
    [SerializeField, Min(3)] private int minCount = 3;
    [SerializeField, Range(3, 4)] private int maxCount = 4;
    [SerializeField, Tooltip("Delta minimal entre 2 aliments adjacents en termes de valeur (ex: kcal).")]
    private float minDeltaBetweenAdjacent = 20f;
    [SerializeField, Tooltip("Nombre maximum d’essais aléatoires pour trouver un set valide.")]
    private int maxAttempts = 80;


    public QuestionData Generate(
        List<FoodData> foods,
        Func<FoodData, PortionSelection> portionResolver,
DifficultyLevel currentDifficulty,
        int count = -1

    )
    {
        int target = GetTargetFoodCount(count, currentDifficulty);
        QuestionSubType subType = base.GetRandomQuestionSubTypeWithDropRates(currentDifficulty);


        // 1) Pré-calcul des valeurs via le resolver
        List<(FoodData food, PortionSelection sel, float value)> candidates =
            new List<(FoodData, PortionSelection, float)>();

        foreach (FoodData f in foods)
        {
            try
            {
                PortionSelection sel = portionResolver != null ? portionResolver.Invoke(f) : default;
                float v = sel.Value; // on suppose que PortionSelection expose Value (float)
                if (float.IsNaN(v) || float.IsInfinity(v) || v <= 0f) continue;

                candidates.Add((f, sel, v));
            }
            catch
            {
                // on ignore les erreurs éventuelles du resolver
            }
        }

        if (candidates.Count < target)
        {
            Debug.LogWarning($"[SortFoodQuestionGenerator] Pas assez de candidats valides (={candidates.Count}) pour {target} items.");
            return null;
        }

        // 2) Chercher un set avec spreads suffisants
        System.Random rng = new System.Random();
        List<(FoodData food, PortionSelection sel, float value)> picked = null;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Shuffle
            List<(FoodData food, PortionSelection sel, float value)> shuffled =
                candidates.OrderBy(_ => rng.Next()).ToList();

            List<(FoodData food, PortionSelection sel, float value)> trial =
                new List<(FoodData, PortionSelection, float)>();

            foreach ((FoodData food, PortionSelection sel, float value) c in shuffled)
            {
                trial.Add(c);

                if (trial.Count > target)
                {
                    int removeIndex = rng.Next(trial.Count);
                    trial.RemoveAt(removeIndex);
                }

                if (trial.Count == target)
                {
                    if (IsSpreadEnough(trial, minDeltaBetweenAdjacent))
                    {
                        picked = new List<(FoodData, PortionSelection, float)>(trial);
                        break;
                    }
                }
            }

            if (picked != null) break;
        }

        // 3) Fallback si le delta minimal est introuvable
        if (picked == null)
        {
            picked = BestSpreadFallback(candidates, target);
        }

        // 4) Préparer l’ordre d’affichage (aléatoire)
        List<(FoodData food, PortionSelection sel, float value)> displayOrder =
            picked.OrderBy(_ => rng.Next()).ToList();

        // 5) Solution = liste d’index des aliments dans displayOrder, triés par valeur croissante
        List<int> solutionIndexes = picked
            .OrderBy(t => t.value)
            .Select(t => displayOrder.FindIndex(x => x.food == t.food) + 1) // +1 ici
            .ToList();

        // Concatène les indices en string
        string concat = string.Concat(solutionIndexes);
        int ordreEncode = int.Parse(concat);

        // 6) Remplir QuestionData
        QuestionData q = new QuestionData
        {
            Type = QuestionType.Tri,
            SousType = QuestionSubType.Calorie,
            Aliments = displayOrder.Select(t => t.food).ToList(),
            PortionSelections = displayOrder.Select(t => t.sel).ToList(),

            // ✅ La solution (ordre correct) est stockée sous forme d'index (cast en float)
            ValeursComparees = solutionIndexes.Select(i => (float)i).ToList(),
            IndexBonneRéponse = ordreEncode,
            SortSolution = EncodeOrderString(solutionIndexes),
        };

        return q;
    }




    private int GetTargetFoodCount(int count, DifficultyLevel difficulty)
    {
        if (difficulty == DifficultyLevel.Easy)
        {
            return 3;
        }

        if (count < 0)
        {
            return UnityEngine.Random.Range(minCount, maxCount + 1);
        }

        return Mathf.Clamp(count, 3, 4);
    }
    private static string EncodeOrderString(List<int> order)
    {
        return string.Concat(order); // ex: [0,2,3] -> "023"
    }


    /// <summary>
    /// Vérifie que, une fois triée par value, la liste respecte un delta minimal entre adjacents.
    /// </summary>
    private static bool IsSpreadEnough(List<(FoodData food, PortionSelection sel, float value)> items, float minDelta)
    {
        List<(FoodData food, PortionSelection sel, float value)> sorted = items.OrderBy(t => t.value).ToList();
        for (int i = 1; i < sorted.Count; i++)
        {
            if (Mathf.Abs(sorted[i].value - sorted[i - 1].value) < minDelta)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Choisit le meilleur spread possible quand on ne peut pas tenir le minDelta.
    /// </summary>
    private static List<(FoodData food, PortionSelection sel, float value)> BestSpreadFallback(
        List<(FoodData food, PortionSelection sel, float value)> candidates,
        int target)
    {
        List<(FoodData food, PortionSelection sel, float value)> sorted = candidates.OrderBy(t => t.value).ToList();

        if (sorted.Count <= target)
            return new List<(FoodData, PortionSelection, float)>(sorted);

        List<(FoodData food, PortionSelection sel, float value)> result =
            new List<(FoodData, PortionSelection, float)>
            {
                sorted.First(),
                sorted.Last()
            };

        while (result.Count < target)
        {
            float currentMin = result.Min(t => t.value);
            float currentMax = result.Max(t => t.value);

            (FoodData food, PortionSelection sel, float value)? best = null;
            float bestScore = float.MinValue;

            foreach ((FoodData food, PortionSelection sel, float value) c in sorted)
            {
                if (result.Contains(c)) continue;

                float d = Math.Min(Mathf.Abs(c.value - currentMin), Mathf.Abs(c.value - currentMax));
                if (d > bestScore)
                {
                    bestScore = d;
                    best = c;
                }
            }

            if (best.HasValue)
            {
                result.Add(best.Value);
            }
            else
            {
                break;
            }
        }

        return result;
    }
}