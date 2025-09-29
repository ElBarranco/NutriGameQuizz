using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class FoodSelectableUI : FoodItemBase
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color intrusColor = Color.red;

    private int index;
    private bool isIntrus = false;
    private Action<int, bool> onClick;

    public void Init(FoodData food, QuestionSubType questionSubType, int idx, Action<int, bool> callback)
    {
        index = idx;
        onClick = callback;
        icon.sprite = SpriteLoader.LoadFoodSprite(food.Name);
        nameLabel.text = food.Name;
        background.color = normalColor;

        base.UpdateHintInfo(food, questionSubType);
    }

    private void ToggleState()
    {
        isIntrus = !isIntrus;
        background.color = isIntrus ? intrusColor : normalColor;
        onClick?.Invoke(index, isIntrus);
    }

    public void Btn_OnClick()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        button.interactable = false;
        ToggleState();

        transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}