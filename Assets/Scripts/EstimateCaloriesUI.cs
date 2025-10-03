using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using NaughtyAttributes;
using DG.Tweening; // Pour DOFillAmount

public class EstimateCaloriesUI : BaseQuestionUI
{
    [SerializeField] private TMP_Text foodNameText;
    [SerializeField] private Image foodImage;
    [SerializeField] private Slider calorieSlider;
    [SerializeField] private TMP_Text sliderValueText;
    [SerializeField] private TMP_Text UnitText;
    [SerializeField] private Image gaugeFill;





    private FoodData food;


    private QuestionSubType currentSubType;


    public void RefreshSliderValue()
    {
        InteractionManager.Instance.TriggerLightVibration();

        guess = Mathf.RoundToInt(calorieSlider.value);
        sliderValueText.text = $"{guess}";

        // Mise à jour fluide de la gauge
        float normalized = Mathf.InverseLerp(calorieSlider.minValue, calorieSlider.maxValue, calorieSlider.value);
        gaugeFill.DOFillAmount(normalized, 0.2f); // anim fluide sur 0.2s
    }

    public void Init(QuestionSubType sousType, FoodData foodData, PortionSelection? portion)
    {
        food = foodData;
        currentSubType = sousType;


        float rawValue = portion.Value.Value;

        float min = 0;
        float noise = UnityEngine.Random.Range(-0.25f, 0.25f);
        float max = Mathf.Max(0f, rawValue * (1f + noise));

        calorieSlider.minValue = min;
        calorieSlider.maxValue = max;

        // Positionner le slider à 50% (milieu de la plage)
        calorieSlider.value = (min + max) / 2f;


        UnitText.text = PortionTextFormatter.UnitForQuestion(sousType);

        // Affichage nom + image
        foodImage.sprite = SpriteLoader.LoadFoodSprite(food.Name);
        foodNameText.text = PortionTextFormatter.ToDisplayWithFood(food, portion.Value);

        // Init gauge directement à la bonne valeur
        float normalized = Mathf.InverseLerp(min, max, calorieSlider.value);
        gaugeFill.fillAmount = normalized;
    }

}