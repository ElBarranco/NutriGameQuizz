using UnityEngine;
using NaughtyAttributes;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Statistiques")]
    [ReadOnly, SerializeField] private int bonnesReponses;
    [ReadOnly, SerializeField] private int mauvaisesReponses;
    [ReadOnly, SerializeField] private int score;

    [Header("Streak")]
    [ReadOnly, SerializeField] private int streakActuel;
    [ReadOnly, SerializeField] private int meilleurStreak;

    [Header("Config Scoring")]
    [SerializeField, Min(1)] private int pointsParBonneReponse = 10;
    [SerializeField, Min(1)] private int pointsParBonneReponseRecycling = 2;
    [SerializeField] private bool utiliserMultiplicateur = true;

    [ShowIf(nameof(utiliserMultiplicateur))]
    [SerializeField, Min(1)] private int palierStreakPourMultiplicateur = 3;

    [ShowIf(nameof(utiliserMultiplicateur))]
    [SerializeField, Min(1)] private int multiplicateurMax = 3;
    [SerializeField] private RewardFXController rewardFX;

    // Événements
    public event Action<int> OnScoreChange;
    public event Action<int, int> OnStreakChange;
    public event Action<int> OnMultiplicateurChange;

    // Accesseurs publics
    public int BonnesReponses => bonnesReponses;
    public int MauvaisesReponses => mauvaisesReponses;
    public int Score => score;
    public int StreakActuel => streakActuel;
    public int MeilleurStreak => meilleurStreak;

    public int MultiplicateurEffectif
    {
        get
        {
            if (!utiliserMultiplicateur || palierStreakPourMultiplicateur <= 0)
                return 1;

            int multiplicateur = 1 + (streakActuel / palierStreakPourMultiplicateur);
            return Mathf.Clamp(multiplicateur, 1, multiplicateurMax);
        }
    }

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

    public void EnregistrerRecyclingAnswer(bool isCorrect)
    {
        score += pointsParBonneReponseRecycling;
        rewardFX.PlayForAnswer(isCorrect, false, 1);
    }
    public void EnregistrerReponse(bool isCorrect, bool isPerfect = false)
    {
        if (isCorrect)
        {
            EnregistrerBonneReponse();
        }
        else
        {
            EnregistrerMauvaiseReponse();
        }
        rewardFX.PlayForAnswer(isCorrect, isPerfect);
    }
    public void EnregistrerBonneReponse()
    {
        bonnesReponses++;
        streakActuel++;
        if (streakActuel > meilleurStreak) meilleurStreak = streakActuel;

        int gain = pointsParBonneReponse * MultiplicateurEffectif;
        score += gain;

        OnStreakChange?.Invoke(streakActuel, meilleurStreak);
        OnMultiplicateurChange?.Invoke(MultiplicateurEffectif);
        OnScoreChange?.Invoke(score);
    }

    public void EnregistrerMauvaiseReponse()
    {
        mauvaisesReponses++;
        bool avaitStreak = streakActuel > 0;
        streakActuel = 0;

        if (avaitStreak)
        {
            OnStreakChange?.Invoke(streakActuel, meilleurStreak);
            OnMultiplicateurChange?.Invoke(MultiplicateurEffectif);
        }

        OnScoreChange?.Invoke(score); // Permet de forcer le refresh du HUD
    }

    public void Reinitialiser()
    {
        bonnesReponses = 0;
        mauvaisesReponses = 0;
        score = 0;
        streakActuel = 0;
        meilleurStreak = 0;

        OnStreakChange?.Invoke(streakActuel, meilleurStreak);
        OnMultiplicateurChange?.Invoke(MultiplicateurEffectif);
        OnScoreChange?.Invoke(score);
    }

    public int GetScore()
    {
        return score;
    }
}