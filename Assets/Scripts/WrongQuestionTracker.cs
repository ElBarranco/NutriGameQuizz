using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class WrongQuestionTracker : MonoBehaviour
{
    [ReadOnly]
    [SerializeField]
    private List<QuestionData> wrongQuestions = new List<QuestionData>();
    private List<int> wrongQuestionsID = new List<int>();


    public void Clear() => wrongQuestions.Clear();


    public void AddWrongQuestion(QuestionData question)
    {
        // 1) on crée la copie
        QuestionData clone = question.Clone();

        // 2) on marque la copie comme "répondue fausse"
        clone.HasBeenAnsweredWrong = true;

        // 3) on l’ajoute à la liste
        wrongQuestions.Add(clone);
    }
}