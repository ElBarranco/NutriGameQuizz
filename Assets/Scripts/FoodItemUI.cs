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
    [SerializeField] private GameObject solutionMarker;     // GO qui s’active si bonne réponse / feedback
    [SerializeField] private GameObject selectedGO;         // indique si le joueur a sélectionné
    [SerializeField] private Image solutionImage;           // Image dont on change la couleur

    [SerializeField] private Color solutionColor = Color.green;   // ✅ Bon choix
    [SerializeField] private Color defaultColor = Color.white;    // ⚪ Neutre
    [SerializeField] private Color wrongColor = Color.red;        // ❌ Mauvais choix
    [SerializeField] private Color missedColor = new Color(1f, 0.65f, 0f); // ⚠️ Raté (orange par défaut)

    public void Init(
        FoodData f,
        PortionSelection sel,
        bool isIntrus,
        QuestionSubType subType = QuestionSubType.Calorie,
        bool isSelected = false
    )
    {
        // Nom + portion lisible (ex. "200 g de fraises")
        nameText.text = PortionTextFormatter.ToDisplayWithFood(f, sel);

        selectedGO.SetActive(isSelected);

        icon.sprite = SpriteLoader.LoadFoodSprite(f.Name);

        // Unité selon le sous-type (kcal / g / prot / etc.)
        string unit = TextFormatter.GetUnitForSubType(subType);

        // Valeur / 100 g
        float per100 = PortionCalculator.GetPer100(f, subType);
        if (kcal100gText != null)
            kcal100gText.text = $"{Mathf.RoundToInt(per100)} {unit} / 100 g";

        // Valeur pour la portion
        if (portionText != null)
        {
            if (sel.Type == FoodPortionType.Simple)
                portionText.text = $"{Mathf.RoundToInt(f.Proteins)} {unit}";
            else
                portionText.text = $"{Mathf.RoundToInt(sel.Value)} {unit}";
        }

        // Feedback visuel
        ApplySolutionVisuals(isIntrus, isSelected);
    }

    private void ApplySolutionVisuals(bool isIntrus, bool isSelected)
    {
        if (solutionMarker != null)
            solutionMarker.SetActive(true);

        if (solutionImage == null)
            return;

        if (isIntrus && isSelected)
        {
            // ✅ Bon choix
            solutionImage.color = solutionColor;
        }
        else if (!isIntrus && isSelected)
        {
            // ❌ Mauvais choix
            solutionImage.color = wrongColor;
        }
        else if (isIntrus && !isSelected)
        {
            // ⚠️ Raté (il fallait cliquer mais le joueur a oublié)
            solutionImage.color = missedColor;
        }
        else
        {
            // ⚪ Neutre
            if (solutionMarker != null) solutionMarker.SetActive(false);
            solutionImage.color = defaultColor;
        }
    }
}