using UnityEngine;
using System.Collections.Generic;
using System;

public class QuestionFactory : MonoBehaviour
{
    [Header("Question Prefabs")]
    [SerializeField] private GameObject tinderLikeGo;
    [SerializeField] private GameObject estimateCaloriesGo;
    [SerializeField] private GameObject SpecialMeasureGo;
    [SerializeField] private GameObject mealCompositionGo;
    [SerializeField] private Transform questionParent;

    public delegate void AnswerCallback(int userAnswer, bool isPerfect);


    public void CreateQuestion(QuestionData data, Action<int, bool> onAnswered)
    {
        switch (data.Type)
        {
            case QuestionType.CaloriesDual:
                GameObject go = Instantiate(tinderLikeGo, questionParent);
                go.GetComponent<QuestionCaloriesDualUI>()
                  .Init(data.Aliments[0], data.Aliments[1],
                        data.PortionSelections[0], data.PortionSelections[1],
                        onAnswered);
                break;

            case QuestionType.FunMeasure:
                GameObject go1 = Instantiate(SpecialMeasureGo, questionParent);
                QuestionFunMeasureUI ui = go1.GetComponent<QuestionFunMeasureUI>();
                ui.Init(data.SpecialMeasures, data.Aliments[0], data.Aliments[1], onAnswered);
                break;

            case QuestionType.EstimateCalories:
                GameObject go3 = Instantiate(estimateCaloriesGo, questionParent);
                go3.GetComponent<EstimateCaloriesUI>()
                  .Init(data.SousType, data.Aliments[0], data.PortionSelections[0], onAnswered);
                break;

            case QuestionType.MealComposition:

                GameObject go4 = Instantiate(mealCompositionGo, questionParent);
                go4.GetComponent<QuestionMealCompositionUI>().Init(data, onAnswered);;


                break;


            default:
                Debug.LogWarning($"Type de question non géré : {data.Type}");
                break;
        }

    }


}