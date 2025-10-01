using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NutritionTableUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI caloriesText;
    [SerializeField] private TextMeshProUGUI proteinesText;
    [SerializeField] private TextMeshProUGUI lipidesText;
    [SerializeField] private TextMeshProUGUI glucidesText;

    public void SetValues(List<float> values)
    {
        if (values == null || values.Count < 4)
        {
            Debug.LogWarning("[NutritionTableUI] Valeurs nutritionnelles invalides.");
            return;
        }

        caloriesText.text  = $"{values[0]}";
        proteinesText.text = $"{values[1]}";
        lipidesText.text   = $"{values[2]}";
        glucidesText.text  = $"{values[3]}";
    }
}