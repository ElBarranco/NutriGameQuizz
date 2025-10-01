using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestionData
{
    // Identifiant numérique unique pour la session
    public int QuestionId;

    // Type et sous-type
    public QuestionType Type;
    public QuestionSubType SousType;

    // Données de la question
    public List<FoodData> Aliments               = new List<FoodData>();
    public List<PortionSelection> PortionSelections = new List<PortionSelection>();
    public List<float> ValeursComparees          = new List<float>();  // selon le sous-type
    public int IndexBonneRéponse;
    public string SortSolution;                  // pour les questions de type Sort

    public List<int> Solutions                   = new List<int>();
    public List<SpecialMeasureData> SpecialMeasures = new List<SpecialMeasureData>(); // pour FunMeasure
    public int DeltaTolerance                    = 50;
    public List<SportData> SportChoices          = new List<SportData>();

    // Flag interne
    public bool HasBeenAnsweredWrong             = false;

    /// <summary>
    /// Crée une copie profonde de cette question, conservant tous les champs.
    /// </summary>
    public QuestionData Clone()
    {
        return new QuestionData
        {
            QuestionId           = this.QuestionId,
            Type                 = this.Type,
            SousType             = this.SousType,
            Aliments             = new List<FoodData>(this.Aliments),
            PortionSelections    = new List<PortionSelection>(this.PortionSelections),
            ValeursComparees     = new List<float>(this.ValeursComparees),
            IndexBonneRéponse    = this.IndexBonneRéponse,
            SortSolution         = this.SortSolution,
            Solutions            = new List<int>(this.Solutions),
            SpecialMeasures      = new List<SpecialMeasureData>(this.SpecialMeasures),
            DeltaTolerance       = this.DeltaTolerance,
            SportChoices         = new List<SportData>(this.SportChoices),
            HasBeenAnsweredWrong = this.HasBeenAnsweredWrong
        };
    }
}