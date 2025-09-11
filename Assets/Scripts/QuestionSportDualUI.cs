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
        SportData sportB,
        Action<int, bool> callback)
    {

        sportAData = sportA;
        sportBData = sportB;


        // Libellés boutons (les champs nameA/nameB sont "protected" dans la classe parente)
        nameA.text = TextFormatter.ToDisplayDuration(sportAData.Duration) + " de " + sportAData.Name;
        nameB.text = TextFormatter.ToDisplayDuration(sportBData.Duration) + " de " + sportBData.Name;

        // Icônes de sport (si présentes dans Resources/SportIcon/)
        imageA.sprite = SpriteLoader.LoadSportSprite(sportAData.Name);
        imageB.sprite = SpriteLoader.LoadSportSprite(sportBData.Name);

        // Wrap du callback : on intercepte l'appel de la classe parente pour calculer isPerfect
        externalCallback = callback;
        onAnswered = WrappedAnswered; // "onAnswered" est protected dans la classe parente

        // Initialisation visuelle de la carte (méthode publique de la classe parente)
        screenWidth = Screen.width; // "screenWidth" est protected dans le parent
        InitVisual();
    }

    /// <summary>
    /// Wrapper appelé par la classe parente au moment de la sélection.
    /// On recalcule "isPerfect" en mode sport puis on appelle le callback original.
    /// </summary>
    private void WrappedAnswered(int chosenIndex, bool ignoredIsPerfectFromBase)
    {

        externalCallback.Invoke(chosenIndex, false);
    }


}