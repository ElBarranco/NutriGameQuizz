using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;      // ex. "3 Snickers", "Un bol de soupe", "200 g de fraises"
    [SerializeField] private TextMeshProUGUI kcal100gText;  // ex. "488 kcal / 100 g"
    [SerializeField] private TextMeshProUGUI portionText;   // ex. "244 kcal" (calories de la portion)



    public void Init(FoodData f, PortionSelection sel)
    {
        // Nom + portion lisible (PAS les calories ici)
        nameText.text = PortionTextFormatter.ToDisplayWithFood(f, sel);

        // Image
        icon.sprite = FoodSpriteLoader.LoadFoodSprite(f.Name);

        // Rappel /100 g
        float kcalPer100 = f.Calories;
       //kcal100gText.text = $"{Mathf.RoundToInt(kcalPer100)} kcal / 100 g";

        // Calories de la portion (pas /100 g)
        float gramsPortion = ResolveGramsFromPortion(f, sel);
        float kcalPortion = kcalPer100 * (gramsPortion / 100f);
        portionText.text = $"{Mathf.RoundToInt(kcalPortion)} kcal";
    }

    private static float ResolveGramsFromPortion(FoodData f, PortionSelection sel)
    {
        switch (sel.Type)
        {
            case FoodPortionType.Unitaire:
                // Poids d'une pièce * nb d'unités (Demi/Un/Deux/…)
                float pieceW = (f.Weight > 0f) ? f.Weight : 100f; // fallback 100 g si pas de poids
                return PortionCalculator.ToGrams(sel.Unitaire.Value, pieceW);

            case FoodPortionType.PetiteUnite:
                return PortionCalculator.ToGrams(sel.PetiteUnite.Value);

            case FoodPortionType.Liquide:
                return PortionCalculator.ToGrams(sel.Liquide.Value);

            case FoodPortionType.ParPoids:
            default:
                return sel.Grams;
        }
    }
}