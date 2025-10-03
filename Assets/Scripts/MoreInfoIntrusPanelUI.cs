using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MoreInfoIntrusPanelUI : MoreInfoPanelBase
{
    [Header("UI")]
    [SerializeField] private Transform goodParent;   // conteneur pour les bons
    [SerializeField] private Transform intrusParent; // conteneur pour les intrus
    [SerializeField] private FoodItemUI itemPrefab;
    [SerializeField] private TMP_Text subtypeLabel;  

    [Header("Steps / Indicateurs")]
    [SerializeField] private List<GameObject> stepIndicators;

    public void Show(QuestionData q, int userAnswer)
    {
        ApplyStepIndicators(q.Aliments.Count);
        ApplySubtypeLabel(q.SousType);

        string correct = q.IndexBonneRéponse.ToString();
        string player = userAnswer.ToString().PadLeft(correct.Length, '0');

        for (int i = 0; i < q.Aliments.Count; i++)
        {
            FoodData f = q.Aliments[i];
            PortionSelection sel = q.PortionSelections != null && i < q.PortionSelections.Count
                ? q.PortionSelections[i]
                : default;

            bool isIntrus = correct[i] == '2';
            bool selectedByPlayer = player[i] == '2';

            FoodItemResultState resultState;

            if (isIntrus && selectedByPlayer)
                resultState = FoodItemResultState.SelectedCorrect;
            else if (!isIntrus && selectedByPlayer)
                resultState = FoodItemResultState.SelectedWrong;
            else if (isIntrus && !selectedByPlayer)
                resultState = FoodItemResultState.MissedCorrect;
            else
                resultState = FoodItemResultState.Neutral;

            Transform parent = isIntrus ? intrusParent : goodParent;

            FoodItemUI ui = Instantiate(itemPrefab, parent);
            ui.name = $"MoreInfo_{i}_{f.Name}";
            ui.Init(f, sel, resultState, q.SousType);
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

    private void ApplySubtypeLabel(QuestionSubType subtype)
    {
        if (subtypeLabel == null) return;

        switch (subtype)
        {
            case QuestionSubType.Proteine:
                subtypeLabel.text = "Aliment Protéiné";
                break;
            case QuestionSubType.Lipide:
                subtypeLabel.text = "Aliment Lipidique";
                break;
            case QuestionSubType.Glucide:
                subtypeLabel.text = "Aliment Glucidique";
                break;
            default:
                subtypeLabel.text = string.Empty;
                break;
        }
    }
}