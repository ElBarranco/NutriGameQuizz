using UnityEngine;

public class HintDetector : MonoBehaviour
{
    [SerializeField] private GameObject SignHintPanel;
    [SerializeField] private RectTransform SignHintRect;
    private bool hintActivated = false;

    void Start()
    {

        HintController controller = PowerUpManager.Instance.GetHintController();
        controller.RegisterDetector(this);
        Debug.Log($"------- INIT DETECTOR -------");
    }

    public void ShowHint()
    {
        if (hintActivated) return;

        hintActivated = true;

        SignHintPanel.SetActive(true);
        Debug.Log("Hint affiché");
    }

    public RectTransform GetSignHintPanel()
    {
        return SignHintRect;
    }

    public bool IsHintActivated()
    {
        return hintActivated;
    }
}