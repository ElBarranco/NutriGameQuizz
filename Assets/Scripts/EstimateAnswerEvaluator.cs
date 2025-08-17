using UnityEngine;

public static class EstimateAnswerEvaluator
{




    public static bool Evaluate(
        QuestionData q,
        int userAnswer,
        out bool isPerfect,
        int deltaCalories = 40,
        int deltaOthers = 10,
        int perfectDelta = 1)
    {
        isPerfect = false;

        if (q == null || q.ValeursComparees == null || q.ValeursComparees.Count == 0)
            return false;

        int trueVal = Mathf.RoundToInt(q.ValeursComparees[0]);
        int errorAbs = Mathf.Abs(userAnswer - trueVal);

        int allowed = GetDeltaForSubType(q.SousType, deltaCalories, deltaOthers);

        isPerfect = errorAbs <= perfectDelta;
        bool isCorrect = errorAbs <= allowed;

#if UNITY_EDITOR
        Debug.Log(
            $"[EstimateAnswerEvaluator] " +
            $"Valeur vraie={trueVal} | " +
            $"Réponse joueur={userAnswer} | " +
            $"Erreur={errorAbs} | " +
            $"Tolérance={allowed} | " +
            $"Correct={isCorrect} | " +
            $"Perfect={isPerfect}"
        );
#endif

        return isCorrect;
    }

    /// <summary>
    /// Version simple si tu ne veux pas gérer le perfect.
    /// </summary>
    public static bool CheckEstimateAnswer(
        QuestionData q,
        int userAnswer,
        int deltaCalories = 40,
        int deltaOthers = 10)
    {
        bool dummyPerfect;
        return Evaluate(q, userAnswer, out dummyPerfect, deltaCalories, deltaOthers, 1);
    }

    private static int GetDeltaForSubType(QuestionSubType st, int deltaCalories, int deltaOthers)
    {
        switch (st)
        {
            case QuestionSubType.Calorie: return deltaCalories;
            case QuestionSubType.Proteine:
            case QuestionSubType.Glucide:
            case QuestionSubType.Lipide:
            case QuestionSubType.Fibres: return deltaOthers;
            case QuestionSubType.Libre:
            default: return deltaOthers; // à spécialiser plus tard
        }
    }
}