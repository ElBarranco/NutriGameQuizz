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
    }

    public void ShowHint()
    {
        if (hintActivated) return;

        hintActivated = true;

        SignHintPanel.SetActive(true);
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