using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FoodItemUI : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;      
    [SerializeField] private TextMeshProUGUI kcal100gText;  
    [SerializeField] private TextMeshProUGUI portionText;   

    [Header("Feedback visuel")]
    [SerializeField] private GameObject solutionMarker;    
    [SerializeField] private GameObject selectedGO;         
    [SerializeField] private Image solutionImage;           

    [SerializeField] private Color solutionColor = Color.green;   
    [SerializeField] private Color defaultColor = Color.white;    
    [SerializeField] private Color wrongColor = Color.red;        
    [SerializeField] private Color missedColor = new Color(1f, 0.65f, 0f);

    public void Init(
        FoodData f,
        PortionSelection? sel = null,
        FoodItemResultState resultState = FoodItemResultState.Neutral,
        QuestionSubType subType = QuestionSubType.Calorie
    )
    {
        // Texte du nom ou portion
        if (sel.HasValue)
        {
            nameText.text = PortionTextFormatter.ToDisplayWithFood(f, sel.Value);
        }
        else
        {
            nameText.text = f.Name;
        }

        selectedGO.SetActive(
            resultState == FoodItemResultState.SelectedCorrect ||
            resultState == FoodItemResultState.SelectedWrong
        );

        icon.sprite = SpriteLoader.LoadFoodSprite(f.Name);

        string unit = TextFormatter.GetUnitForSubType(subType);

        // /100g
        float per100 = PortionCalculator.GetPer100(f, subType);
        if (kcal100gText != null)
        {
            kcal100gText.text = $"{Mathf.RoundToInt(per100)} {unit} / 100 g";
        }

        // Portion
        if (portionText != null && sel.HasValue)
        {
            PortionSelection ps = sel.Value;
            if (ps.Type == FoodPortionType.Simple)
            {
                portionText.text = $"{Mathf.RoundToInt(f.Proteins)} {unit}";
            }
            else
            {
                portionText.text = $"{Mathf.RoundToInt(ps.Value)} {unit}";
            }
        }

        ApplySolutionVisuals(resultState);
    }

    private void ApplySolutionVisuals(FoodItemResultState state)
    {
        if (solutionMarker != null)
        {
            solutionMarker.SetActive(true);
        }
        if (solutionImage == null)
        {
            return;
        }

        switch (state)
        {
            case FoodItemResultState.SelectedCorrect:
                solutionImage.color = solutionColor;
                break;

            case FoodItemResultState.SelectedWrong:
                solutionImage.color = wrongColor;
                break;

            case FoodItemResultState.MissedCorrect:
                solutionImage.color = missedColor;
                break;

            default: // Neutral
                if (solutionMarker != null)
                {
                    solutionMarker.SetActive(false);
                }
                solutionImage.color = defaultColor;
                break;
        }
    }
}