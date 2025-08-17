using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class MoreInfoFunMeasurePanelUI : MoreInfoPanelBase
{
    [SerializeField] private TextMeshProUGUI titleA;



    [SerializeField] private TextMeshProUGUI titleB;



    [SerializeField] private Image imageA;
    [SerializeField] private Image imageB;

    [Header("Highlights")]
    [SerializeField] private Image foodAHighlight;
    [SerializeField] private Image foodBHighlight;
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    // ----- Morinfo A / B (4 blocs x2) -----
    [System.Serializable]
    private class MorinfoUI
    {
        public TMP_Text Aliment;     // "100 g Fraise = 32 kcal"
        public TMP_Text ObjetLabel;  // "Volume Tour Eiffel"
        public TMP_Text ObjetValue;  // "≈ 7 300 L"
        public TMP_Text Resultat;    // "≈ 1 234 567 kcal"
        public TMP_Text Calcul;      // "= 7 300 L × 0,95 g/mL → …"
    }

    [Header("Morinfo (A)")]
    [SerializeField] private MorinfoUI morinfoA;

    [Header("Morinfo (B)")]
    [SerializeField] private MorinfoUI morinfoB;

    public void Show(List<FoodData> aliments, List<SpecialMeasureData> measures, int indexBonneReponse)
    {
        if (aliments == null || aliments.Count < 2 || measures == null || measures.Count < 2)
        {
            Debug.LogError("[MoreInfoFunMeasurePanelUI] Données invalides dans Show().");
            return;
        }

        FoodData a = aliments[0];
        FoodData b = aliments[1];
        SpecialMeasureData m1 = measures[0];
        SpecialMeasureData m2 = measures[1];

        // --- A ---
        float densityA = a.Volume > 0 ? (float)a.Weight / a.Volume : 1f; // g/mL
        float poidsA_g = densityA * m1.VolumeLitres * 1000f;             // g
        int kcalA = SpecialMeasureManager.GetCaloriesFor(a, m1.VolumeLitres);

        // --- B ---
        float densityB = b.Volume > 0 ? (float)b.Weight / b.Volume : 1f; // g/mL
        float poidsB_g = densityB * m2.VolumeLitres * 1000f;             // g
        int kcalB = SpecialMeasureManager.GetCaloriesFor(b, m2.VolumeLitres);

        // --- UI A/B (panneaux détaillés gauche/droite) ---
        titleA.text = $"{m1.name} de {a.Name}";
        titleB.text = $"{m2.name} de {b.Name}";

        // Sprites
        if (imageA) imageA.sprite = FoodSpriteLoader.LoadFoodSprite(a.Name);
        if (imageB) imageB.sprite = FoodSpriteLoader.LoadFoodSprite(b.Name);

        // Highlights
        if (foodAHighlight) foodAHighlight.color = defaultColor;
        if (foodBHighlight) foodBHighlight.color = defaultColor;
        if (indexBonneReponse == 0) { if (foodAHighlight) foodAHighlight.color = correctColor; }
        else { if (foodBHighlight) foodBHighlight.color = correctColor; }

        // --- Morinfo (4 blocs) pour A et pour B ---
        SetMorinfoBlocks(morinfoA, a, m1);
        SetMorinfoBlocks(morinfoB, b, m2);

        // --- Anim de panneau ---
        panel.SetActive(true);
        canvasGroup.alpha = 0f;
        panel.transform.localScale = Vector3.zero;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(1f, 0.3f));
        sequence.Join(panel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
    }

    private void SetMorinfoBlocks(MorinfoUI ui, FoodData food, SpecialMeasureData measure)
    {
        if (ui == null || food == null || measure == null) return;

        // Calcul identique à ta logique (cohérent avec GetCaloriesFor)
        float density = food.Volume > 0 ? (float)food.Weight / food.Volume : 1f; // g/mL
        if (food.Volume <= 0)
            Debug.LogWarning($"[MoreInfoFunMeasurePanelUI] Volume invalide pour {food.Name}, fallback densité = 1 g/mL.");

        double poids_g = density * measure.VolumeLitres * 1000.0;
        int kcalTotal = SpecialMeasureManager.GetCaloriesFor(food, measure.VolumeLitres); // valeur officielle

        // UI
        if (ui.Aliment)     ui.Aliment.text    = $"100 g {food.Name} = {food.Calories:N0} kcal";
        if (ui.ObjetLabel)  ui.ObjetLabel.text = $"Volume {measure.Type}";
        if (ui.ObjetValue)  ui.ObjetValue.text = $"{measure.VolumeLitres:N0} L";
        if (ui.Resultat)    ui.Resultat.text   = $"{kcalTotal:N0} kcal";
        if (ui.Calcul)      ui.Calcul.text     = $"= {measure.VolumeLitres:N0} L × {density:F2} g/mL → {poids_g:N0} g → {kcalTotal:N0} kcal";
    }
}