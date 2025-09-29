using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using NaughtyAttributes;

public class EstimateSugarUI : BaseQuestionUI
{
    [SerializeField] private TMP_Text foodNameText;
    [SerializeField] private Image foodImage;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;

    private FoodData food;
    private PortionSelection portion;
    private QuestionSubType currentSubType;

    [Header("Config")]
    [SerializeField] private int minGuess = 0;
    [SerializeField] private int maxGuess = 50; // borne haute arbitraire

    public void Init(QuestionSubType sousType, FoodData foodData, PortionSelection portionSel)
    {
        plusButton.interactable = true;
        minusButton.interactable = true;

        food = foodData;
        portion = portionSel;
        currentSubType = sousType;

        // Définir un intervalle ±50% autour de la vraie valeur
        minGuess = 0;
        maxGuess = Mathf.Max(1, Mathf.RoundToInt(portion.Value * 1.5f));

        // Valeur initiale au milieu
        guess = 0;
        UpdateDisplay();

        // Image + nom
        foodImage.sprite = SpriteLoader.LoadFoodSprite(food.Name);
        foodNameText.text = PortionTextFormatter.ToDisplayWithFood(food, portion);

    }

    private void UpdateDisplay()
    {
        valueText.text = guess.ToString();
    }

    public void Btn_OnPlus()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        if (guess < maxGuess)
        {
            guess++;
            UpdateDisplay();
        }
    }

    public void Btn_OnMinus()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        if (guess > minGuess)
        {
            guess--;
            UpdateDisplay();
        }
    }

}