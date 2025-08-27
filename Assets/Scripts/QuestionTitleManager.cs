using UnityEngine;
using TMPro;

public class QuestionTitleManager : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private TextMeshProUGUI questionTitleText;

    /// <summary>
    /// Définit le titre de la question en fonction de son type/sous-type.
    /// </summary>
    public void SetQuestionTitle(
        QuestionType type,
        QuestionSubType subType,
        float targetCalories = -1f,
        FoodData food = null)
    {
        string title;

        switch (type)
        {
            case QuestionType.EstimateCalories:
                switch (subType)
                {
                    case QuestionSubType.Proteine: title = "Estime la quantité de protéines !"; break;
                    case QuestionSubType.Glucide:  title = "Estime la quantité de glucides !"; break;
                    case QuestionSubType.Lipide:   title = "Estime la quantité de lipides !"; break;
                    case QuestionSubType.Fibres:   title = "Estime la quantité de fibres !"; break;
                    default:                       title = "Devine les calories !"; break;
                }
                break;

            case QuestionType.CaloriesDual:
                switch (subType)
                {
                    case QuestionSubType.Proteine: title = "Quel aliment contient le plus de protéines ?"; break;
                    case QuestionSubType.Glucide:  title = "Quel aliment contient le plus de glucides ?"; break;
                    case QuestionSubType.Lipide:   title = "Quel aliment est le plus gras ?"; break;
                    case QuestionSubType.Fibres:   title = "Quel aliment contient le plus de fibres ?"; break;
                    default:                       title = "Quel aliment est le plus calorique ?"; break;
                }
                break;

            case QuestionType.FunMeasure:
                title = "Tu vas être surpris...";
                break;

            case QuestionType.MealComposition:
                if (targetCalories > 0)
                    title = $"Compose un repas de {Mathf.RoundToInt(targetCalories)} calories !";
                else
                    title = "Compose ton repas !";
                break;

            case QuestionType.Sport:
                if (food != null)
                    title = $"Combien de sport faut-il pour brûler {Mathf.RoundToInt(targetCalories)} kcal (≈ {food.Name}) ?";
                else
                    title = "Trouve l'équivalent en sport !";
                break;

            default:
                title = "Question nutritionnelle";
                break;
        }

        if (questionTitleText != null)
            questionTitleText.text = title;
        else
            Debug.LogWarning("[QuestionTitleManager] Aucun TextMeshProUGUI assigné pour le titre de question.");
    }
}