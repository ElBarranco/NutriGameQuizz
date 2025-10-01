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

    public void Init(FoodData food, QuestionSubType questionSubType, int idx, Action<int, bool> callback, PortionSelection? sel = null)
    {
        index = idx;
        onClick = callback;


        if (sel.HasValue)
        {
            nameText.text = PortionTextFormatter.ToDisplayWithFood(food, sel.Value);
        }
        else
        {
            nameText.text = food.Name;
        }
        icon.sprite = SpriteLoader.LoadFoodSprite(food.Name);
        base.UpdateHintInfo(food, questionSubType);
        isSelected = false;
        background.color = normalColor;
        button.interactable = true;
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