using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField, Header("Références")]
    private FoodDataParser foodParser;
    [SerializeField] private RewardFXController rewardFX;
    [SerializeField] private QuestionFactory questionFactory;
    [SerializeField] private LevelGenerator generator;
    [SerializeField] private HUDController hud;
    [SerializeField] private DifficultyManager difficultyManager;
    [SerializeField] private ScoreManager scoreManager;

    [Header("Session")]
    public bool IsGameRunning { get; private set; } = false;
    [ReadOnly][SerializeField] private LevelData generatedLevel;
    [ReadOnly] private QuestionData currentQuestion;
    [ReadOnly][SerializeField] private int currentAnswer = 0;

    [Header("Données")]
    private List<FoodData> foodList;

    private List<QuestionData> questionList;
    [ReadOnly] private int currentQuestionIndex = 0;
    public List<float> CurrentQuestionDataAnswer = new List<float>();
    public bool EnableMoreInfo { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void LaunchGame()
    {
        IsGameRunning = true;
        List<FoodData> allFoods = foodParser.GetFoodData();
        foodList = difficultyManager.FilterFoods(allFoods);

        if (foodList.Count < 2)
        {
            Debug.LogError("Pas assez d'aliments après filtrage !");
            return;
        }

        generator.SetFoodDataList(foodList);
        generatedLevel = generator.GenerateLevel(10);

        questionList = generatedLevel.Questions;
        currentQuestionIndex = 0;

        CurrentQuestionDataAnswer.Clear();
        scoreManager.Reinitialiser();
        hud.InitGame(generatedLevel.Questions.Count);
        LaunchNextQuestion();
    }

    public void LaunchNextQuestion()
    {
        if (currentQuestionIndex >= questionList.Count)
        {
            Debug.Log("Fin du niveau.");
            return;
        }

        EnableMoreInfo = false;

        currentQuestion = questionList[currentQuestionIndex];
        CurrentQuestionDataAnswer.Clear();
        CurrentQuestionDataAnswer.AddRange(currentQuestion.ValeursComparees);

        // ✅ Passe la méthode comme callback tri-paramètres
        questionFactory.CreateQuestion(currentQuestion, OnQuestionAnswered);
        hud.UpdateHUDForNewQuestion(currentQuestionIndex, currentQuestion);
    }

    // ✅ Signature unique utilisée partout
    private void OnQuestionAnswered(int userAnswer, bool isPerfect = false)
    {
        bool isCorrect = false;
        currentAnswer = userAnswer;
        float errorAbs = 0f;
        float errorPct = 0f;

        switch (currentQuestion.Type)
        {
            case QuestionType.CaloriesDual:
            case QuestionType.FunMeasure:
                isCorrect = (currentAnswer == currentQuestion.IndexBonneRéponse);
                break;

            case QuestionType.EstimateCalories:
                isCorrect = EstimateAnswerEvaluator.Evaluate(currentQuestion, userAnswer, out isPerfect);
                break;
        }

        Debug.Log($"[GameManager] Answer - Type:{currentQuestion.Type} | Correct:{isCorrect} | User:{userAnswer}");




        EnableMoreInfo = true;

        if (isCorrect)
            scoreManager.EnregistrerBonneReponse();
        else
            scoreManager.EnregistrerMauvaiseReponse();

        hud.SetNextButtonVisible(true, isCorrect);
        hud.ShowMoreInfo(currentQuestion, currentAnswer);

        // Si ton système de FX gère le perfect, passe-le
        rewardFX.PlayForAnswer(isCorrect, isPerfect);

        if (CurrentQuestionDataAnswer.Count > 1)
        {
            Debug.Log($"[GameManager] Valeurs comparées : {CurrentQuestionDataAnswer[0]} vs {CurrentQuestionDataAnswer[1]}");
        }
    }

    private bool CheckEstimateAnswer(QuestionData q, int userAnswer)
    {
        if (q.ValeursComparees == null || q.ValeursComparees.Count == 0)
            return false;

        return userAnswer == Mathf.RoundToInt(q.ValeursComparees[0]);
    }


    public void TriggerNextStep()
    {
        if (!EnableMoreInfo || !IsGameRunning) return;

        currentQuestionIndex++;

        if (currentQuestionIndex >= questionList.Count)
        {
            IsGameRunning = false;
            Debug.Log("Fin du niveau.");
            hud.ShowEndGameUI(); // à implémenter dans HUDController
            return;
        }

        hud.UpdateQuestionNumber(currentQuestionIndex);
        LaunchNextQuestion();
    }

    public void ValidateAnswer(int index, float difference)
    {
        currentAnswer = index;
        bool perfect = difference <= 1f; // Ajuste la tolérance si nécessaire

        if (CurrentQuestionDataAnswer == null || CurrentQuestionDataAnswer.Count <= index || CurrentQuestionDataAnswer.Count < 2)
        {
            Debug.LogWarning("Validation impossible : données insuffisantes.");
            // ✅ Fournir les 3 paramètres (userAnswer = -1 pour invalide)
            OnQuestionAnswered(-1, false);
            return;
        }

        float chosen = CurrentQuestionDataAnswer[index];
        float other = CurrentQuestionDataAnswer[1 - index];
        bool isCorrect = chosen >= other;

        // ✅ Fournir les 3 paramètres correctement typés
        OnQuestionAnswered(index, perfect);
    }

    public int GetCurrentAnswer()
    {
        return currentAnswer;
    }
}