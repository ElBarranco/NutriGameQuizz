using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestionData
{
     public string QuestionId;

     
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

    public bool HasBeenAnsweredWrong = false;


    public QuestionData Clone()
    {
        return new QuestionData
        {
            Type = this.Type,
            SousType = this.SousType,
            Aliments = new List<FoodData>(this.Aliments),
            PortionSelections = new List<PortionSelection>(this.PortionSelections),
            ValeursComparees = new List<float>(this.ValeursComparees),
            IndexBonneRéponse = this.IndexBonneRéponse,
            SortSolution = this.SortSolution,
            Solutions = new List<int>(this.Solutions),
            SpecialMeasures = new List<SpecialMeasureData>(this.SpecialMeasures),
            DeltaTolerance = this.DeltaTolerance,
            SportChoices = new List<SportData>(this.SportChoices),
            HasBeenAnsweredWrong = this.HasBeenAnsweredWrong  
        };
    }

}