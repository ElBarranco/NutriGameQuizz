using UnityEngine;
using TMPro;
using System;

public class QuestionSportDualUI : QuestionCaloriesDualUI
{
    [Header("Sport — Tuning")]
    [SerializeField] private float perfectTolerancePercent = 0.05f; // 5% de la cible pour "parfait"

    // Données sport
    private SportData sportAData;
    private SportData sportBData;



    // Callback original de la Factory
    private Action<int, bool> externalCallback;

    /// <summary>
    /// Initialise une carte "XX min de {SportA}" vs "YY min de {SportB}".
    /// targetKcal = calories à matcher (ex: 950).
    /// Minutes lues depuis sportA.Minutes et sportB.Minutes.
    /// </summary>
    public void Init(

        SportData sportA,
        SportData sportB)
    {

        sportAData = sportA;
        sportBData = sportB;


        // Libellés boutons (les champs nameA/nameB sont "protected" dans la classe parente)
        nameA.text = TextFormatter.ToDisplayDuration(sportAData.Duration) + " de " + sportAData.Name;
        nameB.text = TextFormatter.ToDisplayDuration(sportBData.Duration) + " de " + sportBData.Name;

        // Icônes de sport (si présentes dans Resources/SportIcon/)
        imageA.sprite = SpriteLoader.LoadSportSprite(sportAData.Name);
        imageB.sprite = SpriteLoader.LoadSportSprite(sportBData.Name);

        // Initialisation visuelle de la carte (méthode publique de la classe parente)
        screenWidth = Screen.width; // "screenWidth" est protected dans le parent
        InitVisual();
    }


}