using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject quizPanel;

    public void ShowQuiz()
    {
        mainMenuPanel.SetActive(false);
        quizPanel.SetActive(true);
    }

    public void ShowMenu()
    {
        quizPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}