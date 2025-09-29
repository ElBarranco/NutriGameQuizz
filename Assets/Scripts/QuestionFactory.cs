using UnityEngine;
using System;

public class QuestionFactory : MonoBehaviour
{
    [SerializeField] private QuestionValidateButtonUI validateButtonUI;

    [Header("Question Prefabs")]
    [SerializeField] private GameObject tinderLikeGo;
    [SerializeField] private GameObject estimateCaloriesGo;
    [SerializeField] private GameObject SpecialMeasureGo;
    [SerializeField] private GameObject mealCompositionGo;
    [SerializeField] private GameObject SportDualGo;
    [SerializeField] private GameObject sortGo;
    [SerializeField] private GameObject intrusGo;
    [SerializeField] private GameObject recyclingGo;
    [SerializeField] private GameObject subtractionGo;
    [SerializeField] private GameObject sucreGo;
    [SerializeField] private Transform questionParent;



    private GameObject _currentQuestionGO; // 🔥 garde une référence

    public void CreateQuestion(QuestionData data)
    {
        // Avant de créer une nouvelle, on supprime l'ancienne
        ClearCurrentQuestion();

        switch (data.Type)
        {
            case QuestionType.CaloriesDual:
                _currentQuestionGO = Instantiate(tinderLikeGo, questionParent);
                _currentQuestionGO.GetComponent<QuestionCaloriesDualUI>()
                      .Init(data.Aliments[0], data.Aliments[1],
                            data.PortionSelections[0], data.PortionSelections[1]);
                break;

            case QuestionType.FunMeasure:
                _currentQuestionGO = Instantiate(SpecialMeasureGo, questionParent);
                _currentQuestionGO.GetComponent<QuestionFunMeasureUI>()
                      .Init(data.SpecialMeasures, data.Aliments[0], data.Aliments[1]);
                break;

            case QuestionType.EstimateCalories:
                _currentQuestionGO = Instantiate(estimateCaloriesGo, questionParent);
                _currentQuestionGO.GetComponent<EstimateCaloriesUI>()
                      .Init(data.SousType, data.Aliments[0], data.PortionSelections[0]);
                break;

            case QuestionType.MealComposition:
                _currentQuestionGO = Instantiate(mealCompositionGo, questionParent);
                _currentQuestionGO.GetComponent<QuestionMealCompositionUI>().Init(data);
                break;

            case QuestionType.Sport:
                _currentQuestionGO = Instantiate(SportDualGo, questionParent);
                _currentQuestionGO.GetComponent<QuestionSportDualUI>()
                      .Init(data.SportChoices[0], data.SportChoices[1]);
                break;

            case QuestionType.Sugar:
                _currentQuestionGO = Instantiate(sucreGo, questionParent);
                _currentQuestionGO.GetComponent<EstimateSugarUI>()
                      .Init(data.SousType, data.Aliments[0], data.PortionSelections[0]);
                break;

            case QuestionType.Tri:
                _currentQuestionGO = Instantiate(sortGo, questionParent);
                _currentQuestionGO.GetComponent<QuestionSortUI>().Init(data);
                break;

            case QuestionType.Intru:
                _currentQuestionGO = Instantiate(intrusGo, questionParent);
                _currentQuestionGO.GetComponent<QuestionIntrusUI>().Init(data);
                break;

            case QuestionType.Recycling:
                _currentQuestionGO = Instantiate(recyclingGo, questionParent);
                _currentQuestionGO.GetComponent<QuestionRecyclingUI>().Init(data);
                break;

            case QuestionType.Subtraction:
                _currentQuestionGO = Instantiate(subtractionGo, questionParent);
                _currentQuestionGO.GetComponent<QuestionSubtractionUI>().Init(data);
                break;

            default:
                Debug.LogWarning("Type de question non géré : " + data.Type);
                break;
        }

        validateButtonUI.BindQuestion(_currentQuestionGO?.GetComponent<BaseQuestionUI>(), data.Type);
    }

    /// <summary>
    /// Détruit la question en cours (si elle existe).
    /// </summary>
    public void ClearCurrentQuestion()
    {
        if (_currentQuestionGO != null)
        {
            Destroy(_currentQuestionGO);
            _currentQuestionGO = null;
        }
    }
}