using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class QuestionTitleManager : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private TextMeshProUGUI questionTitleText;

    [Header("Prefab pour Sport")]
    [SerializeField] private FoodItemUI foodItemPrefab;
    [SerializeField] private Transform foodItemParent;
    private GameObject _currentFoodItem;

    public void SetQuestionTitle(
        QuestionType type,
        QuestionSubType subType,
        List<FoodData> foods,
        List<PortionSelection> portions,
        float targetCalories = -1f)
    {
        string title;
        if (_currentFoodItem != null)
        {
            Destroy(_currentFoodItem);
            _currentFoodItem = null;
        }

        switch (type)
        {
            case QuestionType.EstimateCalories:
                switch (subType)
                {
                    case QuestionSubType.Proteine: title = "Estime la quantité de protéines !"; break;
                    case QuestionSubType.Glucide: title = "Estime la quantité de glucides !"; break;
                    case QuestionSubType.Lipide: title = "Estime la quantité de lipides !"; break;
                    case QuestionSubType.Fibres: title = "Estime la quantité de fibres !"; break;
                    default: title = "Devine les calories !"; break;
                }
                break;

            case QuestionType.CaloriesDual:
                switch (subType)
                {
                    case QuestionSubType.Proteine: title = "Quel aliment contient le plus de protéines ?"; break;
                    case QuestionSubType.Glucide: title = "Quel aliment contient le plus de glucides ?"; break;
                    case QuestionSubType.Lipide: title = "Quel aliment est le plus gras ?"; break;
                    case QuestionSubType.Fibres: title = "Quel aliment contient le plus de fibres ?"; break;
                    default: title = "Quel aliment est le plus calorique ?"; break;
                }
                break;

            case QuestionType.FunMeasure:
                title = "Quelle proposition est la plus calorique ?";
                break;

            case QuestionType.MealComposition:
                title = $"Compose un repas de {Mathf.RoundToInt(targetCalories)} calories !";
                break;

            case QuestionType.Tri:
                title = "Classe les aliments du - au + calorique !";
                break;

            case QuestionType.Sport:
                if (foods != null && foods.Count > 0)
                {
                    FoodData food = foods[0];
                    title = $"Combien de sport faut-il pour brûler un {food.Name}) ?";

                    FoodItemUI item = Instantiate(foodItemPrefab, foodItemParent);
                    _currentFoodItem = item.gameObject;
                    item.Init(food, portions[0], false, QuestionSubType.Calorie);
                }
                else
                {
                    title = "Trouve l'équivalent en sport !";
                }
                break;

            case QuestionType.Intru:
                switch (subType)
                {
                    case QuestionSubType.Proteine: title = "Élimine les aliments pauvres en protéines !"; break;
                    case QuestionSubType.Glucide: title = "Élimine les aliments pauvres en glucides !"; break;
                    case QuestionSubType.Lipide: title = "Élimine les aliments pauvres en lipides !"; break;
                    default: title = "Élimine les intrus nutritionnels !"; break;
                }
                break;

            case QuestionType.Recycling:
                switch (subType)
                {
                    case QuestionSubType.Proteine: title = "Trie les aliments protéiques sur le tapis roulant !"; break;
                    case QuestionSubType.Glucide: title = "Trie les aliments riches en glucides sur le tapis roulant !"; break;
                    case QuestionSubType.Lipide: title = "Trie les aliments gras sur le tapis roulant !"; break;
                    default: title = "Trie les bons aliments sur le tapis roulant !"; break;
                }
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