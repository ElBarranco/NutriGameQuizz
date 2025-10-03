using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class QuestionData
{
    // Identifiant numérique unique pour la session
    public int QuestionId;

    // Type et sous-type
    public QuestionType Type;
    public QuestionSubType SousType;
    
    public TapTaupeSubType TapTaupeSubType;

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

    public QuestionData Clone()
    {
        return new QuestionData
        {
            QuestionId        = this.QuestionId,
            Type              = this.Type,
            SousType          = this.SousType,
            
            TapTaupeSubType   = this.TapTaupeSubType,

            Aliments          = new List<FoodData>(this.Aliments ?? Enumerable.Empty<FoodData>()),
            PortionSelections = new List<PortionSelection>(this.PortionSelections ?? Enumerable.Empty<PortionSelection>()),
            ValeursComparees  = new List<float>(this.ValeursComparees ?? Enumerable.Empty<float>()),

            IndexBonneRéponse = this.IndexBonneRéponse,
            SortSolution      = this.SortSolution,

            Solutions         = new List<int>(this.Solutions ?? Enumerable.Empty<int>()),
            SpecialMeasures   = new List<SpecialMeasureData>(this.SpecialMeasures ?? Enumerable.Empty<SpecialMeasureData>()),
            SportChoices      = new List<SportData>(this.SportChoices ?? Enumerable.Empty<SportData>()),

            DeltaTolerance       = this.DeltaTolerance,
            HasBeenAnsweredWrong = this.HasBeenAnsweredWrong
        };
    }
}