using UnityEngine;
public static class TextFormatter
{
    public static string GetUnitForSubType(QuestionSubType type)
    {
        switch (type)
        {
            case QuestionSubType.Calorie:
                return "kcal";

            case QuestionSubType.Proteine:
            case QuestionSubType.Glucide:
            case QuestionSubType.Lipide:
            case QuestionSubType.Fibres:
                return "g";

            case QuestionSubType.Libre:
            default:
                return "";
        }
    }

    public static string GetLabelFromSubType(QuestionSubType subType)
    {
        return subType switch
        {
            QuestionSubType.Proteine => "Protéines",
            QuestionSubType.Glucide => "Glucides",
            QuestionSubType.Lipide => "Lipides",
            QuestionSubType.Fibres => "Fibres",
            _ => "Calories"
        };
    }
    public static string ToDisplayValue(QuestionSubType subType, float value)
    {
        string unit = GetUnitForSubType(subType);
        return $"{Mathf.RoundToInt(value)} {unit}";
    }

}