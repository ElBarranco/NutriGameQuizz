using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MoreInfoMealPanelUI : MoreInfoPanelBase
{
    [Header("Header")]

    [SerializeField] private TextMeshProUGUI targetText;         // ex: "Objectif : 950 kcal"
    [SerializeField] private TextMeshProUGUI userTotalText;      // ex: "Votre plat : 910 kcal"

    [Header("Liste aliments")]
    [SerializeField] private Transform itemsContainer;           // parent pour les items
    [SerializeField] private GameObject itemPrefab;          // prefab (nom, image, kcal, portion optionnelle)

    [Header("Couleurs / Feedback")]
    [SerializeField] private Color nearTargetColor = new Color(0.3f, 0.8f, 0.3f);
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private int nearThreshold = 50;             // tolérance (±kcal) pour colorer "proche"

    private readonly List<GameObject> _spawned = new List<GameObject>();


    public void Show(List<FoodData> foods, List<PortionSelection> portions, float targetCalories, float userTotalCalories, List<int> Solutions)
    {
        // Sécu
        if (foods == null || portions == null || foods.Count != portions.Count)
        {
            Debug.LogWarning("[MoreInfoMealPanelUI] Données incohérentes (foods/portions).");
            return;
        }

        // Header

        if (targetText) targetText.text = $"{Mathf.RoundToInt(targetCalories)} kcal";

        if (userTotalText)
        {
            int userK = Mathf.RoundToInt(userTotalCalories);
            userTotalText.text = $"{userK} kcal";
            userTotalText.color = Mathf.Abs(userK - Mathf.RoundToInt(targetCalories)) <= nearThreshold
                ? nearTargetColor
                : defaultColor;
        }

        // Clear anciens items
        for (int i = 0; i < _spawned.Count; i++)
            if (_spawned[i]) Destroy(_spawned[i]);
        _spawned.Clear();

        // Remplir la liste
        for (int i = 0; i < foods.Count; i++)
        {
            FoodData f = foods[i];
            PortionSelection sel = portions[i];
            bool isSolution = Solutions.Contains(i); // true si cet index est une solution

            GameObject go = Instantiate(itemPrefab, itemsContainer);
            FoodItemUI foodItem = go.GetComponent<FoodItemUI>();
            foodItem.Init(f, sel, isSolution);


            _spawned.Add(go);
        }
        // Anim d’apparition
        panel.SetActive(true);
        canvasGroup.alpha = 0f;
        panel.transform.localScale = Vector3.one * 0.95f;

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1f, 0.2f));
        seq.Join(panel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutQuad));
    }
}