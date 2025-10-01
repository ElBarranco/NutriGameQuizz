using UnityEngine;
using TMPro;

public class FinalUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI countQuestionsText;
    [SerializeField] private TextMeshProUGUI bestStreakText;


    public void InitFinalUI(int score, int totalQuestion, int correctAnswers, int bestStreak)
    {
        scoreText.text = $"{score}";
        countQuestionsText.text = $"{correctAnswers}/{totalQuestion}";
        bestStreakText.text = $"{bestStreak}";

    }
}