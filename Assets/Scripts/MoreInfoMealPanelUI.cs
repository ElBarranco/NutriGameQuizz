using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MoreInfoMealPanelUI : MoreInfoPanelBase
{
    [Header("Header")]
    [SerializeField] private TextMeshProUGUI targetText;    // ex : "950 kcal" ou "45 g"
    [SerializeField] private TextMeshProUGUI userTotalText; // ex : "910 kcal" ou "40 g"

    [Header("Liste aliments")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private GameObject itemPrefab;

    [Header("Couleurs / Feedback")]
    [SerializeField] private Color nearTargetColor = new Color(0.3f, 0.8f, 0.3f);
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private int nearThreshold = 50; // en unités (kcal ou g)

    private readonly List<GameObject> _spawned = new List<GameObject>();

    /// <summary>
    /// Affiche le panel de récapitulatif de composition de repas.
    /// </summary>
    /// <param name="foods">Liste des aliments sélectionnés.</param>
    /// <param name="portions">Portions correspondantes (sel.Value contient déjà la valeur adaptée au sous-type).</param>
    /// <param name="targetValue">Objectif à atteindre (calories ou grammes).</param>
    /// <param name="userTotalValue">Total atteint par l’utilisateur.</param>
    /// <param name="solutions">Indices des aliments « solution ».</param>
    /// <param name="subType">Sous-type de la question (Calorie, Proteine, Lipide, Glucide, Fibres).</param>
    public void Show(
        List<FoodData> foods,
        List<PortionSelection> portions,
        float targetValue,
        float userTotalValue,
        List<int> solutions,
        QuestionSubType subType
    )
    {
        // 0) Déterminer l’unité à afficher
        string unit = TextFormatter.GetUnitForSubType(subType);

        // 1) Header : Objectif
        if (targetText != null)
        {
            int roundedTarget = Mathf.RoundToInt(targetValue);
            targetText.text = $"{roundedTarget} {unit}";
        }

        // 2) Header : Total utilisateur + feedback couleur
        if (userTotalText != null)
        {
            int roundedUser = Mathf.RoundToInt(userTotalValue);
            userTotalText.text  = $"{roundedUser} {unit}";
            userTotalText.color = Mathf.Abs(roundedUser - Mathf.RoundToInt(targetValue)) <= nearThreshold
                ? nearTargetColor
                : defaultColor;
        }

        // 3) Vider les anciens items
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null)
                Destroy(_spawned[i]);
        }
        _spawned.Clear();

        // 4) Remplir la liste avec les nouveaux items
        for (int i = 0; i < foods.Count; i++)
        {
            FoodData f           = foods[i];
            PortionSelection sel = portions[i];

            bool isSolution = solutions.Contains(i);
            FoodItemResultState state = isSolution
                ? FoodItemResultState.SelectedCorrect
                : FoodItemResultState.Neutral;

            GameObject go       = Instantiate(itemPrefab, itemsContainer);
            FoodItemUI foodItem = go.GetComponent<FoodItemUI>();
            foodItem.Init(f, sel, state, subType);

            _spawned.Add(go);
        }

        // 5) Animation d’apparition
        panel.SetActive(true);
        canvasGroup.alpha           = 0f;
        panel.transform.localScale  = Vector3.one * 0.95f;

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, 0.2f));
        seq.Join(panel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutQuad));
    }
}