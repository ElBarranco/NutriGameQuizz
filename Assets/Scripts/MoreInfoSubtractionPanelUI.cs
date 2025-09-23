using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MoreInfoSubtractionPanelUI : MoreInfoPanelBase
{
    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private FoodItemUI itemPrefab;   


    public void Show(QuestionData q, int userAnswer)
    {


        // ⚡ Solutions correctes
        HashSet<int> correctSolutions = new HashSet<int>(q.Solutions);

        // ⚡ Réponse joueur décodée (concaténée → ex: 13)
        HashSet<int> playerSolutions = new HashSet<int>();
        string playerStr = userAnswer.ToString();
        foreach (char c in playerStr)
        {
            if (char.IsDigit(c))
                playerSolutions.Add(int.Parse(c.ToString()));
        }

        // Boucle sur chaque aliment
        for (int i = 0; i < q.Aliments.Count; i++)
        {
            FoodData f = q.Aliments[i];
            PortionSelection sel = (q.PortionSelections != null && i < q.PortionSelections.Count)
                ? q.PortionSelections[i]
                : default;

            int id = i + 1;

            bool isIntrus = correctSolutions.Contains(id);
            bool isSelectedByPlayer = playerSolutions.Contains(id);
            bool isCorrectHere = playerSolutions.Contains(id) && isIntrus;

            // ✅ On instancie ton prefab FoodItemUI
            FoodItemUI ui = Instantiate(itemPrefab, contentParent);
            ui.name = $"{f.Name}_{i}";
            ui.Init(f, sel, isCorrectHere, q.SousType, isSelectedByPlayer); 
        }

        base.IntroAnim();
    }
}