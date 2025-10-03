using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;


public class StreakGaugeUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private Image _fillImage;
    [SerializeField] private StreakRewardVisual _rewardVisual;
    [SerializeField] private PowerUpType _rewardedPowerUpType = PowerUpType.Hint;

    [Header("Réglages")]
    [SerializeField, Min(1)] private int _maxStreakForFull = 4;
    [Tooltip("Déclencher la récompense à chaque multiple du palier (ex: 4, 8, 12...), pas seulement la 1ère fois.")]
    [SerializeField] private bool _rewardEachMultiple = false;

    [Header("Debug ")]
    [ReadOnly, SerializeField] private int _currentStreak;
    [ReadOnly, SerializeField] private int _bestStreak;
    [ReadOnly, SerializeField] private float _fill01;

    // interne
    private int _lastRewardedMultiple = 0; // 0 = aucune récompense encore donnée



    private void Awake()
    {

        ResetGauge();

    }

    public void ResetGauge()
    {
        _currentStreak = 0;
        _bestStreak = 0;
        _fill01 = 0f;
        _lastRewardedMultiple = 0;

        _fillImage.fillAmount = 0f;
    }

    private void OnEnable()
    {
        _scoreManager.OnStreakChange += HandleStreakChangedDelayed;

        // Init affichage direct (sans délai)
        HandleStreakChanged(_scoreManager.StreakActuel, _scoreManager.MeilleurStreak);
    }

    private void OnDisable()
    {
        _scoreManager.OnStreakChange -= HandleStreakChangedDelayed;
    }

    private int _pendingStreak;
    private int _pendingBest;

    private void HandleStreakChangedDelayed(int streakActuel, int meilleurStreak)
    {
        _pendingStreak = streakActuel;
        _pendingBest = meilleurStreak;

        // Lance après 0.5s
        Invoke(nameof(ApplyStreakChange), 0.5f);
    }

    private void ApplyStreakChange()
    {
        HandleStreakChanged(_pendingStreak, _pendingBest);
    }


    private void HandleStreakChanged(int streakActuel, int meilleurStreak)
    {
        _currentStreak = streakActuel;
        _bestStreak = meilleurStreak;

        if (_maxStreakForFull <= 0) _maxStreakForFull = 1; // sécurité
        float targetFill = Mathf.Clamp01((float)_currentStreak / _maxStreakForFull);


        // On kill l’anim en cours pour éviter les conflits
        _fillImage.DOKill();

        _fillImage.DOFillAmount(targetFill, 0.22f).SetEase(Ease.OutQuad);

        _fill01 = targetFill;


        // Gestion de la récompense
        if (_currentStreak == 0)
        {
            _lastRewardedMultiple = 0;
            return;
        }

        if (_rewardEachMultiple)
        {
            int multiple = _currentStreak / _maxStreakForFull;
            if (multiple > 0 && multiple > _lastRewardedMultiple)
            {
                _lastRewardedMultiple = multiple;
                GrantReward();
            }
        }
        else
        {
            if (_currentStreak >= _maxStreakForFull && _lastRewardedMultiple == 0)
            {
                _lastRewardedMultiple = 1;
                GrantReward();
            }
        }
    }


    private void GrantReward()
    {
        PowerUpManager.Instance.AddPowerUp(_rewardedPowerUpType, 1);
        PowerUpButton targetButton = PowerUpManager.Instance.GetButtonForType(_rewardedPowerUpType);
        _rewardVisual.PlayEffectTowards(targetButton.GetSourceTransform());
    }
}