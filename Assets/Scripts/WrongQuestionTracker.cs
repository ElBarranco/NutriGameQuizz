using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class WrongQuestionTracker : MonoBehaviour
{
    [ReadOnly]
    [SerializeField]
    private List<QuestionData> wrongQuestions = new List<QuestionData>();

    /// <summary> Combien de questions ont été ratées. </summary>
    public int WrongQuestionCount => wrongQuestions.Count;

    /// <summary> Vide la liste en début de partie. </summary>
    public void Clear()
    {
        wrongQuestions.Clear();
    }

    /// <summary> Ajoute un clone de la question ratée, et marque le clone. </summary>
    public void AddWrongQuestion(QuestionData question)
    {
        if (question == null)
            return;

        QuestionData clone = question.Clone();
        clone.HasBeenAnsweredWrong = true;
        wrongQuestions.Add(clone);
    }

    public List<QuestionData> GetRandomWrongQuestions(int quantity)
    {
        int available = wrongQuestions.Count;
        if (available == 0 || quantity <= 0)
        {
            return new List<QuestionData>();
        }

        int toTake = quantity < available ? quantity : available;

        // Copie locale pour extraire sans doublons
        List<QuestionData> pool = new List<QuestionData>(wrongQuestions);
        List<QuestionData> selection = new List<QuestionData>(toTake);

        for (int i = 0; i < toTake; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            selection.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return selection;
    }
}