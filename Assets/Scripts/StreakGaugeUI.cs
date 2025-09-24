using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;


public class StreakGaugeUI : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private Image _fillImage; // Mettre Type = Filled (Horizontal ou Radial)

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

        // Mise à jour de la jauge : clamp entre 0 et 1
        if (_maxStreakForFull <= 0) _maxStreakForFull = 1; // sécurité
        _fill01 = Mathf.Clamp01((float)_currentStreak / _maxStreakForFull);

        if (_fillImage != null)
            _fillImage.fillAmount = _fill01;

        // Gestion de la récompense
        if (_currentStreak == 0)
        {
            // reset des verrous de récompense quand on casse la série
            _lastRewardedMultiple = 0;
            return;
        }

        if (_rewardEachMultiple)
        {
            // Calcule le multiple atteint (ex: 4 -> 1, 8 -> 2, 9 -> 2, 12 -> 3…)
            int multiple = _currentStreak / _maxStreakForFull;
            if (multiple > 0 && multiple > _lastRewardedMultiple)
            {
                _lastRewardedMultiple = multiple;
                GrantReward();
            }
        }
        else
        {
            // Version simple : on récompense une fois quand on atteint le palier, puis plus rien tant que la série ne retombe pas à 0
            if (_currentStreak >= _maxStreakForFull && _lastRewardedMultiple == 0)
            {
                _lastRewardedMultiple = 1; // lock jusqu'au prochain reset à 0
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