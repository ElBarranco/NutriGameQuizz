using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;


public class StreakGaugeUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private Image _fillImage;

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

        _scoreManager.OnStreakChange += HandleStreakChanged;
        // Init affichage avec l'état courant si nécessaire
        HandleStreakChanged(_scoreManager.StreakActuel, _scoreManager.MeilleurStreak);

    }

    private void OnDisable()
    {

        _scoreManager.OnStreakChange -= HandleStreakChanged;

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
        // TODO: Implémenter ce que gagne le joueur (ex: +100 pts / +1 joker / +1 vie / confettis, etc.)
        // L’UI se contente de détecter l’atteinte du palier via le streak.
    }
}