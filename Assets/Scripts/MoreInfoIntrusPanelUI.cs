using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MoreInfoIntrusPanelUI : MoreInfoPanelBase
{
    [Header("UI")]
    [SerializeField] private Transform goodParent;   // conteneur pour les bons
    [SerializeField] private Transform intrusParent; // conteneur pour les intrus
    [SerializeField] private FoodItemUI itemPrefab;

    [Header("Steps / Indicateurs")]
    [SerializeField] private List<GameObject> stepIndicators;

    public void Show(QuestionData q, int userAnswer)
    {
        // Indicateurs = nb d’aliments
        ApplyStepIndicators(q.Aliments.Count);

        // Solution encodée (ex: 1212)
        string correct = q.IndexBonneRéponse.ToString();
        // Réponse joueur encodée
        string player = userAnswer.ToString().PadLeft(correct.Length, '0');

        // Boucle sur chaque aliment
        for (int i = 0; i < q.Aliments.Count; i++)
        {
            FoodData f = q.Aliments[i];
            PortionSelection sel = q.PortionSelections != null && i < q.PortionSelections.Count
                ? q.PortionSelections[i]
                : default;

            bool isIntrus = correct[i] == '2'; // "2" → intrus
            bool isCorrectHere = i < player.Length && player[i] == correct[i];

            // Choisir le parent (bons ou intrus)
            Transform parent = isIntrus ? intrusParent : goodParent;

            FoodItemUI ui = Instantiate(itemPrefab, parent);
            ui.name = $"MoreInfo_{i}_{f.Name}";
            ui.Init(f, sel, isCorrectHere, q.SousType);
        }

        base.IntroAnim();
    }

    private void ApplyStepIndicators(int activeCount)
    {
        for (int i = 0; i < stepIndicators.Count; i++)
        {
            stepIndicators[i].SetActive(i < activeCount);
        }
    }
}