using UnityEngine;
using System.Collections.Generic;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    [SerializeField] private HintController hintController;
    [SerializeField] private List<PowerUpButton> boutons = new();

    private Dictionary<PowerUpType, int> inventory = new Dictionary<PowerUpType, int>();
    public event System.Action OnPowerUpInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        foreach (PowerUpType type in System.Enum.GetValues(typeof(PowerUpType)))
            AddPowerUp(type, 3);

        foreach (var b in boutons)
            b.Init();

        UpdateAllButtonStates(); // initial state update
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.H))
        {
            foreach (PowerUpType type in System.Enum.GetValues(typeof(PowerUpType)))
            {
                AddPowerUp(type, 5);
            }
        }
#endif
    }

    public void AddPowerUp(PowerUpType type, int amount = 1)
    {
        if (!inventory.ContainsKey(type))
            inventory[type] = 0;

        inventory[type] += amount;

        TriggerGainFeedback(type, amount);

        OnPowerUpInventoryChanged?.Invoke();
        UpdateButtonState(type);
    }

    public bool UsePowerUp(PowerUpType type, RectTransform buttonOrigin = null)
    {
        if (inventory.ContainsKey(type) && inventory[type] > 0)
        {
            inventory[type]--;
            TriggerPowerUpEffect(type, buttonOrigin);
            OnPowerUpInventoryChanged?.Invoke();
            UpdateButtonState(type);
            return true;
        }
        return false;
    }

    private void TriggerPowerUpEffect(PowerUpType type, RectTransform buttonOrigin = null)
    {
        switch (type)
        {
            case PowerUpType.Hint:
                hintController.ActivateHint(buttonOrigin);
                break;

            case PowerUpType.Skip:
                Debug.Log("💥 Skip activé : passer à la question suivante");
                break;
        }
    }

    private void TriggerGainFeedback(PowerUpType type, int amount)
    {
        foreach (PowerUpButton button in boutons)
        {
            if (button.Type == type)
            {
                button.PlayFeedback(amount);
            }
        }
    }

    public int GetCount(PowerUpType type)
    {
        return inventory.ContainsKey(type) ? inventory[type] : 0;
    }

    public void UpdateHintAvailability(bool hasDetector)
    {
        foreach (var b in boutons)
        {
            if (b.Type == PowerUpType.Hint)
            {
                int count = GetCount(PowerUpType.Hint);

                if (!hasDetector)
                    b.SetState(PowerUpButtonState.Unavailable);
                else if (count <= 0)
                    b.SetState(PowerUpButtonState.Empty);
                else
                    b.SetState(PowerUpButtonState.Available);
            }
        }
    }

    private void UpdateAllButtonStates()
    {
        foreach (var b in boutons)
        {
            UpdateButtonState(b.Type);
        }
    }

    private void UpdateButtonState(PowerUpType type)
    {
        foreach (var b in boutons)
        {
            if (b.Type != type)
                continue;

            int count = GetCount(type);

            if (type == PowerUpType.Hint)
            {
                // defer to HintController logic
                UpdateHintAvailability(hintController.HasAvailableDetectors());
                return;
            }

            if (count <= 0)
                b.SetState(PowerUpButtonState.Empty);
            else
                b.SetState(PowerUpButtonState.Available);
        }
    }

    public HintController GetHintController()
    {
        return hintController;
    }

    public PowerUpButton GetButtonForType(PowerUpType type)
    {
        foreach (var button in boutons)
        {
            if (button.Type == type)
                return button;
        }
        return null;
    }
}