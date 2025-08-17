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
}