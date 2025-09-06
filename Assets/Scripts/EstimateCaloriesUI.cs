using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using NaughtyAttributes;

public class EstimateCaloriesUI : MonoBehaviour
{
    [SerializeField] private TMP_Text foodNameText;
    [SerializeField] private Image foodImage;
    [SerializeField] private Slider calorieSlider;
    [SerializeField] private TMP_Text sliderValueText;
    [SerializeField] private TMP_Text UnitText;
    [SerializeField] private Button validateButton;
    [ReadOnly][SerializeField] private float answer = 0;

    [Header("Bornes par sous-type")]
    [SerializeField] private int minCalories = 0;
    [SerializeField] private int maxCalories = 700;

    [SerializeField] private int minProteines = 0;
    [SerializeField] private int maxProteines = 100;

    [SerializeField] private int minGlucides = 0;
    [SerializeField] private int maxGlucides = 100;

    private FoodData food;
    protected Action<int, bool> onComplete;

    private QuestionSubType currentSubType;
    private int guess = 0;

    public void RefreshSliderValue()
    {
        InteractionManager.Instance.TriggerLightVibration();

        guess = Mathf.RoundToInt(calorieSlider.value);
        sliderValueText.text = $"{guess}";
    }


    public void Init(QuestionSubType sousType, FoodData foodData, PortionSelection? portion, Action<int, bool> callback)
    {
        food = foodData;
        currentSubType = sousType;
        onComplete = callback;

        switch (sousType)
        {
            case QuestionSubType.Calorie:
                calorieSlider.minValue = minCalories;
                calorieSlider.maxValue = maxCalories;
                answer = food.Calories;
                break;

            case QuestionSubType.Proteine:
                calorieSlider.minValue = minProteines;
                calorieSlider.maxValue = maxProteines;
                answer = food.Proteins;
                break;

            case QuestionSubType.Glucide:
                calorieSlider.minValue = minGlucides;
                calorieSlider.maxValue = maxGlucides;
                answer = food.Carbohydrates;
                break;

            default:
                Debug.LogWarning("Sous-type non géré dans EstimateCaloriesUI");
                break;
        }

        UnitText.text = PortionTextFormatter.UnitForQuestion(sousType);

        // Clamp pour sécurité
        answer = Mathf.Clamp(answer, calorieSlider.minValue, calorieSlider.maxValue);

        // Centre le slider
        calorieSlider.value = (calorieSlider.minValue + calorieSlider.maxValue) / 2f;

        // UI
        foodImage.sprite = SpriteLoader.LoadFoodSprite(food.Name);

        foodNameText.text = PortionTextFormatter.ToDisplayWithFood(food, portion.Value);


        RefreshSliderValue();
    }

    public void Btn_Validate()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        validateButton.interactable = false;

        onComplete?.Invoke(guess, false);

        Destroy(gameObject);
    }
}