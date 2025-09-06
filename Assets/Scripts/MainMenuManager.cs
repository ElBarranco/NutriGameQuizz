using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    public void Btn_Play()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        
        uiManager.ShowQuiz();
        GameManager.Instance.LaunchGame();
    }
    public void Btn_Play_Easy()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        
        uiManager.ShowQuiz();
        GameManager.Instance.LaunchGame(DifficultyLevel.Easy);
    }
    public void Btn_Play_Medium()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        
        uiManager.ShowQuiz();
        GameManager.Instance.LaunchGame(DifficultyLevel.Medium);
    }
    public void Btn_Play_Hard()
    {
        InteractionManager.Instance.TriggerMediumVibration();
        
        uiManager.ShowQuiz();
        GameManager.Instance.LaunchGame(DifficultyLevel.Hard);
    }
}