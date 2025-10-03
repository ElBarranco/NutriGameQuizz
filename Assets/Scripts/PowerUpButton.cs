using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

public class PowerUpButton : MonoBehaviour
{
    [BoxGroup("References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image icon;
    [SerializeField] private PowerUpFeedback feedbackEffect;

    [BoxGroup("Configuration")]
    [SerializeField] private PowerUpType type;
    [SerializeField] private RectTransform sourceTransform;

    [ReadOnly][SerializeField] private PowerUpButtonState currentState = PowerUpButtonState.Available;

    public PowerUpType Type => type;

    public void Init()
    {
        PowerUpManager.Instance.OnPowerUpInventoryChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (PowerUpManager.Instance != null)
            PowerUpManager.Instance.OnPowerUpInventoryChanged -= UpdateUI;
    }

    public void Btn_OnClick()
    {
        InteractionManager.Instance.TriggerMediumVibration();

        bool success = PowerUpManager.Instance.UsePowerUp(type, sourceTransform);
        if (!success)
        {
            Debug.Log($"[UI] No power-up of type {type} available.");
        }
    }

    private void UpdateUI()
    {
        int count = PowerUpManager.Instance.GetCount(type);
        quantityText.text = $"{count}";

        if (count <= 0)
        {
            SetState(PowerUpButtonState.Empty);
        }
        else
        {
            SetState(PowerUpButtonState.Available);
        }
    }

    public void SetState(PowerUpButtonState state)
    {
        currentState = state;

        switch (state)
        {
            case PowerUpButtonState.Available:
                button.interactable = true;
                SetIconAlpha(1f);
                break;

            case PowerUpButtonState.Unavailable:
                button.interactable = false;
                SetIconAlpha(0.3f);
                break;

            case PowerUpButtonState.Empty:
                button.interactable = false;
                SetIconAlpha(0f);
                break;
        }
    }


    public void PlayFeedback(int amount)
    {

        feedbackEffect.Play(amount);

    }
    private void SetIconAlpha(float alpha)
    {

        Color color = icon.color;
        color.a = alpha;
        icon.color = color;

    }

    public RectTransform GetSourceTransform() => sourceTransform;
}