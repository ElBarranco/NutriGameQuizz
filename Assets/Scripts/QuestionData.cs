using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestionData
{
    public QuestionType Type;
    public QuestionSubType SousType;
    public List<FoodData> Aliments;

    public List<PortionSelection> PortionSelections;
    public List<float> ValeursComparees; // selon le sous-type
    public int IndexBonneRéponse;
    public string SortSolution; // pour les questions de type Sort

    public List<int> Solutions;
    public List<SpecialMeasureData> SpecialMeasures; // pour les questions de type FunMeasure
    public int DeltaTolerance = 50;
    public List<SportData> SportChoices;

}