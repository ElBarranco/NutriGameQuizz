using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class FoodSelectableSubtractionUI : FoodItemBase
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;

    [Header("Couleurs")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.green;

    private int index;
    private bool isSelected = false;
    private Action<int, bool> onClick;

    public void Init(FoodData food, PortionSelection sel, QuestionSubType questionSubType, int idx, Action<int, bool> callback)
    {
        index = idx;
        onClick = callback;

        nameText.text = PortionTextFormatter.ToDisplayWithFood(food, sel);
        icon.sprite = SpriteLoader.LoadFoodSprite(food.Name);

        isSelected = false;
        background.color = normalColor;

        button.interactable = true;

        base.UpdateHintInfo(food, questionSubType);
    }

    public void Btn_OnClick()
    {
        InteractionManager.Instance.TriggerMediumVibration();

        // Toggle local
        isSelected = !isSelected;
        background.color = isSelected ? selectedColor : normalColor;

        // ⚡ On ne détruit pas le bouton ici (contrairement à l’intrus)
        onClick?.Invoke(index, isSelected);
    }

    // ✅ Méthode publique pour forcer un état depuis le parent (unique selection)
    public void SetSelected(bool state)
    {
        isSelected = state;
        background.color = isSelected ? selectedColor : normalColor;
    }
}