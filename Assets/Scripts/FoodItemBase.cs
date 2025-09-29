using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
public class FoodItemBase : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI HintText;


    protected void UpdateHintInfo(FoodData f, QuestionSubType questionSubType)
    {
        float per100 = GetValueBySubType(f, questionSubType);
        string unit = TextFormatter.GetUnitForSubType(questionSubType);

        HintText.text = $"{Mathf.RoundToInt(per100)} {unit}";
    }

    protected float GetValueBySubType(FoodData food, QuestionSubType subType)
    {
        switch (subType)
        {
            case QuestionSubType.Proteine: return food.Proteins;
            case QuestionSubType.Glucide: return food.Carbohydrates;
            case QuestionSubType.Lipide: return food.Lipids;
            case QuestionSubType.Fibres: return food.Fibers;
            default: return food.Calories;
        }
    }
}