using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

public class DebugFoodGallery : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private FoodDataParser dataParser;
    [SerializeField] private FoodItemUI itemPrefab;
    [SerializeField] private RectTransform scrollViewContent;
    [SerializeField] private GameObject buttonGO;

    [Header("Génération")]
    [SerializeField] private QuestionSubType subType = QuestionSubType.Calorie;

    [Space]
    [Tooltip("Nombre maximum d'unités pour les portions unitaires (1..5).")]
    [SerializeField, Min(1)] private int maxUnitaireCount = 5;
    [SerializeField] private bool includeDemiUnitaire = true;

    [Space]
    [SerializeField] private bool includePetiteUnite = true;

    [Space]
    [SerializeField] private bool includeLiquide = true;

    [Space]
    [SerializeField] private bool includeParPoidsFallback = true;
    [SerializeField] private int[] gramSteps = new[] { 50, 100, 150, 200, 300 };

    [Header("Hypothèses de conversion")]
    [SerializeField] private float defaultPieceWeightG = 120f;
    [SerializeField] private float defaultDensityGPerMl = 1f;

    [Header("Divers")]
    [SerializeField] private bool clearBeforeBuild = true;

    // ------------------ Entry points ------------------

    
    public void Build()
    {
        buttonGO.SetActive(false);
        if (clearBeforeBuild)
            ClearChildren(scrollViewContent);

        List<FoodData> foods = dataParser.GetFoodData();

        List<string> petiteUniteLogs = new List<string>();

        for (int i = 0; i < foods.Count; i++)
        {
            FoodData f = foods[i];
            List<PortionSelection> selections = new List<PortionSelection>(GenerateAllSelections(f));

            if (f.PortionType == FoodPortionType.PetiteUnite)
            {
                foreach (var sel in selections)
                {
                    petiteUniteLogs.Add($"{f.Name} - {sel.PetiteUnite}");
                }
            }

            foreach (PortionSelection sel in selections)
            {
                // Copier la struct pour pouvoir la modifier
                PortionSelection computed = sel;

                float grams = PortionCalculator.ToGrams(computed, f);
                computed.Grams = grams;
                computed.Value = PortionCalculator.ComputeValue(f, grams, subType);

                FoodItemUI ui = Object.Instantiate(itemPrefab, scrollViewContent);
                ui.name = PortionTextFormatter.ToDisplayWithFood(f, computed);
                ui.Init(f, computed, false, subType);
            }
        }

        if (petiteUniteLogs.Count > 0)
        {
            Debug.Log(string.Join("\n", petiteUniteLogs));
        }

        Debug.Log("[DebugFoodGallery] Build terminé.");
    }

    [Button("Clear Content")]
    public void Clear()
    {
        ClearChildren(scrollViewContent);
    }

    // ------------------ Core ------------------

    private IEnumerable<PortionSelection> GenerateAllSelections(FoodData f)
    {
        switch (f.PortionType)
        {
            case FoodPortionType.Unitaire:
                {
                    foreach (PortionSelection s in GenerateUnitaire())
                        yield return s;
                    break;
                }
            case FoodPortionType.Liquide:
                {
                    foreach (PortionSelection s in GenerateLiquide())
                        yield return s;
                    break;
                }
            case FoodPortionType.PetiteUnite:
                {
                    foreach (PortionSelection s in GeneratePetiteUnite())
                        yield return s;
                    break;
                }
            case FoodPortionType.Tranche:
                {
                    foreach (PortionSelection s in GenerateParPoidsFallback(new[] { 30, 50, 100 }))
                        yield return s;
                    break;
                }
            case FoodPortionType.ParPoids:
                {
                    foreach (PortionSelection s in GenerateParPoidsFallback(gramSteps))
                        yield return s;
                    break;
                }
            default:
                {
                    if (includeParPoidsFallback)
                    {
                        foreach (PortionSelection s in GenerateParPoidsFallback(gramSteps))
                            yield return s;
                    }
                    break;
                }
        }
    }

    // ---------- Générateurs de PortionSelection ----------

    private IEnumerable<PortionSelection> GenerateUnitaire()
    {
        if (includeDemiUnitaire)
        {
            PortionSelection demi = new PortionSelection
            {
                Type = FoodPortionType.Unitaire,
                Unitaire = PortionUnitaire.Demi
            };
            yield return demi;
        }

        int max = Mathf.Clamp(maxUnitaireCount, 1, 5);
        for (int i = 1; i <= max; i++)
        {
            PortionUnitaire pu = PortionUnitaire.Un;
            if (i == 2) pu = PortionUnitaire.Deux;
            else if (i == 3) pu = PortionUnitaire.Trois;
            else if (i == 4) pu = PortionUnitaire.Quatre;
            else if (i >= 5) pu = PortionUnitaire.Cinq;

            PortionSelection sel = new PortionSelection
            {
                Type = FoodPortionType.Unitaire,
                Unitaire = pu
            };
            yield return sel;
        }
    }

    private IEnumerable<PortionSelection> GenerateLiquide()
    {
        if (!includeLiquide) yield break;

        PortionSelection cuillere = new PortionSelection
        {
            Type = FoodPortionType.Liquide,
            Liquide = PortionLiquide.CuillereASoupe
        };
        yield return cuillere;

        PortionSelection verre = new PortionSelection
        {
            Type = FoodPortionType.Liquide,
            Liquide = PortionLiquide.Verre
        };
        yield return verre;

        PortionSelection bol = new PortionSelection
        {
            Type = FoodPortionType.Liquide,
            Liquide = PortionLiquide.Bol
        };
        yield return bol;
    }

    private IEnumerable<PortionSelection> GeneratePetiteUnite()
    {
        if (!includePetiteUnite) yield break;

        PortionSelection poignee = new PortionSelection
        {
            Type = FoodPortionType.PetiteUnite,
            PetiteUnite = PortionPetiteUnite.Poignee
        };
        yield return poignee;

        PortionSelection verre = new PortionSelection
        {
            Type = FoodPortionType.PetiteUnite,
            PetiteUnite = PortionPetiteUnite.Verre
        };
        yield return verre;

        PortionSelection bol = new PortionSelection
        {
            Type = FoodPortionType.PetiteUnite,
            PetiteUnite = PortionPetiteUnite.Bol
        };
        yield return bol;

        PortionSelection saladier = new PortionSelection
        {
            Type = FoodPortionType.PetiteUnite,
            PetiteUnite = PortionPetiteUnite.Saladier
        };
        yield return saladier;

        PortionSelection cagette = new PortionSelection
        {
            Type = FoodPortionType.PetiteUnite,
            PetiteUnite = PortionPetiteUnite.Cagette
        };
        yield return cagette;
    }

    private IEnumerable<PortionSelection> GenerateParPoidsFallback(IList<int> gramsList)
    {
        if (!includeParPoidsFallback || gramsList == null) yield break;

        for (int i = 0; i < gramsList.Count; i++)
        {
            int g = gramsList[i];
            if (g <= 0) continue;

            PortionSelection sel = new PortionSelection
            {
                Type = FoodPortionType.ParPoids,
                Grams = g
            };
            yield return sel;
        }
    }

    // ------------------ Utils ------------------

    private static void ClearChildren(Transform root)
    {
        if (root == null) return;
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform c = root.GetChild(i);
            if (Application.isPlaying)
                Object.Destroy(c.gameObject);
            else
                Object.DestroyImmediate(c.gameObject);
        }
    }
}