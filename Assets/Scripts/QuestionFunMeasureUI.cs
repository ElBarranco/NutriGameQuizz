using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestionFunMeasureUI : QuestionCaloriesDualUI
{
    [SerializeField] private Image measureImageA;
    [SerializeField] private Image measureImageB;

    private string measureNameA;
    private string measureNameB;
    


    public void Init(List<SpecialMeasureData> specialMeasures, FoodData a, FoodData b, Action<int, bool> callback)
    {
        if (specialMeasures == null || specialMeasures.Count < 2)
        {
            Debug.LogError("[QuestionFunMeasureUI] SpecialMeasures invalide ou incomplète.");
            return;
        }

        SpecialMeasureData m1 = specialMeasures[0];
        SpecialMeasureData m2 = specialMeasures[1];

        this.measureNameA = m1.name;
        this.measureNameB = m2.name;


        nameA.text = GetFormattedLabel(measureNameA, a.Name);
        nameB.text = GetFormattedLabel(measureNameB, b.Name);

        measureImageA.sprite = LoadMeasureSprite(measureNameA);
        measureImageB.sprite = LoadMeasureSprite(measureNameB);

        
        foodA = a;
        foodB = b;
        onAnswered = callback;


        imageA.sprite = SpriteLoader.LoadFoodSprite(foodA.Name);
        imageB.sprite = SpriteLoader.LoadFoodSprite(foodB.Name);

        base.InitVisual();
    }

    private Sprite LoadMeasureSprite(string measureName)
    {
        string formattedName = measureName.Replace(" ", "").ToLower();
        return Resources.Load<Sprite>($"FoodIcon/{formattedName}");
    }

    private string GetFormattedLabel(string measureKey, string foodName)
    {
        string readable = SplitPascalCase(measureKey).ToLower();
        return $"Une {readable} de {foodName}";
    }

    private string SplitPascalCase(string input)
    {
        System.Text.StringBuilder sb = new();

        foreach (char c in input)
        {
            if (char.IsUpper(c) && sb.Length > 0)
                sb.Append(' ');

            sb.Append(c);
        }

        return sb.ToString();
    }
}