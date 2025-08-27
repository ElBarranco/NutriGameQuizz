using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodItemUI : MonoBehaviour
{

    [Header("UI Refs")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;      // ex. "3 Snickers", "Un bol de soupe", "200 g de fraises"
    [SerializeField] private TextMeshProUGUI kcal100gText;  // ex. "488 kcal / 100 g"
    [SerializeField] private TextMeshProUGUI portionText;   // ex. "244 kcal" (calories de la portion)


    [Header("Feedback visuel")]
    [SerializeField] private GameObject solutionMarker;     // GO qui s’active si bonne réponse
    [SerializeField] private Image solutionImage;           // Image dont on change la couleur
    [SerializeField] private Color solutionColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;
    public void Init(FoodData f, PortionSelection sel, bool trueAnswer = false, QuestionSubType subType = QuestionSubType.Calorie)
    {
        // Nom + portion lisible (PAS les calories ici)
        nameText.text = PortionTextFormatter.ToDisplayWithFood(f, sel);

        // Image
        icon.sprite = SpriteLoader.LoadFoodSprite(f.Name);

        // Rappel /100 g
        float per100 = PortionCalculator.GetPer100(f, subType);
        //kcal100gText.text = $"{Mathf.RoundToInt(per100)} {unit} / 100 g";

        // Unité selon le sous-type (kcal / g)
        string unit = TextFormatter.GetUnitForSubType(subType);

        // Valeur PAR PORTION (pré-calculée dans sel.Value)
        portionText.text = $"{Mathf.RoundToInt(sel.Value)} {unit}";
        
        // Feedback visuel
        ApplySolutionVisuals(trueAnswer);
    }
    private void ApplySolutionVisuals(bool isSolution)
    {

        //solutionMarker.SetActive(isSolution);

        solutionImage.color = isSolution ? solutionColor : defaultColor;
    }

}