using UnityEngine;
using TMPro;

public class FinalUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI countQuestionsText;


    public void InitFinalUI(int score, int totalQuestion, int correctAnswers)
    {
        scoreText.text = $"{score}";
        countQuestionsText.text = $"{correctAnswers}/{totalQuestion}";

    }
}