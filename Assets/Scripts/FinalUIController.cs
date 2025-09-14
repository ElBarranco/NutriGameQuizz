using UnityEngine;
using TMPro;

public class FinalUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;


    public void InitFinalUI(int score)
    {
        scoreText.text = $"{score}";

    }
}