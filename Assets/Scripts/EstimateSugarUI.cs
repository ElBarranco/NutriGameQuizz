using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using NaughtyAttributes;

public class EstimateSugarUI : MonoBehaviour
{
    [SerializeField] private TMP_Text foodNameText;
    [SerializeField] private Image foodImage;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button validateButton;

    [ReadOnly][SerializeField] private int answer = 0; // vraie réponse (glucides/4)
    private int guess = 0;

    private FoodData food;
    private PortionSelection portion;
    private QuestionSubType currentSubType;
    protected Action<int, bool> onComplete;

    [Header("Config")]
    [SerializeField] private int minGuess = 0;
    [SerializeField] private int maxGuess = 50; // borne haute arbitraire

    public void Init(QuestionSubType sousType, FoodData foodData, PortionSelection portionSel, Action<int, bool> callback)
    {
        food = foodData;
        portion = portionSel;
        currentSubType = sousType;
        onComplete = callback;

        // Récupérer la vraie valeur (déjà préparée par SugarQuestionGenerator)
        answer = Mathf.RoundToInt(portion.Value);

        // Définir un intervalle ±50% autour de la vraie valeur
        minGuess = 0;
        maxGuess = Mathf.Max(1, Mathf.RoundToInt(answer * 1.5f));

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

    public void OnValidate()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        validateButton.interactable = false;
        plusButton.interactable = false;
        minusButton.interactable = false;

        onComplete?.Invoke(guess, false);

        Destroy(gameObject);
    }
}