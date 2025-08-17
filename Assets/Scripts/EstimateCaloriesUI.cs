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

    [SerializeField] private int minSucres = 0;
    [SerializeField] private int maxSucres = 100;

    private FoodData food;
    protected Action<int, bool> onComplete;

    private string unit = "";
    private QuestionSubType currentSubType;
    private int guess = 0;

    public void RefreshSliderValue()
    {
        InteractionManager.Instance.TriggerLightVibration();

        guess = Mathf.RoundToInt(calorieSlider.value);
        sliderValueText.text = $"{guess}";
    }

    public void Init(QuestionSubType sousType, FoodData foodData, Action<int, bool> callback)
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
                calorieSlider.minValue = minSucres;
                calorieSlider.maxValue = maxSucres;
                answer = food.Carbohydrates;
                break;

            default:
                Debug.LogWarning("Sous-type non géré dans EstimateCaloriesUI");
                break;
        }

        UnitText.text = PortionTextFormatter.UnitForQuestion(sousType);

        // Clamp pour éviter une erreur hors bornes
        answer = Mathf.Clamp(answer, calorieSlider.minValue, calorieSlider.maxValue);

        // Centre le slider
        calorieSlider.value = (calorieSlider.minValue + calorieSlider.maxValue) / 2f;

        // UI
        foodNameText.text = food.Name;
        foodImage.sprite = FoodSpriteLoader.LoadFoodSprite(food.Name);
        RefreshSliderValue();
    }

    private float GetTolerance(QuestionSubType sousType)
    {
        return sousType switch
        {
            QuestionSubType.Calorie => 50,
            QuestionSubType.Proteine => 5f,
            QuestionSubType.Glucide => 5f,
            _ => 5f // fallback
        };
    }

    public void Btn_Validate()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        validateButton.interactable = false;

        int roundedAnswer = Mathf.RoundToInt(answer);
        int diff = Mathf.Abs(roundedAnswer - guess);

        bool isCorrect = diff <= answer * 0.05f || diff <= 20;

        GameManager.Instance.ValidateAnswer(guess, diff);
        onComplete?.Invoke(guess, diff <= GetTolerance(currentSubType));

        Destroy(gameObject);
    }
}