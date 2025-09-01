using UnityEngine;
using System;

public class QuestionFactory : MonoBehaviour
{
    [Header("Question Prefabs")]
    [SerializeField] private GameObject tinderLikeGo;
    [SerializeField] private GameObject estimateCaloriesGo;
    [SerializeField] private GameObject SpecialMeasureGo;
    [SerializeField] private GameObject mealCompositionGo;
    [SerializeField] private GameObject SportDualGo; // prefab UI sport
    [SerializeField] private GameObject sortGo;      // prefab UI tri
    [SerializeField] private Transform questionParent;

    // Référence éventuelle, mais on n’utilise pas ce type en paramètre pour éviter les conversions
    public delegate void AnswerCallback(int userAnswer, bool isPerfect);

    public void CreateQuestion(QuestionData data, Action<int, bool> onAnswered)
    {
        switch (data.Type)
        {
            case QuestionType.CaloriesDual:
                {
                    GameObject go = Instantiate(tinderLikeGo, questionParent);
                    go.GetComponent<QuestionCaloriesDualUI>()
                      .Init(
                          data.Aliments[0], data.Aliments[1],
                          data.PortionSelections[0], data.PortionSelections[1],
                          onAnswered
                      );
                    break;
                }

            case QuestionType.FunMeasure:
                {
                    GameObject go = Instantiate(SpecialMeasureGo, questionParent);
                    go.GetComponent<QuestionFunMeasureUI>()
                      .Init(data.SpecialMeasures, data.Aliments[0], data.Aliments[1], onAnswered);
                    break;
                }

            case QuestionType.EstimateCalories:
                {
                    GameObject go = Instantiate(estimateCaloriesGo, questionParent);
                    go.GetComponent<EstimateCaloriesUI>()
                      .Init(data.SousType, data.Aliments[0], data.PortionSelections[0], onAnswered);
                    break;
                }

            case QuestionType.MealComposition:
                {
                    GameObject go = Instantiate(mealCompositionGo, questionParent);
                    go.GetComponent<QuestionMealCompositionUI>().Init(data, onAnswered);
                    break;
                }

            case QuestionType.Sport:
                {
                    GameObject go = Instantiate(SportDualGo, questionParent);
                    go.GetComponent<QuestionSportDualUI>()
                      .Init(
                          data.SportChoices[0],
                          data.SportChoices[1],
                          onAnswered
                      );
                    break;
                }

            case QuestionType.Tri:
                {
                    GameObject go = Instantiate(sortGo, questionParent);
                    go.GetComponent<QuestionSortUI>()
                      .Init(data, onAnswered); 
                    break;
                }

            default:
                Debug.LogWarning("Type de question non géré : " + data.Type);
                break;
        }
    }
}