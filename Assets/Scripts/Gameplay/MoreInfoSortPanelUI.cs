using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class MoreInfoSortPanelUI : MoreInfoPanelBase
{
    [Header("UI")]
    [SerializeField] private Transform itemsParent;   // conteneur vertical/horizontal
    [SerializeField] private FoodItemUI itemPrefab;   // prefab d’item
    [SerializeField] private HorizontalLayoutGroup layoutGroup;


    [Header("Steps / Indicateurs")]
    [SerializeField] private List<GameObject> stepIndicators; // assignés dans l’inspecteur


    public void Show(QuestionData q, int userAnswer)
    {
        // Indicateurs = nb d’aliments
        ApplyStepIndicators(q.Aliments.Count);

        // Ordre correct (solution)
        string correct = q.IndexBonneRéponse.ToString();

        // Ordre joueur
        string player = userAnswer.ToString().PadLeft(correct.Length, '0'); // sécurité sur la longueur

        // Décodage indices solution
        List<int> solutionIdx = new List<int>(correct.Length);
        for (int i = 0; i < correct.Length; i++)
        {
            int idx = (correct[i] - '0') - 1; // "1" → 0, "2" → 1...
            solutionIdx.Add(idx);
        }

        // Instancie les items dans l’ordre de la solution
        for (int i = 0; i < solutionIdx.Count; i++)
        {
            int idx = solutionIdx[i];
            FoodData f = q.Aliments[idx];
            PortionSelection sel = q.PortionSelections[idx];

            // Vérifie si joueur a mis le bon aliment au bon endroit
            bool isCorrectHere = i < player.Length && player[i] == correct[i];

            FoodItemResultState state = isCorrectHere
                ? FoodItemResultState.SelectedCorrect
                : FoodItemResultState.SelectedWrong;

            FoodItemUI ui = Instantiate(itemPrefab, itemsParent);
            ui.name = $"MoreInfo_{i}_{f.Name}";
            ui.Init(f, sel, state, q.SousType);
            
            if (solutionIdx.Count == 4)
            {
                layoutGroup.spacing = -25;
                ui.transform.localScale = Vector3.one * 0.6f;
            }
        }

        // Anim d’intro générique
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